// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using Microsoft.Practices.Prism.PubSubEvents;

using Surge.Core.Models;
using Surge.Shared.Common;

namespace Surge.Windows8.ViewModels.MainPage
{
    public class FileViewModel : ItemViewModel
    {
        private long _bytesCompleted;
        private bool _isWanted;
        private int _priority;
        private ServerUnits _sizeUnits;

        public FileViewModel(File file, FolderViewModel parent, int torrentId, IEventAggregator eventAggregator, ServerUnits sizeUnits)
            : base(file, parent, torrentId, eventAggregator)
        {
            _sizeUnits = sizeUnits;
            isUpdating = true;
            Priority = file.Priority + 1;
            IsWanted = file.IsWanted;
            Size = file.FileSize;
            BytesCompleted = file.BytesCompleted;
            Id = file.Id;
            isUpdating = false;
        }

        public int Id { get; set; }
        public bool IsUserModified { get; set; }

        public override int Priority
        {
            get
            {
                return _priority;
            }
            set
            {
                SetProperty(ref _priority, value);

                if (PropagatingUpdate)
                {
                    return;
                }

                if (!isUpdating)
                {
                    IsUserModified = true;
                    eventAggregator.GetEvent<PriorityChanged>().Publish(new PriorityChangedData(torrentId, Id, value));

                    if (Parent != null)
                    {
                        Parent.Update();
                    }
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

                if (PropagatingUpdate)
                {
                    return;
                }

                if (!isUpdating)
                {
                    IsUserModified = true;
                    eventAggregator.GetEvent<WantedChanged>().Publish(new WantedChangedData(torrentId, Id, value));

                    if (Parent != null)
                    {
                        Parent.Update();
                    }
                }
            }
        }

        public long BytesCompleted
        {
            get
            {
                return _bytesCompleted;
            }
            set
            {
                SetProperty(ref _bytesCompleted, value);
                IsIncomplete = _bytesCompleted != Size;
                OnPropertyChanged(nameof(Detail));
            }
        }

        public long Size { get; set; }

        public override string Detail
        {
            get
            {
                return BytesCompleted.ToFileDetailsString(Size, _sizeUnits);
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

        public void Update(Item item, ServerUnits sizeUnits)
        {
            _sizeUnits = sizeUnits;

            var file = item as File;

            BytesCompleted = file.BytesCompleted;

            isUpdating = true;

            if (!IsUserModified)
            {
                Priority = file.Priority + 1;
                IsWanted = file.IsWanted;
            }

            isUpdating = false;
            IsUserModified = false;
        }
    }
}
