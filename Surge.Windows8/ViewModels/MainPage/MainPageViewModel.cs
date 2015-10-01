// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Mvvm.Interfaces;

using Surge.Core;
using Surge.Core.Common;
using Surge.Core.Models;
using Surge.Core.Network;
using Surge.Shared.Common;
using Surge.Shared.Common.ErrorTracking;
using Surge.Shared.Common.ListFilter;

using Windows.ApplicationModel.Activation;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Surge.Windows8.ViewModels.MainPage
{
    public class MainPageViewModel : ViewModel
    {
        private bool _isActive;
        private bool _showPause;
        private bool _showStart;
        private bool _showForceStart;
        private bool _showMore;
        private bool _isFilePaneOpen;
        private List<string> _uriQueue;
        private List<TorrentFileData> _torrentsQueue;
        private CancellationTokenSource _cancelToken;
        private TorrentViewModel _selectedItem;
        private PublicTorrentClient _torrentClient;
        private SettingsHelper _settingsHelper;
        private IEventAggregator _eventAggregator;
        private INavigationService _navigationService;
        private ErrorTracker _errorTracker;
        protected ObservableCollection<TorrentViewModel> backingTorrents;

        public MainPageViewModel(INavigationService navigationService, IEventAggregator eventAggregator,
                                 SettingsHelper settingsHelper, SettingsViewModel settingsViewModel,
                                 SettingsPaneViewModel settingsPaneViewModel, FilterListViewModel filterListViewModel,
                                 ListFilterViewModel listFilterViewModel, TorrentWindowViewModel torrentWindowViewModel,
                                 ErrorTracker errorTracker)
        {
            _errorTracker = errorTracker;
            _uriQueue = new List<string>();
            _torrentsQueue = new List<TorrentFileData>();

            _settingsHelper = settingsHelper;
            _eventAggregator = eventAggregator;
            _navigationService = navigationService;

            FilterList = filterListViewModel;
            ListFilter = listFilterViewModel;
            SettingsPane = settingsPaneViewModel;
            Settings = settingsViewModel;
            TorrentWindow = torrentWindowViewModel;

            backingTorrents = new ObservableCollection<TorrentViewModel>();
            Torrents = new ListCollectionView(backingTorrents);
            SelectedItems = new ObservableCollection<TorrentViewModel>();

            ChangeOrder(settingsHelper.GetSetting<bool>(SettingType.OrderByQueue));
            Torrents.Filter = ListFilter.GetFilter(Filter.All);

            ClearCommand = new DelegateCommand(() => ClearSelection());
            SelectAllCommand = new DelegateCommand(() => SelectAll());
            ForceStartCommand = new DelegateCommand(() => ChangeTorrentState(StateType.ForceStart));
            PauseCommand = new DelegateCommand(() => ChangeTorrentState(StateType.Stop));
            StartCommand = new DelegateCommand(() => ChangeTorrentState(StateType.Start));
            VerifyCommand = new DelegateCommand(() => ChangeTorrentState(StateType.Verify));
            ShowAddTorrentCommand = new DelegateCommand(() => TorrentWindow.Open(Purpose.Add, Server.DefaultDownloadLocation));
            ShowMoveTorrentsCommand = new DelegateCommand(() => TorrentWindow.Open(Purpose.Move, Server.DefaultDownloadLocation, SelectedItems));
            ShowConfirmDeleteCommand = new DelegateCommand(() => TorrentWindow.Open(Purpose.Delete, Server.DefaultDownloadLocation, SelectedItems));

            _eventAggregator.GetEvent<AddTorrent>().Subscribe(AddTorrents);
            _eventAggregator.GetEvent<DeleteTorrents>().Subscribe(Delete);
            _eventAggregator.GetEvent<MoveTorrents>().Subscribe(MoveTorrents);
            _eventAggregator.GetEvent<SearchChanged>().Subscribe(query => Torrents.Refresh());
            _eventAggregator.GetEvent<SearchCleared>().Subscribe(_ => Torrents.Refresh());
            _eventAggregator.GetEvent<PriorityChanged>().Subscribe(PriorityChanged);
            _eventAggregator.GetEvent<WantedChanged>().Subscribe(WantedChanged);
            _eventAggregator.GetEvent<FilterChanged>().Subscribe(FilterChanged);
            _eventAggregator.GetEvent<OrderByQueueSettingChanged>().Subscribe(orderByQueue => ChangeOrder(orderByQueue));
            _eventAggregator.GetEvent<FileActivated>().Subscribe(FileActivated);
            _eventAggregator.GetEvent<URIActivated>().Subscribe(URIActivated);
            _eventAggregator.GetEvent<ServerLoaded>().Subscribe(ProcessQueues);

            SelectedItems.CollectionChanged += (s, e) =>
            {
                SelectionChange();
            };
        }

        internal void KeyPressed(KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Delete && SelectedItems.Count > 0)
                TorrentWindow.Open(Purpose.Delete, Server.DefaultDownloadLocation);
            else if (e.Key == VirtualKey.Enter && SelectedItems.Count == 1)
                IsFilePaneOpen = !IsFilePaneOpen;
            else if (e.Key == VirtualKey.F)
                ChangeTorrentState(StateType.ForceStart);
            else if (e.Key == VirtualKey.P)
                ChangeTorrentState(StateType.Stop);
            else if (e.Key == VirtualKey.S)
                ChangeTorrentState(StateType.Start);
            else if (e.Key == VirtualKey.V)
                ChangeTorrentState(StateType.Verify);
        }

        public ICommand ClearCommand { get; set; }
        public ICommand ForceStartCommand { get; set; }
        public ICommand PauseCommand { get; set; }
        public ICommand SelectAllCommand { get; set; }
        public ICommand StartCommand { get; set; }
        public ICommand VerifyCommand { get; set; }
        public ICommand ShowAddTorrentCommand { get; set; }
        public ICommand ShowMoveTorrentsCommand { get; set; }
        public ICommand ShowConfirmDeleteCommand { get; set; }

        public ListCollectionView Torrents { get; private set; }
        public ObservableCollection<TorrentViewModel> SelectedItems { get; set; }
        public IEnumerable<int> SelectedItemsIDs { get { return SelectedItems.Select(x => x.Id); } }
        public string SelectedItemsNames { get { return string.Join("\n", SelectedItems.Select(x => x.Name)); } }
        public Action<IActivatedEventArgs> URIAction { get; private set; }
        public Action<IActivatedEventArgs> FileAction { get; private set; }

        public ListFilterViewModel ListFilter { get; private set; }
        public SettingsViewModel Settings { get; private set; }
        public FilterListViewModel FilterList { get; private set; }
        public ServerViewModel Server { get; protected set; }
        public SettingsPaneViewModel SettingsPane { get; set; }
        public TorrentWindowViewModel TorrentWindow { get; set; }

        public bool IsFilePaneOpen
        {
            get
            {
                return _isFilePaneOpen;
            }
            set
            {
                // Always force an update for the file pane, as we don't
                // care about previous state
                _isFilePaneOpen = value;
                OnPropertyChanged(nameof(IsFilePaneOpen));
            }
        }

        public bool ShowPause
        {
            get
            {
                return _showPause;
            }
            set
            {
                SetProperty(ref _showPause, value);
            }
        }

        public bool ShowStart
        {
            get
            {
                return _showStart;
            }
            set
            {
                SetProperty(ref _showStart, value);
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
            }
        }

        public bool ShowMore
        {
            get
            {
                return _showMore;
            }
            set
            {
                SetProperty(ref _showMore, value);
            }
        }

        public TorrentViewModel SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                SetProperty(ref _selectedItem, value);
            }
        }

        public override async void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);

            _isActive = true;

            if (Server == null || navigationMode == NavigationMode.New)
            {
                ClearState();
                Initialise();
                StartUpdate();
            }
        }

        public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatedFrom(viewModelState, suspending);

            _isActive = false;
        }

        public void Update(UpdateData updateData)
        {
            for (int i = 0; i < backingTorrents.Count; ++i)
            {
                var item = backingTorrents[i];
                if (updateData.DeletedTorrents.Contains(item.Id))
                {
                    var torrent = backingTorrents[i--];
                    backingTorrents.Remove(torrent);
                    continue;
                }
                else if (updateData.UpdatedTorrents.ContainsKey(item.Id))
                {
                    item.Update(updateData.UpdatedTorrents[item.Id], updateData.ServerStats.SpeedUnits, updateData.ServerStats.SizeUnits);
                    continue;
                }
            }

            foreach (var torrent in updateData.NewTorrents)
            {
                var torrentVM = new TorrentViewModel(torrent, _eventAggregator, _errorTracker, updateData.ServerStats.SpeedUnits, updateData.ServerStats.SizeUnits);
                backingTorrents.Add(torrentVM);
            }

            UpdateMenu();
            FilterList.Update(backingTorrents);
            Server.Update(updateData.ServerStats);
        }

        internal void ClearSelection()
        {
            SelectedItem = null;
            SelectedItems.Clear();
        }

        internal void SelectAll()
        {
            foreach (var item in backingTorrents)
            {
                if (!SelectedItems.Contains(item))
                {
                    SelectedItems.Add(item);
                }
            }
        }

        internal void Delete(TorrentDeletedData data)
        {
            _torrentClient.Delete(data.TorrentIDs, data.KeepFiles);

            foreach (var id in data.TorrentIDs)
            {
                var torrent = SelectedItems.Where(x => x.Id == id).FirstOrDefault();
                backingTorrents.Remove(torrent);
            }

            SelectionChange();
        }

        internal void ChangeTorrentState(StateType type)
        {
            _torrentClient.ChangeState(type, SelectedItemsIDs);

            Action<TorrentViewModel> action;

            if (type == StateType.Verify)
            {
                action = (t => t.IsVerifying = true);
            }
            else
            {
                var pause = type == StateType.Stop;
                action = (t => t.IsPaused = pause);
            }

            SelectedItems.ForEach(x =>
            {
                action(x);
                x.IsUserModified = true;
            });

            SelectionChange();
        }

        internal void ChangeOrder(bool orderByQueue)
        {
            Torrents.SortDescriptions.Clear();
            if (orderByQueue)
            {
                Torrents.SortDescriptions.Add(new SortDescription("Queue", ListSortDirection.Descending));
            }
            else
            {
                Torrents.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }
        }

        private void MoveTorrents(TorrentMovedData data)
        {
            _torrentClient.ChangeLocation(data.TorrentIDs, data.Location, data.MoveFiles);
            foreach (var item in backingTorrents)
            {
                if (data.TorrentIDs.Where(x => x == item.Id).Count() > 0)
                {
                    item.Location = data.Location;
                }
            }
        }

        private void UpdateMenu()
        {
            ShowPause = SelectedItems.Where(x => !x.IsPaused).Count() > 0;
            ShowStart = SelectedItems.Where(x => x.IsPaused).Count() > 0;
            ShowForceStart = _settingsHelper.GetSetting<bool>(SettingType.ShowForceStart) && SelectedItems.Where(x => x.IsPaused).Count() > 0;
            ShowMore = SelectedItems.Count() > 0;
        }

        private void HandleError(InternalError error)
        {
            _navigationService.Navigate("ServerConfiguration", error.ToString());
        }

        private void HandleException(Exception exn)
        {
            _errorTracker.Send(exn, "FSharp Exception");
            _cancelToken.Cancel();
            StartUpdate();
        }

        public void AddTorrents(TorrentAddedData data)
        {
            var location = data.Location.EscapeSlashes();
            foreach (var torrent in data.Files)
            {
                _torrentClient.AddFile(torrent, location);
            }

            foreach (var uri in data.URIs)
            {
                _torrentClient.AddURL(uri, location);
            }
        }

        private void ClearState()
        {
            backingTorrents.Clear();
            TorrentWindow.Clear();
            SelectedItem = null;
            SelectedItems.Clear();
            Torrents.Filter = ListFilter.GetFilter(Filter.All);

            if (_cancelToken != null)
            {
                if (!_cancelToken.IsCancellationRequested)
                {
                    _cancelToken.Cancel();
                }

                _cancelToken.Dispose();
            }

            if (_torrentClient != null)
            {
                // TODO: What? Why do I have to cast to IDisposable?
                // The object clearly implements IDisposable...
                (_torrentClient as IDisposable).Dispose();
            }
        }

        private void Initialise()
        {
            var server = _settingsHelper.GetServer();
            if (Server == null)
            {
                Server = new ServerViewModel(_eventAggregator, server);
            }
            else
            {
                Server.SetServer(server);
            }

            _torrentClient = Initialiser.GetTorrentClient(server, Update, HandleError, HandleException);
        }

        private void SelectionChange()
        {
            OnPropertyChanged(nameof(SelectedItemsIDs));
            OnPropertyChanged(nameof(SelectedItemsNames));
            UpdateMenu();
            FilterList.Update(backingTorrents);
        }

        private void StartUpdate()
        {
            if (_cancelToken != null)
            {
                if (!_cancelToken.IsCancellationRequested)
                {
                    _cancelToken.Cancel();
                }

                _cancelToken.Dispose();
            }

            _cancelToken = _torrentClient.StartUpdate();
        }

        private void WantedChanged(WantedChangedData data)
        {
            _torrentClient.ChangeFileWanted(data.TorrentID, data.ItemIDs, data.IsWanted);
        }

        private void PriorityChanged(PriorityChangedData data)
        {
            _torrentClient.ChangeFilePriority(data.TorrentID, data.ItemIDs, data.Priority);
        }

        private void FilterChanged(Filter filter)
        {
            Torrents.Filter = ListFilter.GetFilter(filter);
            Torrents.Refresh();
        }

        private async Task ShowCannotAddAsync()
        {
            var currentState = "";
            switch (TorrentWindow.Purpose)
            {
                case Purpose.Delete:
                    currentState = "deleting";
                    break;
                case Purpose.Move:
                    currentState = "moving";
                    break;
                default:
                    throw new ArgumentException("Invalid purpose for " + nameof(ShowCannotAddAsync) + ":" + TorrentWindow.Purpose.ToString());
            }
            if (TorrentWindow.Purpose == Purpose.Delete)
            {
                currentState = "deleting";
            }

            await new MessageDialog(string.Format("Finish {0} torrents first", currentState), "Couldn't add torrent").ShowAsync();
        }

        private async void URIActivated(string uri)
        {
            if (!_isActive)
            {
                return;
            }

            if (Server.IsLoaded)
            {
                if (TorrentWindow.IsOpen)
                {
                    if (TorrentWindow.IsAdd)
                    {
                        ProcessURI(uri);
                    }
                    else
                    {
                        await ShowCannotAddAsync();
                    }
                }
                else
                {
                    ProcessURI(uri);
                }
            }
            else
            {
                _uriQueue.Add(uri);
            }
        }

        private async void FileActivated(IEnumerable<TorrentFileData> torrents)
        {
            if (!_isActive)
            {
                return;
            }

            if (Server.IsLoaded)
            {
                if (TorrentWindow.IsOpen)
                {
                    if (TorrentWindow.IsAdd)
                    {
                        ProcessTorrents(torrents);
                    }
                    else
                    {
                        await ShowCannotAddAsync();
                    }
                }
                else
                {
                    ProcessTorrents(torrents);
                }
            }
            else
            {
                _torrentsQueue.AddRange(torrents);
            }
        }

        private void ProcessURI(string uri)
        {
            if ((TorrentWindow.IsOpen && TorrentWindow.Purpose == Purpose.Add) || _settingsHelper.GetSetting<bool>(SettingType.AlwaysAskDownloadLocation))
            {
                TorrentWindow.AddTorrentURI(uri);
                if (!TorrentWindow.IsOpen)
                {
                    TorrentWindow.Open(Purpose.Add, Server.DefaultDownloadLocation);
                }
            }
            else
            {
                _torrentClient.AddURL(uri, Server.DefaultDownloadLocation.EscapeSlashes());
            }
        }

        private void ProcessTorrents(IEnumerable<TorrentFileData> torrents)
        {
            if (_settingsHelper.GetSetting<bool>(SettingType.AlwaysAskDownloadLocation))
            {
                TorrentWindow.AddTorrentFiles(torrents);
                TorrentWindow.Open(Purpose.Add, Server.DefaultDownloadLocation);
            }
            else
            {
                torrents.ForEach(torrent => _torrentClient.AddFile(torrent.Base64, Server.DefaultDownloadLocation.EscapeSlashes()));
            }
        }

        private void ProcessQueues(bool _)
        {
            if (_torrentsQueue.Count == 0 && _uriQueue.Count == 0)
            {
                return;
            }

            if (_settingsHelper.GetSetting<bool>(SettingType.AlwaysAskDownloadLocation))
            {
                TorrentWindow.AddTorrentFiles(_torrentsQueue);
                TorrentWindow.AddTorrentURIs(_uriQueue);
                TorrentWindow.Open(Purpose.Add, Server.DefaultDownloadLocation);
            }
            else
            {
                _torrentsQueue.ForEach(torrent => _torrentClient.AddFile(torrent.Base64, Server.DefaultDownloadLocation.EscapeSlashes()));
                _uriQueue.ForEach(uri => _torrentClient.AddURL(uri, Server.DefaultDownloadLocation.EscapeSlashes()));
            }

            _torrentsQueue.Clear();
            _uriQueue.Clear();
        }
    }
}
