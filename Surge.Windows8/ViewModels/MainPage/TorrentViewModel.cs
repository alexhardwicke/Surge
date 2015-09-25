// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;

using Surge.Core.Models;
using Surge.Shared.Common;
using Surge.Shared.Common.ErrorTracking;

namespace Surge.Windows8.ViewModels.MainPage
{
    public class TorrentViewModel : ViewModel
    {
        private double _percentValue;
        private double _verifiedPercentValue;
        private double _magnetResolvedPercentValue;
        private string _name;
        private string _size;
        private bool _isPaused;
        private bool _isVerifying;
        private bool _isComplete;
        private bool _isActive;
        private bool _isMagnetResolving;
        private string _downloadSpeed;
        private string _uploadSpeed;
        private string _availability;
        private string _remainingTime;
        private string _downloaded;
        private string _uploaded;
        private string _runningTime;
        private string _lastActivity;
        private string _error;
        private string _comment;
        private string _percent;
        private string _ratio;
        private string _location;
        private long _queue;

        public TorrentViewModel(Torrent torrent, IEventAggregator eventAggregator, ErrorTracker errorTracker, ServerUnits speedUnits, ServerUnits sizeUnits)
        {
            Id = torrent.ID;
            TorrentFileViewModel = new TorrentFileViewModel(Id, eventAggregator, errorTracker);
            Size = torrent.Size.ToSizeString(sizeUnits);
            Hash = torrent.Hash;
            Update(torrent, speedUnits, sizeUnits);
        }

        public bool IsUserModified { get; set; }
        public int Id { get; set; }
        public string Hash { get; set; }

        public string Size
        {
            get
            {
                return _size;
            }
            set
            {
                SetProperty(ref _size, value);
            }
        }

        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                SetProperty(ref _isActive, value);
            }
        }

        public bool IsComplete
        {
            get
            {
                return _isComplete;
            }
            set
            {
                SetProperty(ref _isComplete, value);
            }
        }

        public bool IsVerifying
        {
            get
            {
                return _isVerifying;
            }
            set
            {
                SetProperty(ref _isVerifying, value);
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(Progress));
            }
        }

        public bool IsMagnetResolving
        {
            get
            {
                return _isMagnetResolving;
            }
            set
            {
                SetProperty(ref _isMagnetResolving, value);
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(Progress));
            }
        }

        public long Queue
        {
            get
            {
                return _queue;
            }
            set
            {
                SetProperty(ref _queue, value);
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                SetProperty(ref _name, value);
            }
        }

        public string Location
        {
            get
            {
                return _location;
            }
            set
            {
                SetProperty(ref _location, value);
            }
        }

        public bool IsPaused
        {
            get
            {
                return _isPaused;
            }
            set
            {
                SetProperty(ref _isPaused, value);
                OnPropertyChanged(nameof(Status));
            }
        }

        public string DownloadSpeed
        {
            get
            {
                return _downloadSpeed;
            }
            set
            {
                SetProperty(ref _downloadSpeed, value);
            }
        }

        public string UploadSpeed
        {
            get
            {
                return _uploadSpeed;
            }
            set
            {
                SetProperty(ref _uploadSpeed, value);
            }
        }

        public string Availability
        {
            get
            {
                return _availability;
            }
            set
            {
                SetProperty(ref _availability, value);
                OnPropertyChanged(nameof(IsUnavailable));
                OnPropertyChanged(nameof(ProgressState));
            }
        }

        public string RemainingTime
        {
            get
            {
                return _remainingTime;
            }
            set
            {
                SetProperty(ref _remainingTime, value);
            }
        }

        public string Downloaded
        {
            get
            {
                return _downloaded;
            }
            set
            {
                SetProperty(ref _downloaded, value);
            }
        }

        public string Uploaded
        {
            get
            {
                return _uploaded;
            }
            set
            {
                SetProperty(ref _uploaded, value);
            }
        }

        public string RunningTime
        {
            get
            {
                return _runningTime;
            }
            set
            {
                SetProperty(ref _runningTime, value);
            }
        }

        public string LastActivity
        {
            get
            {
                return _lastActivity;
            }
            set
            {
                SetProperty(ref _lastActivity, value);
            }
        }

        public string Error
        {
            get
            {
                return _error;
            }
            set
            {
                SetProperty(ref _error, value);
                OnPropertyChanged(nameof(HasError));
                OnPropertyChanged(nameof(ProgressState));
            }
        }

        public string Comment
        {
            get
            {
                return _comment;
            }
            set
            {
                SetProperty(ref _comment, value);
            }
        }

        public string Percent
        {
            get
            {
                return _percent;
            }
            set
            {
                SetProperty(ref _percent, value);
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(ProgressState));
            }
        }

        public string Ratio
        {
            get
            {
                return _ratio;
            }
            set
            {
                SetProperty(ref _ratio, value);
            }
        }

        public string ProgressState
        {
            get
            {
                if (IsVerifying)
                {
                    return "Verifying";
                }
                else if (IsMagnetResolving)
                {
                    return "Resolving Magnet";
                }
                else if (HasError || IsUnavailable)
                {
                    return "Error";
                }
                else if (_percentValue < 1.0)
                {
                    return "Incomplete";
                }
                else
                {
                    return "Complete";
                }
            }
        }

        public bool HasError
        {
            get
            {
                return Error != "None";
            }
        }

        public bool IsUnavailable
        {
            get
            {
                return Availability != "100%";
            }
        }

        public double Progress
        {
            get
            {
                if (IsVerifying)
                {
                    return _verifiedPercentValue;
                }
                else if (IsMagnetResolving)
                {
                    return _magnetResolvedPercentValue;
                }
                else
                {
                    return _percentValue;
                }
            }
        }

        public string Status
        {
            get
            {
                if (IsVerifying)
                {
                    return "Verifying";
                }
                else if (IsMagnetResolving)
                {
                    return "Magnetized Transfer" + (_isPaused || HasError || IsUnavailable ? "" : " - Resolving");
                }
                else if (IsPaused)
                {
                    return "Paused";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public bool IsDownloading
        {
            get
            {
                return !IsComplete && !IsPaused;
            }
        }

        public bool IsSeeding
        {
            get
            {
                return IsComplete && !IsPaused && !IsVerifying;
            }
        }

        public TorrentFileViewModel TorrentFileViewModel { get; set; }

        public void Update(Torrent torrent, ServerUnits speedUnits, ServerUnits sizeUnits)
        {
            // We only update certain properties if the user has a clean object.
            // Otherwise we ignore the change this time, but clear the flag.
            // The reason for this is that when pausing/verifying, the UI
            // is updated instantly (don't wait in a server response).
            if (!IsUserModified)
            {
                IsUserModified = false;
                IsPaused = torrent.IsPaused;
                IsVerifying = torrent.IsVerifying;
            }

            _percentValue = torrent.Percent;
            _magnetResolvedPercentValue = torrent.MagnetResolvedPercent;
            _verifiedPercentValue = torrent.VerifiedPercent;
            IsMagnetResolving = torrent.IsMagnetResolving;
            DownloadSpeed = torrent.DownloadSpeed.ToSizeString(speedUnits);
            UploadSpeed = torrent.UploadSpeed.ToSizeString(speedUnits);
            Availability = torrent.Availability.ToPercent(torrent.Size);
            RemainingTime = torrent.RemainingTime.ToTimeString();
            Downloaded = torrent.Downloaded.ToSizeString(sizeUnits);
            Uploaded = torrent.Uploaded.ToSizeString(sizeUnits);
            RunningTime = torrent.RunningTime.ToTimeString();
            LastActivity = torrent.LastActivity.ToTimeString();
            Percent = torrent.Percent.ToPercent();
            Ratio = torrent.Ratio.ToRatioString();
            Error = torrent.Error.ToNoneIfEmpty();
            Comment = torrent.Comment.ToNoneIfEmpty();
            IsComplete = torrent.Percent == 1.0;
            IsActive = torrent.DownloadSpeed > 0 || torrent.UploadSpeed > 0;
            Queue = torrent.QueuePosition;
            Location = torrent.Location;

            if (torrent.Name != string.Empty)
            {
                Name = torrent.Name;
            }

            if (torrent.Size > 0)
            {
                Size = torrent.Size.ToSizeString(sizeUnits);
            }

            TorrentFileViewModel.Update(torrent.Files, sizeUnits);
        }
    }
}
