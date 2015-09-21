// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using Surge.Core.Network;
using Surge.Shared.Common;
using Surge.Windows8.ViewModels.ServerConfigurationPage;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Surge.Windows8.Views
{
    public sealed partial class ServerConfigurationPage : BasePage
    {
        private string _URLText = "Here, you need to provide the URL or IP address of your server. Typically this will be the IP address of a device on your LAN (which often means it begins with 192.168).";
        private string _portText = "Here, you need to provide the port that can be used to connect to the server's remote protocol. Unless you have modified this, you can leave it at the default value.";
        private string _usernameText = "Here, you need to provide the username configured in your torrent client, if any.";
        private string _passwordText = "Here, you need to provide the password configured in your torrent client, if any." + Environment.NewLine + Environment.NewLine +
                                      "Please note that due to the nature of the torrent client's API, this password can be broadcast in plaintext - which means you should be careful on public networks.";

        public ServerConfigurationPage()
        {
            InitializeComponent();
            SizeChanged += WindowSizeChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var scvm = DataContext as ServerConfigurationPageViewModel;
            if (e.Parameter is string)
            {
                scvm.Initialise(ServerState.FromString(e.Parameter as string));
            }
            else
            {
                scvm.Initialise(ServerState.Unconfigured);
            }

            DataContext = scvm;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            DataContext = null;
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ActualWidth < 770)
            {
                SidebarGridWidth.Width = new GridLength(ActualWidth);
                VisualStateManager.GoToState(this, "Narrow", true);
            }
            else
            {
                SidebarGridWidth.Width = new GridLength(500);
                VisualStateManager.GoToState(this, "Expanded", true);
            }
        }

        private void SetText(string title, string text)
        {
            SelectedTitle.Text = title;
            SelectedInfo.Text = text;
        }

        private void FocusChange(object sender, RoutedEventArgs e)
        {
            if ((e.OriginalSource as Control).FocusState == FocusState.Unfocused)
            {
                return;
            }

            var name = (sender as Control).Name.ToString();
            switch (name)
            {
                case "URL":
                    SetText(name, _URLText);
                    break;
                case "Port":
                    SetText(name, _portText);
                    break;
                case "Username":
                    SetText(name, _usernameText);
                    break;
                case "Password":
                    SetText(name, _passwordText);
                    break;
            }
        }

        private void BackButtonClicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}