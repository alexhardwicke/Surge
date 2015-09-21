namespace Surge.Core.Transmission

module Files =
    open Surge.Core.Models
    open System.Collections.Generic

    let private getInsertIndex (item : Item) (genFiles : Item list) =
        let rec getChildrenCount (folder : Folder) (item : Item) =
            let folderChildren = Seq.length folder.Children
            let folderChildrenCount = Seq.sum (query { for child in folder.Children do
                                                       where (child :? Folder && child <> item)
                                                       select (getChildrenCount(child :?> Folder) item) })
            folderChildren + folderChildrenCount

        match item.HasParent with
        | false -> 0
        | true -> let parentIndex = genFiles |> List.findIndex (fun e ->
                                                                    match e : Item with
                                                                    | :? Folder as f -> f = item.Parent
                                                                    | _ -> false)

                  let childrenCount = getChildrenCount item.Parent item
                  parentIndex - childrenCount + 1

    let private tempStrangeDataFail query =
        let rec buildParentTree (item : Item) currentPath =
            let myString = if item.HasParent then buildParentTree item.Parent currentPath
                            else ""
            myString + item.Name
                            
        let rec genListData (currList : Item list) (currentString : string) =
            if List.isEmpty currList then currentString
            else
                let newString = currentString + (buildParentTree currList.Head "")
                genListData currList.Tail newString

        let list = Seq.toList query
        let errorString = genListData list ""
        raise (System.Exception("Multiple identical folders? " + errorString))

    let private getOrGenFolder id (genFiles : Item list) path pos : (Folder * bool) =
        let rec checkFolderParents (folder : Folder) (path : string[]) pos : bool =
            match pos < 0 with
            | true -> false
            | false ->
                if path.[pos] = folder.Name then
                    if pos = 0 then true
                    else if folder.HasParent then checkFolderParents folder.Parent path (pos - 1)
                    else false
                else false

        let genFolder id (path : string[]) pos =
            Folder(id, path.[pos], None)

        if pos < 0 then (genFolder id path pos, true)
        else
            let searchResult : Item list = List.filter (fun e -> e.Name = path.[pos] && e :? Folder) genFiles
            if searchResult.IsEmpty then (genFolder id path pos, true)
            else
                let query = query { for item in searchResult do
                                    where (checkFolderParents (item :?> Folder) path pos)
                                    select item }

                if Seq.isEmpty query then (genFolder id path pos, true)
                // TODO: This is happening: I'm getting multiple things that perfectly match
                // The algorithm is clearly broken. Figure out why and fix.
                // Error reports show that there are 2 or 3 instances when it fails.
                else if Seq.length query > 1 then tempStrangeDataFail query
                else (Seq.exactlyOne query :?> Folder, false)

    let rec private genStructure count id (files : list<TorrentGet.File * TorrentGet.FileStat>) (genFiles : Item list) i =
        if i < 0 then List.rev genFiles
        else
            let jsonFile = fst files.[i]
            let stats = snd files.[i]
            let path = jsonFile.Name.Split('/')
            let rec generateFileOrFolder (child : (Item * bool) option) files j =
                let item =
                    if (j+1) = path.Length then
                        let pos = count - i
                        let name = path.[j]
                        File(id, pos, stats.Priority, stats.Wanted, stats.BytesCompleted, name, jsonFile.Length, None) :> Item, true
                    else
                        let folder, newFolder = getOrGenFolder id genFiles path j
                        let childItem, newChild = child.Value
                        if newChild then
                            childItem.MutableParent <- folder
                            folder.MutableChildren <- childItem :: folder.MutableChildren
                        else ()
                        (folder :> Item, newFolder)

                match j = 0 with
                | true -> item :: files
                | false -> generateFileOrFolder (Some item) (item :: files) (j - 1)

            let newItems = generateFileOrFolder None [] (path.Length - 1)
            let toAddItems = query { for tuple in newItems do
                                     where (snd tuple)
                                     select (fst tuple) }
                             |> Seq.toList
                             |> List.rev

            let insertIndex = getInsertIndex (match Seq.length toAddItems with
                                             | 1 -> toAddItems.Head
                                             | _ -> (List.rev toAddItems).Head)
                                             genFiles

            let newList = if insertIndex = 0 then
                              toAddItems @ genFiles
                          else
                              let rec addRec items gen =
                                  let item = Seq.head items
                                  let list = gen |> List.mapi (fun i el -> if i = insertIndex then [item; el] else [el])
                                                 |> List.concat
                                  if Seq.length items = 1 then list
                                  else addRec (Seq.skip 1 items) list
                              addRec (List.rev toAddItems) genFiles

            genStructure count id files newList (i - 1)

    let private genFile torrentid count i (file : TorrentGet.File * TorrentGet.FileStat) =
        let f = fst file
        let s = snd file
        File(torrentid, count - i, s.Priority, s.Wanted, s.BytesCompleted, f.Name, f.Length, None) :> Item

    let internal deserialiseFiles torrentid genFiles (files : array<TorrentGet.File * TorrentGet.FileStat>) =
        if files |> Array.isEmpty then []
        else
            let count = Array.length files - 1
            let files' =
                files
                |> Array.rev
                |> Array.toList

            match genFiles with
            | true -> genStructure count torrentid files' [] count
            | false ->
                let fileGenerator = genFile torrentid count
                files' |> List.mapi fileGenerator