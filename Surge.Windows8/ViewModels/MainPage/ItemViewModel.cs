// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using System.IO;

using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;

using Surge.Core.Models;
using Surge.Shared.Common;

namespace Surge.Windows8.ViewModels.MainPage
{
    public abstract class ItemViewModel : ViewModel
    {
        protected bool isUpdating;
        protected bool isIncomplete;
        protected int torrentId;
        protected IEventAggregator eventAggregator;
        private bool _isPropagatingUpdate;

        protected ItemViewModel(Item item, FolderViewModel parent, int torrentId, IEventAggregator eventAggregator)
        {
            Parent = parent;
            Name = item.Name;
            torrentId = torrentId;
            eventAggregator = eventAggregator;
        }

        internal bool PropagatingUpdate
        {
            get
            {
                return _isPropagatingUpdate;
            }
            set
            {
                _isPropagatingUpdate = value;
                if (this is FolderViewModel)
                {
                    (this as FolderViewModel).Children.ForEach(x => x.PropagatingUpdate = value);
                }
            }
        }

        public bool IsFolder { get { return this is FolderViewModel; } }
        public FolderViewModel Parent { get; private set; }
        public string Name { get; private set; }

        public int ParentDepth
        {
            get
            {
                var parent = Parent;
                int count = 0;
                while (parent != null)
                {
                    parent = parent.Parent;
                    count++;
                }

                return count;
            }
        }

        public FileType Type
        {
            get
            {
                if (IsFolder)
                {
                    return FileType.Folder;
                }
                else
                {
                    return Path.GetExtension(Name).GetFileType();
                }
            }
        }

        public abstract int Priority { get; set; }
        public abstract bool IsWanted { get; set; }

        public abstract string Detail { get; }
        public abstract bool IsIncomplete { get; set; }

        public static ItemViewModel Create(Folder folder, FolderViewModel parent, int torrentId, IEventAggregator eventAggregator)
        {
            return new FolderViewModel(folder, parent, torrentId, eventAggregator);
        }

        public static ItemViewModel Create(File file, FolderViewModel parent, int torrentId, IEventAggregator eventAggregator, ServerUnits sizeUnits)
        {
            return new FileViewModel(file, parent, torrentId, eventAggregator, sizeUnits);
        }
    }
}
