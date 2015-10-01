// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Surge.Core.Models;
using Surge.Shared.Common;

using Microsoft.Practices.Prism.PubSubEvents;

namespace Surge.Windows8.ViewModels.MainPage
{
    public class FolderViewModel : ItemViewModel
    {
        private int _priority;
        private bool _isWanted;

        public FolderViewModel(Folder folder, FolderViewModel parent, int torrentId, IEventAggregator eventAggregator)
            : base(folder, parent, torrentId, eventAggregator)
        {
            isUpdating = true;
            Children = new ObservableCollection<ItemViewModel>();
            isUpdating = false;
        }

        public ObservableCollection<ItemViewModel> Children { get; private set; }

        public override int Priority
        {
            get
            {
                return _priority;
            }
            set
            {
                SetProperty(ref _priority, value);

                var isInitialUpdate = !PropagatingUpdate;

                if (!isUpdating)
                {
                    Children.Where(x => x is FileViewModel).ForEach(x => (x as FileViewModel).IsUserModified = true);
                    Children.ForEach(x => x.Priority = value);

                    if (!isInitialUpdate)
                    {
                        return;
                    }

                    PropagatingUpdate = true;

                    eventAggregator.GetEvent<PriorityChanged>().Publish(new PriorityChangedData(torrentId, ChildIDs, value));

                    var parent = Parent;
                    while (parent != null)
                    {
                        parent.Update();
                        parent = parent.Parent;
                    }
                    PropagatingUpdate = false;
                    Update();
                }
            }
        }

        public override bool IsWanted
        {
            get
            {
                return _isWanted;
            }
            set
            {
                SetProperty(ref _isWanted, value);

                var isInitialUpdate = !PropagatingUpdate;

                if (!isUpdating)
                {
                    Children.Where(x => x is FileViewModel).ForEach(x => (x as FileViewModel).IsUserModified = true);
                    Children.ForEach(x => x.IsWanted = value);

                    if (!isInitialUpdate)
                    {
                        return;
                    }

                    PropagatingUpdate = true;

                    eventAggregator.GetEvent<WantedChanged>().Publish(new WantedChangedData(torrentId, ChildIDs, value));

                    var parent = Parent;
                    while (parent != null)
                    {
                        parent.Update();
                        parent = parent.Parent;
                    }
                    PropagatingUpdate = false;
                    Update();
                }
            }
        }

        public override bool IsIncomplete
        {
            get
            {
                return isIncomplete;
            }
            set
            {
                SetProperty(ref isIncomplete, value);
            }
        }

        public override string Detail
        {
            get
            {
                return string.Empty;
            }
        }

        public void Update()
        {
            isUpdating = true;
            IsIncomplete = Children.Where(x => x.IsIncomplete).Count() > 0;
            IsWanted = Children.Where(x => x.IsWanted).Count() > 0;
            int numLow = Children.Where(x => x.Priority == 0).Count();
            int numMed = Children.Where(x => x.Priority == 1).Count();
            int numHigh = Children.Where(x => x.Priority == 2).Count();

            if (numLow > numMed && numLow > numHigh)
            {
                Priority = 0;
            }
            else if (numHigh > numMed)
            {
                Priority = 2;
            }
            else
            {
                // Default to normal priority
                Priority = 1;
            }

            isUpdating = false;
        }

        private IEnumerable<int> ChildIDs
        {
            get
            {
                var ids = Children.Where(x => x is FileViewModel).Select(x => (x as FileViewModel).Id).ToList();
                Children.Where(x => x is FolderViewModel).ForEach(x => ids.AddRange((x as FolderViewModel).ChildIDs));
                return ids;
            }
        }
    }
}
