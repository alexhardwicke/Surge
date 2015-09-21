// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;

using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;

using Surge.Shared.Common;

using Windows.Storage.Pickers;

namespace Surge.Windows8.ViewModels.MainPage
{
    public class TorrentWindowViewModel : ViewModel
    {
        private IEventAggregator _eventAggregator;
        private SettingsHelper _settingsHelper;
        private Purpose _purpose;
        private string _defaultLocation;
        private string _location;
        private string _url;
        private string _addURL;
        private bool _useDefaultLocation;
        private bool _isOpen;
        private bool _moveFiles;
        private bool _keepFiles;
        private bool _showFavouriteLocations;
        private Dictionary<string, string> _torrentCache;

        public TorrentWindowViewModel(IEventAggregator eventAggregator, SettingsHelper settingsHelper)
        {
            _settingsHelper = settingsHelper;
            _eventAggregator = eventAggregator;

            SubmitCommand = new DelegateCommand(Submit);
            CancelCommand = new DelegateCommand(Clear);
            BrowseCommand = new DelegateCommand(BrowseForFiles);
            AddByURLCommand = new DelegateCommand(AddByURL);
            ToggleFavouriteLocationsCommand = new DelegateCommand(() => ShowFavouriteLocations = !ShowFavouriteLocations);

            Torrents = new ObservableCollection<TorrentSimpleViewModel>();
            _torrentCache = new Dictionary<string, string>();
            FavouriteLocations = new ObservableCollection<string>();

            Torrents.CollectionChanged += (s, e) => OnPropertyChanged(nameof(TorrentsQueued));

            eventAggregator.GetEvent<UseDefaultDownloadSettingChanged>().Subscribe(x => UseDefaultLocation = x);
        }

        public ObservableCollection<TorrentSimpleViewModel> Torrents { get; set; }
        public ObservableCollection<string> FavouriteLocations { get; set; }

        public bool TorrentsQueued
        {
            get
            {
                return Torrents.Count != 0;
            }
        }

        public string DefaultLocation
        {
            get
            {
                return _defaultLocation;
            }
        }

        public bool ShowFavouriteLocations
        {
            get
            {
                return _showFavouriteLocations;
            }
            set
            {
                SetProperty(ref _showFavouriteLocations, value);
            }
        }

        public bool UseDefaultLocation
        {
            get
            {
                if (Purpose == Purpose.Move)
                {
                    return false;
                }
                else
                {
                    return _useDefaultLocation;
                }
            }
            set
            {
                SetProperty(ref _useDefaultLocation, value);
                OnPropertyChanged(nameof(Location));
            }
        }

        public string Location
        {
            get
            {
                if (UseDefaultLocation)
                {
                    return _defaultLocation;
                }
                else
                {
                    return _location;
                }
            }
            set
            {
                SetProperty(ref _location, value);
            }
        }

        public string AddURL
        {
            get
            {
                return _addURL;
            }
            set
            {
                SetProperty(ref _addURL, value);
            }
        }

        public bool KeepFiles
        {
            get
            {
                return _keepFiles;
            }
            set
            {
                SetProperty(ref _keepFiles, value);
            }
        }

        public bool IsOpen
        {
            get
            {
                return _isOpen;
            }
            set
            {
                SetProperty(ref _isOpen, value);
            }
        }

        public Purpose Purpose
        {
            get
            {
                return _purpose;
            }
            set
            {
                SetProperty(ref _purpose, value);
                OnPropertyChanged(nameof(IsAdd));
                OnPropertyChanged(nameof(IsMove));
                OnPropertyChanged(nameof(IsDelete));
                OnPropertyChanged(nameof(UseDefaultLocation));
            }
        }

        public bool IsAdd
        {
            get
            {
                return _purpose == Purpose.Add;
            }
        }

        public bool IsMove
        {
            get
            {
                return _purpose == Purpose.Move;
            }
        }

        public bool IsDelete
        {
            get
            {
                return _purpose == Purpose.Delete;
            }
        }

        public bool MoveFiles
        {
            get
            {
                return _moveFiles;
            }
            set
            {
                SetProperty(ref _moveFiles, value);
            }
        }

        public ICommand AddByURLCommand { get; set; }
        public ICommand BrowseCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand SubmitCommand { get; set; }
        public ICommand ToggleFavouriteLocationsCommand { get; set; }

        public void Open(Purpose purpose, string defaultLocation, IEnumerable<TorrentViewModel> torrents = null)
        {
            if (purpose == Purpose.Add && torrents != null)
            {
                throw new ArgumentException("Passed Purpose.Add ad a non-empty enumerable of torrents");
            }

            if (!IsOpen)
            {
                _defaultLocation = defaultLocation;
                Location = defaultLocation;
                Purpose = purpose;
                IsOpen = true;
                FavouriteLocations.Add(_defaultLocation);
                var favouriteLocations = _settingsHelper.GetFavouriteLocations(5);
                favouriteLocations.ForEach(x =>
                {
                    if (_defaultLocation != x)
                    {
                        FavouriteLocations.Add(x);
                    }
                });
            }

            if (torrents != null)
            {
                AddTorrentObjects(torrents);
            }
        }

        internal void AddTorrentObjects(IEnumerable<TorrentViewModel> torrents)
        {
            torrents.ForEach(torrent => Torrents.Add(new TorrentSimpleViewModel(torrent.Name, torrent.Location, torrent.Id)));
        }

        internal void AddTorrentURI(string uri)
        {
            AddByURL(uri);
        }

        internal void AddTorrentURIs(List<string> uris)
        {
            foreach (var uriString in uris)
            {
                AddByURL(uriString);
            }
        }

        internal void AddTorrentFiles(IEnumerable<TorrentFileData> torrents)
        {
            foreach (var torrent in torrents)
            {
                if (!_torrentCache.ContainsKey(torrent.Base64))
                {
                    _torrentCache[torrent.Base64] = torrent.Base64;
                    AddTorrent(new TorrentSimpleViewModel(torrent.Base64, torrent.Name, torrent.Path));
                }
            }
        }

        private void RemoveTorrentHandler(object sender, EventArgs e)
        {
            var tvm = sender as TorrentSimpleViewModel;
            RemoveTorrent(tvm);
        }

        private void AddTorrent(TorrentSimpleViewModel tvm)
        {
            tvm.OnRemoveItem += RemoveTorrentHandler;
            Torrents.Add(tvm);
        }

        private void RemoveTorrent(TorrentSimpleViewModel tvm)
        {
            Torrents.Remove(tvm);
            tvm.OnRemoveItem -= RemoveTorrentHandler;
            if (_torrentCache.ContainsKey(tvm.TorrentData))
            {
                _torrentCache.Remove(tvm.TorrentData);
            }
        }

        private void Submit()
        {
            if (_purpose == Purpose.Add)
            {
                SendAddFiles();
            }
            else if (_purpose == Purpose.Move)
            {
                SendMoveFiles();
            }
            else
            {
                SendDeleteFiles();
            }

            _settingsHelper.AddFavouriteLocation(Location);

            Clear();
        }

        private void AddByURL()
        {
            if (AddByURL(AddURL))
            {
                AddURL = string.Empty;
            }
        }

        private bool AddByURL(string uriString)
        {
            if (string.IsNullOrEmpty(uriString))
            {
                return false;
            }

            if (!_torrentCache.ContainsKey(uriString))
            {
                try
                {
                    AddTorrent(new TorrentSimpleViewModel(uriString));
                    _torrentCache[uriString] = uriString;
                }
                catch (FormatException)
                {
                    return false;
                }
            }

            return true;
        }

        private async void BrowseForFiles()
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

            var torrentData = await Helpers.GenerateTorrentFileDataAsync(files);
            AddTorrentFiles(torrentData);
        }

        private void SendAddFiles()
        {
            var data = new TorrentAddedData(Location);
            data.Files.AddRange(Torrents.Where(torrent => torrent.IsAdd && torrent.IsFile).Select(x => x.TorrentData));
            data.URIs.AddRange(Torrents.Where(torrent => torrent.IsAdd && !torrent.IsFile).Select(x => x.TorrentData));

            _eventAggregator.GetEvent<AddTorrent>().Publish(data);
        }

        private void SendMoveFiles()
        {
            var data = new TorrentMovedData(Location, MoveFiles, Torrents.Where(x => !x.IsAdd).Select(x => x.Id));
            _eventAggregator.GetEvent<MoveTorrents>().Publish(data);
        }

        private void SendDeleteFiles()
        {
            var data = new TorrentDeletedData(KeepFiles, Torrents.Where(x => !x.IsAdd).Select(x => x.Id));
            _eventAggregator.GetEvent<DeleteTorrents>().Publish(data);
        }

        internal void Clear()
        {
            Torrents.Clear();
            _torrentCache.Clear();
            FavouriteLocations.Clear();
            Location = string.Empty;
            _defaultLocation = string.Empty;
            _url = string.Empty;
            IsOpen = false;
            MoveFiles = true;
            KeepFiles = true;
            UseDefaultLocation = !_settingsHelper.GetSetting<bool>(SettingType.AlwaysAskDownloadLocation);
        }
    }

    public enum Purpose
    {
        Add,
        Move,
        Delete
    }

    public struct SearchResult
    {
        public string SearchTerm { get; set; }
        public IEnumerable<string> Results { get; set; }
    }
}
