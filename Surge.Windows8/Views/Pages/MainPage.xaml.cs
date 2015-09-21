namespace Surge.Views.Pages
{
    using System;
    using Common;
    using Controls;
    using Windows.Storage.Pickers;
    using Windows.System;
    using Windows.UI.ApplicationSettings;
    using Windows.UI.Popups;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Navigation;
    using ViewModels.Main;
    using Core.Network;
    using System.Collections.Generic;

    public sealed partial class MainPage : BasePage
    {
        private UIState state;
        private MainPageVM mainVM;
        public MainPageVM MainPageVM { get { return mainVM; } }

        public MainPage()
        {
            this.InitializeComponent();

            this.Loaded += async (s, e) =>
            {
                var upgradeDetailGenerator = new UpgradeDetailGenerator(new SettingsHelper());
                var upg = upgradeDetailGenerator.GetUpdateData();
                if (upg.IsUpgrade)
                {
                    var dialog = new MessageDialog(string.Empty, "Upgraded to Surge " + upg.NewVersionString);
                    foreach (var item in upg.UpgradeNotes)
                    {
                        dialog.Content += "- " + item + Environment.NewLine;
                    }

                    if (upg.NewFeaturesSettings)
                    {
                        dialog.Content += Environment.NewLine;
                        dialog.Content += "Some of these features may need to be enabled via app settings.";
                    }

                    await dialog.ShowAsync();
                }
            };

            this.Tapped += (s, e) => this.Files.IsOpen = false;
            this.Sidebar.AddToSearchGotFocus((s, e) => this.Files.IsOpen = false);
            this.Sidebar.SearchFocused += (_, e) => this.GoToState(UIState.Expanded);
            this.Sidebar.OnSearchSubmitted += (s, e) => this.mainVM.Filter.SearchText = (e as SearchEventArgs).Query;
            this.Torrents.TorrentSelectionChanged += (s, e) => { if (!e.OneSelected) this.Files.IsOpen = false; };
            this.AppBarHint.OpenAppBar += (se, ev) =>
                {
                    this.BottomAppBar.IsOpen = true;
                };
            this.BottomAppBar.Opened += (_, e) =>
                {
                    if (!this.mainVM.Server.IsInitialised)
                    {
                        this.BottomAppBar.IsOpen = false;
                        return;
                    }
                };
            this.BottomAppBar.Closed += (_, e) =>
                {
                    if (!this.mainVM.Server.IsInitialised)
                    {
                        return;
                    }

                    this.BottomAppBar.IsOpen = true;
                };
            this.SizeChanged += (_, e) =>
                {
                    var newState = this.ActualWidth >= 1366 ? UIState.Expanded : UIState.Narrow;
                    this.GoToState(newState);
                };
            this.Sidebar.IconTapped += (_, e) =>
                {
                    UIState newState = UIState.Expanded;
                    if (this.state == UIState.Expanded)
                    {
                        newState = UIState.Narrow;
                    }

                    this.GoToState(newState);
                };
        }

        public void HandleKey(KeyRoutedEventArgs e)
        {
            if (this.Sidebar.SearchHasFocus || this.AddTorrent.IsOpen || this.ChooseTorrentLocation.IsOpen || this.ChangeTorrentLocation.IsOpen || this.DeleteTorrent.IsOpen)
            {
                return;
            }

            switch (e.Key)
            {
                case VirtualKey.Delete:
                    if (this.mainVM.SelectedItems.Count > 0)
                    {
                        this.DeleteTorrent.IsOpen = true;
                    }

                    break;
                case VirtualKey.Enter:
                    if (this.mainVM.SelectedItems.Count == 1)
                    {
                        this.Files.IsOpen = !this.Files.IsOpen;
                    }

                    break;
                case VirtualKey.Q:
                    this.Sidebar.FocusSearch();
                    break;
                case VirtualKey.F:
                    this.mainVM.ForceStartCommand.Execute(null);
                    break;
                case VirtualKey.P:
                    this.mainVM.PauseCommand.Execute(null);
                    break;
                case global::Windows.System.VirtualKey.S:
                    this.mainVM.StartCommand.Execute(null);
                    break;
                case global::Windows.System.VirtualKey.V:
                    this.mainVM.VerifyCommand.Execute(null);
                    break;
            }

            e.Handled = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                base.OnNavigatedTo(e);
                SettingsPane.GetForCurrentView().CommandsRequested += this.SettingsCommandsRequested;

                if (this.mainVM != null)
                {
                    this.mainVM.OnErrorOccured -= (Application.Current as App).ErrorOccured;
                    this.mainVM.Server.OnServerLoaded -= this.ServerLoaded;
                    this.mainVM.Filter.OnSearchCleared -= this.SearchCleared;
                }

                this.mainVM = new MainPageVM();

                this.mainVM.OnErrorOccured += (Application.Current as App).ErrorOccured;
                this.mainVM.Server.OnServerLoaded += this.ServerLoaded;
                this.mainVM.Filter.OnSearchCleared += this.SearchCleared;
                this.DataContext = mainVM;
                mainVM.StartUpdate();

                this.BottomAppBar.IsOpen = false;
            }
            catch (Exception ex)
            {
                (Application.Current as App).ErrorTracker.Send(ex, new List<string>() { "MainPage.OnNavigatedTo" });
                throw;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            SettingsPane.GetForCurrentView().CommandsRequested -= this.SettingsCommandsRequested;
            this.mainVM.Server.OnServerLoaded -= this.ServerLoaded;
            this.mainVM.Filter.OnSearchCleared -= this.SearchCleared;
            this.DataContext = null;
            this.mainVM = null;
        }

        private void SearchCleared(object sender, EventArgs e)
        {
            this.Sidebar.ClearSearch();
        }

        private void GoToState(UIState newState)
        {
            this.state = newState;
            VisualStateManager.GoToState(this, this.state.ToString(), true);
            this.Sidebar.GoToState(this.state);
        }

        private void ServerLoaded(object sender, EventArgs e)
        {
            this.BottomAppBar.IsOpen = true;
        }

        private void SettingsCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            if (!this.mainVM.Server.IsInitialised)
            {
                return;
            }

            this.Files.IsOpen = false;
            var about = new SettingsCommand("E0C01107-73FA-4F14-A9B6-49E637E7A509", "About", handler =>
            {
                var aboutFlyout = new About();
                aboutFlyout.Show();
            });

            var serverSettings = new SettingsCommand("A1A10948-C3D4-4692-AAE2-E59F891F06B0", "Server", (handler) =>
            {
                (Application.Current as App).OpenServerConfigurationPage(ServerState.Working);
            });

            var settings = new SettingsCommand("4FDE3FF0-7A33-4B76-B8F6-A85221D99606", "Settings", handler =>
            {
                var settingsFlyout = new Settings(this.mainVM.Settings);
                settingsFlyout.Show();
            });

            args.Request.ApplicationCommands.Add(about);
            args.Request.ApplicationCommands.Add(serverSettings);
            args.Request.ApplicationCommands.Add(settings);
        }

        public void ShowChooseLocation()
        {
            this.ChooseTorrentLocation.IsOpen = true;
        }

        private void OpenFiles(object sender, RoutedEventArgs e)
        {
            this.Files.IsOpen = true;
        }

        private void ShowAddByURL(object sender, RoutedEventArgs e)
        {
            this.AddTorrent.IsOpen = true;
        }

        private void ShowConfirmDelete(object sender, RoutedEventArgs e)
        {
            this.DeleteTorrent.IsOpen = true;
        }

        private void ShowMoveTorrents(object sender, RoutedEventArgs e)
        {
            this.ChangeTorrentLocation.IsOpen = true;
        }

        private async void ShowAddByFile(object sender, RoutedEventArgs e)
        {
            var filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add(".torrent");
            filePicker.ViewMode = PickerViewMode.List;
            filePicker.SuggestedStartLocation = PickerLocationId.Downloads;
            filePicker.SettingsIdentifier = "torrentpicker";
            filePicker.CommitButtonText = "Add Torrent(s)";

            var files = await filePicker.PickMultipleFilesAsync();

            if (files.Count == 0)
            {
                return;
            }

            var strings = await Helpers.GenerateStrings(files);

            if (this.mainVM.Settings.AlwaysAskDownloadLocation)
            {
                this.mainVM.AddTorrent.QueuedFiles = strings;
                this.ShowChooseLocation();
            }
            else
            {
                this.mainVM.AddFiles(strings, this.mainVM.Server.DefaultDownloadLocation);
            }
        }
    }
}