// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;

using Surge.Core.Network;
using Surge.Shared.Common;

namespace Surge.Windows8.ViewModels.MainPage
{
    public class ServerViewModel : ViewModel
    {
        private string _downloadSpeed;
        private string _uploadSpeed;
        private string _defaultDownloadLocation;
        private string _remainingSpace;
        private bool _isLoaded;
        private IEventAggregator _eventAggregator;

        public ServerViewModel(IEventAggregator eventAggregator, Server server)
        {
            _eventAggregator = eventAggregator;
            SetServer(server);
        }

        public void SetServer(Server server)
        {
            URL = server.URL;
            _defaultDownloadLocation = "";
        }

        public string URL { get; set; }

        public bool IsLoaded
        {
            get
            {
                return _isLoaded;
            }
            set
            {
                SetProperty(ref _isLoaded, value);
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

        public string DefaultDownloadLocation
        {
            get
            {
                return _defaultDownloadLocation;
            }
            set
            {
                SetProperty(ref _defaultDownloadLocation, value);
            }
        }

        public string RemainingSpace
        {
            get
            {
                return _remainingSpace;
            }
            set
            {
                SetProperty(ref _remainingSpace, value);
            }
        }

        public void Update(UpdateData updateData)
        {
            DownloadSpeed = updateData.ServerStats.DownloadSpeed.ToSpeedString(updateData.ServerStats.SpeedUnits);
            UploadSpeed = updateData.ServerStats.UploadSpeed.ToSpeedString(updateData.ServerStats.SpeedUnits);
            DefaultDownloadLocation = updateData.ServerStats.DefaultDownloadLocation;
            RemainingSpace = updateData.ServerStats.SpaceRemaining.ToSizeString(updateData.ServerStats.SizeUnits);

            if (!IsLoaded)
            {
                IsLoaded = true;
                _eventAggregator.GetEvent<ServerLoaded>().Publish(true);
            }
        }
    }
}
