// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;

using Surge.Core.Network;
using Surge.Windows8.Views.Controls;

using Windows.UI.ApplicationSettings;

namespace Surge.Windows8.ViewModels.MainPage
{
    public class SettingsPaneViewModel : ViewModel
    {
        private INavigationService _navigationService;

        public SettingsPaneViewModel(INavigationService navigationService, SettingsViewModel settings)
        {
            _navigationService = navigationService;

            AboutCommand = new SettingsCommand("E0C01107-73FA-4F14-A9B6-49E637E7A509", "About", handler =>
            {
                var aboutFlyout = new About();
                aboutFlyout.Show();
            });

            ServerSettingsCommand = new SettingsCommand("A1A10948-C3D4-4692-AAE2-E59F891F06B0", "Server", (handler) =>
            {
                navigationService.Navigate("ServerConfiguration", ServerState.Working.ToString());
            });

            SettingsCommand = new SettingsCommand("4FDE3FF0-7A33-4B76-B8F6-A85221D99606", "Settings", handler =>
            {
                var settingsFlyout = new Settings(settings);
                settingsFlyout.Show();
            });
        }

        public SettingsCommand AboutCommand { get; private set; }
        public SettingsCommand ServerSettingsCommand { get; private set; }
        public SettingsCommand SettingsCommand { get; private set; }
    }
}
