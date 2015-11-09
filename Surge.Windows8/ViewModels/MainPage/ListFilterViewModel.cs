// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using System;
using System.Windows.Input;

using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;

using Surge.Shared.Common;

namespace Surge.Windows8.ViewModels.MainPage
{
    public class ListFilterViewModel : ViewModel
    {
        private Predicate<object> _filterByAll;
        private Predicate<object> _filterByActive;
        private Predicate<object> _filterByPaused;
        private Predicate<object> _filterByDownloading;
        private Predicate<object> _filterBySeeding;
        private Predicate<object> _filterByError;
        private Filter _currentFilter;
        private string _searchText;
        private IEventAggregator _eventAggregator;

        public ListFilterViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _searchText = string.Empty;

            _filterByAll = (a) => FilterBySearch(a as TorrentViewModel);
            _filterByActive = (a) =>
            {
                var torrent = a as TorrentViewModel;
                return torrent.IsActive && FilterBySearch(torrent);
            };
            _filterByPaused = (a) =>
            {
                var torrent = a as TorrentViewModel;
                return torrent.IsPaused && FilterBySearch(torrent);
            };
            _filterByDownloading = (a) =>
            {
                var torrent = a as TorrentViewModel;
                return torrent.IsDownloading && FilterBySearch(torrent);
            };
            _filterBySeeding = (a) =>
            {
                var torrent = a as TorrentViewModel;
                return torrent.IsSeeding && FilterBySearch(torrent);
            };
            _filterByError = (a) =>
            {
                var torrent = a as TorrentViewModel;
                return (torrent.HasError || torrent.IsUnavailable) && FilterBySearch(torrent);
            };

            ClearSearch = new DelegateCommand(() =>
            {
                SearchText = string.Empty;
                _eventAggregator.GetEvent<SearchCleared>().Publish(true);
            });
        }

        public ICommand ClearSearch { get; set; }

        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    OnPropertyChanged(nameof(HasSearch));
                    _eventAggregator.GetEvent<SearchChanged>().Publish(value);
                }
            }
        }

        public bool HasSearch
        {
            get
            {
                return !string.IsNullOrEmpty(SearchText);
            }
        }

        public Predicate<object> GetFilter(Filter currentFilter)
        {
            _currentFilter = currentFilter;

            switch (currentFilter)
            {
                case Filter.All:
                    return _filterByAll;
                case Filter.Downloading:
                    return _filterByDownloading;
                case Filter.Seeding:
                    return _filterBySeeding;
                case Filter.Active:
                    return _filterByActive;
                case Filter.Paused:
                    return _filterByPaused;
                case Filter.Error:
                    return _filterByError;
                default:
                    throw new InvalidOperationException("Invalid filter type");
            }
        }

        private bool FilterBySearch(TorrentViewModel torrent)
        {
            if (torrent.Name.ToLower().Contains(SearchText))
            {
                return true;
            }

            return false;
        }
    }
}
