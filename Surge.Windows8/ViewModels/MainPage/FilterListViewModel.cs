// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;

using Surge.Shared.Common;

namespace Surge.Windows8.ViewModels.MainPage
{
    public class FilterListViewModel : ViewModel
    {
        private FilterViewModel _selectedFilter;
        private IEventAggregator _eventAggregator;

        public FilterListViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            Filters = new ObservableCollection<FilterViewModel>();

            var startFilter = new FilterViewModel(Filter.All,
                                           new Func<IEnumerable<TorrentViewModel>, int>(x => x.Count()));
            SelectedFilter = startFilter;

            Filters.Add(startFilter);
            Filters.Add(new FilterViewModel(Filter.Active,
                                          new Func<IEnumerable<TorrentViewModel>, int>(x =>
                                              x.Where(y => y.IsActive).Count())));
            Filters.Add(new FilterViewModel(Filter.Downloading,
                                          new Func<IEnumerable<TorrentViewModel>, int>(x =>
                                              x.Where(y => y.IsDownloading).Count())));
            Filters.Add(new FilterViewModel(Filter.Seeding,
                                          new Func<IEnumerable<TorrentViewModel>, int>(x =>
                                              x.Where(y => y.IsSeeding).Count())));
            Filters.Add(new FilterViewModel(Filter.Paused,
                                          new Func<IEnumerable<TorrentViewModel>, int>(x =>
                                              x.Where(y => y.IsPaused).Count())));
            Filters.Add(new FilterViewModel(Filter.Error,
                                          new Func<IEnumerable<TorrentViewModel>, int>(x =>
                                              x.Where(y => y.HasError || (y.IsUnavailable && !y.IsPaused)).Count())));
        }

        public ObservableCollection<FilterViewModel> Filters { get; private set; }

        public FilterViewModel SelectedFilter
        {
            get
            {
                return _selectedFilter;
            }
            set
            {
                SetProperty(ref _selectedFilter, value);
                if (value != null)
                {
                    _eventAggregator.GetEvent<FilterChanged>().Publish(value.Filter);
                }
            }
        }

        public void Update(IEnumerable<TorrentViewModel> torrents)
        {
            foreach (var filter in Filters)
            {
                filter.Update(torrents);
            }
        }
    }
}
