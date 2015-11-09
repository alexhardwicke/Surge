// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using System;
using System.Collections.Generic;

using Surge.Shared.Common;
using Surge.Windows8.ViewModels.MainPage;

using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Surge.Windows8.Views
{
    public sealed partial class MainPage : BasePage
    {
        private MainPageViewModel _mainVM;
        private UIState _state;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Frame.KeyUp += HandleKey;
            Tapped += HandleTap;
            Torrents.TorrentSelectionChanged += HandleSelectionChange;
            BottomAppBar.Closed += HandleAppBarClose;
            SizeChanged += HandleSizeChange;
            Sidebar.IconTapped += HandleIconTap;
            Sidebar.SearchFocused += HandleSearchFocused;
            SettingsPane.GetForCurrentView().CommandsRequested += SettingsCommandsRequested;

            _mainVM = DataContext as MainPageViewModel;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SettingsPane.GetForCurrentView().CommandsRequested -= SettingsCommandsRequested;
            Frame.KeyUp -= HandleKey;
            Tapped -= HandleTap;
            Torrents.TorrentSelectionChanged -= HandleSelectionChange;
            BottomAppBar.Closed -= HandleAppBarClose;
            SizeChanged -= HandleSizeChange;
            Sidebar.IconTapped -= HandleIconTap;
            Sidebar.SearchFocused -= HandleSearchFocused;
        }

        private void HandleSearchFocused(object sender, object args)
        {
            GoToState(UIState.Expanded);
            _mainVM.IsFilePaneOpen = false;
        }

        private void HandleIconTap(object sender, object args)
        {
            var newState = UIState.Expanded;
            if (_state == UIState.Expanded)
            {
                newState = UIState.Narrow;
            }

            GoToState(newState);
        }

        private void HandleSizeChange(object sender, SizeChangedEventArgs args)
        {
            var newState = ActualWidth >= 1366 ? UIState.Expanded : UIState.Narrow;
            GoToState(newState);
        }

        private void HandleAppBarClose(object sender, object args)
        {
            if (!_mainVM.Server.IsLoaded)
            {
                return;
            }

            BottomAppBar.IsOpen = true;
        }

        private void HandleSelectionChange(object sender, int count)
        {
            if (count == 0) _mainVM.IsFilePaneOpen = false;
        }

        private void HandleTap(object sender, TappedRoutedEventArgs e)
        {
            _mainVM.IsFilePaneOpen = false;
        }

        private void HandleKey(object sender, KeyRoutedEventArgs e)
        {
            if (Sidebar.SearchHasFocus || TorrentWindow.IsOpen)
            {
                return;
            }

            if (e.Key == VirtualKey.Q)
            {
                Sidebar.FocusSearch();
            }
            else
            {
                _mainVM.KeyPressed(e);
            }

            e.Handled = true;
        }

        private void GoToState(UIState newState)
        {
            _state = newState;
            VisualStateManager.GoToState(this, _state.ToString(), true);
            Sidebar.GoToState(_state);
        }

        private void SettingsCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            _mainVM.IsFilePaneOpen = false;

            args.Request.ApplicationCommands.Add((_mainVM).SettingsPane.AboutCommand);
            args.Request.ApplicationCommands.Add((_mainVM).SettingsPane.ServerSettingsCommand);
            args.Request.ApplicationCommands.Add((_mainVM).SettingsPane.SettingsCommand);
        }

        private void OpenFiles(object sender, RoutedEventArgs e)
        {
            _mainVM.IsFilePaneOpen = true;
        }
    }
}