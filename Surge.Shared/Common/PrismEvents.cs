// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Practices.Prism.PubSubEvents;

namespace Surge.Shared.Common
{
    public class FilterChanged : PubSubEvent<Filter> { }
    public class ServerLoaded : PubSubEvent<bool> { }
    public class UseDefaultDownloadSettingChanged : PubSubEvent<bool> { }
    public class OrderByQueueSettingChanged : PubSubEvent<bool> { }
    public class SearchCleared : PubSubEvent<bool> { }
    public class SearchChanged : PubSubEvent<string> { }
    public class MoveTorrents : PubSubEvent<TorrentMovedData> { }
    public class DeleteTorrents : PubSubEvent<TorrentDeletedData> { }
    public class AddTorrent : PubSubEvent<TorrentAddedData> { }
    public class PriorityChanged : PubSubEvent<PriorityChangedData> { }
    public class WantedChanged : PubSubEvent<WantedChangedData> { }
    public class URIActivated : PubSubEvent<string> { }
    public class FileActivated : PubSubEvent<IEnumerable<TorrentFileData>> { }

    public class PriorityChangedData
    {
        public PriorityChangedData(int torrentId, int item, int priority) : this(torrentId, new List<int>() { item }, priority) { }

        public PriorityChangedData(int torrentID, IEnumerable<int> itemIDs, int priority)
        {
            TorrentID = torrentID;
            ItemIDs = itemIDs;
            Priority = priority;
        }

        public IEnumerable<int> ItemIDs { get; }
        public int TorrentID { get; }
        public int Priority { get; }
    }

    public class WantedChangedData
    {
        public WantedChangedData(int torrentID, int itemID, bool isWanted) : this(torrentID, new List<int>() { itemID }, isWanted) { }

        public WantedChangedData(int torrentID, IEnumerable<int> itemIDs, bool isWanted)
        {
            TorrentID = torrentID;
            ItemIDs = itemIDs;
            IsWanted = isWanted;
        }

        public IEnumerable<int> ItemIDs { get; }
        public int TorrentID { get; }
        public bool IsWanted { get; }
    }

    public class TorrentMovedData
    {
        public TorrentMovedData(string location, bool moveFiles, IEnumerable<int> torrentIDs)
        {
            Location = location;
            MoveFiles = moveFiles;
            TorrentIDs = torrentIDs;
        }

        public IEnumerable<int> TorrentIDs;
        public string Location { get; }
        public bool MoveFiles { get; }
    }

    public class TorrentDeletedData
    {
        public TorrentDeletedData(bool keepFiles, IEnumerable<int> torrentIDs)
        {
            KeepFiles = keepFiles;
            TorrentIDs = torrentIDs;
        }

        public IEnumerable<int> TorrentIDs;
        public string Location { get; }
        public bool KeepFiles { get; }
    }

    public class TorrentAddedData
    {
        public TorrentAddedData(string location)
        {
            Files = new List<string>();
            URIs = new List<string>();
            Location = location;
        }

        public List<string> Files { get; }

        public List<string> URIs { get; }

        public string Location { get; }
    }
}
