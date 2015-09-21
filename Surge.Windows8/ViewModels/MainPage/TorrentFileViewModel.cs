// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;

using Surge.Core.Models;
using Surge.Shared.Common.ErrorTracking;

namespace Surge.Windows8.ViewModels.MainPage
{
    public class TorrentFileViewModel : ViewModel
    {
        private int _id;
        private IEventAggregator _eventAggregator;
        private ErrorTracker _errorTracker;

        public TorrentFileViewModel(int id, IEventAggregator eventAggregator, ErrorTracker errorTracker)
        {
            _id = id;
            _errorTracker = errorTracker;
            _eventAggregator = eventAggregator;
            Files = new ObservableCollection<ItemViewModel>();
        }

        public ObservableCollection<ItemViewModel> Files { get; private set; }

        public void Update(IEnumerable<Item> items)
        {
            var itemList = items.ToList();
            if (Files.Count == 0)
            {
                // Add all of the files to the list in the right column and with the correct parents
                foreach (var item in itemList)
                {
                    FolderViewModel parent = null;
                    if (item.HasParent)
                    {
                        parent = Files[itemList.IndexOf(item.Parent)] as FolderViewModel;
                    }

                    ItemViewModel itemToAdd;
                    if (item is File)
                    {
                        itemToAdd = ItemViewModel.Create(item as File, parent, _id, _eventAggregator);
                    }
                    else
                    {
                        itemToAdd = ItemViewModel.Create(item as Folder, parent, _id, _eventAggregator);
                    }

                    Files.Add(itemToAdd);
                }

                // Iterate through each folder and add its new ItemVM children to itself
                // so that we can navigate downwards
                for (int i = 0; i < Files.Count; ++i)
                {
                    if (!Files[i].IsFolder)
                    {
                        continue;
                    }

                    var folder = Files[i] as FolderViewModel;
                    var itemListFolder = itemList[i] as Folder;
                    var childrenIDs = itemListFolder.Children.Select(x => itemList.IndexOf(x));

                    foreach (var child in childrenIDs)
                    {
                        folder.Children.Add(Files[child]);
                    }

                    folder.Update();
                }
            }
            else
            {
                foreach (var item in Files)
                {
                    if (!item.IsFolder)
                    {
                        try
                        {
                            (item as FileViewModel).Update(itemList[(item as FileViewModel).Id] as File);
                        }
                        catch (Exception e)
                        {
                            // TODO: Getting errors here.
                            // 4100 times total at time of commit.
                            // The data that gets here is completely wrong.
                            // Either it's the data for the wrong torrent (unlikely),
                            // or the file algorithm in Surge.Core is broken.
                            // I _know_ the algorithm is broken, so until it's fixed, not
                            // doing anything here (but leaving tracking so that once I've reworked
                            // the file algorithm, I'll know if it's fixed or not)
                            _errorTracker.Send(e,
                                              "Files Count: " + Files.Count,
                                              "Index: " + (item as FileViewModel).Id,
                                              "ItemList Count: " + itemList.Count);
                        }
                    }
                }

                foreach (var item in Files.Reverse())
                {
                    if (item.IsFolder)
                    {
                        (item as FolderViewModel).Update();
                    }
                }
            }
        }
    }
}
