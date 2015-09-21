// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;

using Surge.Shared.Common;

namespace Surge.Windows8.ViewModels.MainPage
{
    public class SettingsViewModel : ViewModel
    {
        private bool _alwaysAskDownloadLocation;
        private bool _orderByQueue;
        private bool _showForceStart;
        private SettingsHelper _settingsHelper;
        private IEventAggregator _eventAggregator;

        public SettingsViewModel(IEventAggregator eventAggregator, SettingsHelper settingsHelper)
        {
            _eventAggregator = eventAggregator;
            _settingsHelper = settingsHelper;
            _alwaysAskDownloadLocation = settingsHelper.GetSetting<bool>(SettingType.AlwaysAskDownloadLocation);
            _orderByQueue = settingsHelper.GetSetting<bool>(SettingType.OrderByQueue);
            _showForceStart = settingsHelper.GetSetting<bool>(SettingType.ShowForceStart);
        }

        public bool AlwaysAskDownloadLocation
        {
            get
            {
                return _alwaysAskDownloadLocation;
            }
            set
            {
                _eventAggregator.GetEvent<UseDefaultDownloadSettingChanged>().Publish(!value);
                SetProperty(ref _alwaysAskDownloadLocation, value);
                _settingsHelper.SetSetting(SettingType.AlwaysAskDownloadLocation, value);
            }
        }

        public bool OrderByQueue
        {
            get
            {
                return _orderByQueue;
            }
            set
            {
                SetProperty(ref _orderByQueue, value);
                _settingsHelper.SetSetting(SettingType.OrderByQueue, value);
                _eventAggregator.GetEvent<OrderByQueueSettingChanged>().Publish(value);
            }
        }

        public bool ShowForceStart
        {
            get
            {
                return _showForceStart;
            }
            set
            {
                SetProperty(ref _showForceStart, value);
                _settingsHelper.SetSetting(SettingType.ShowForceStart, value);
            }
        }
    }
}
