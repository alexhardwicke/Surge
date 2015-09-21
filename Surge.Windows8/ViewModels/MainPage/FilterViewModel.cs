// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using Microsoft.Practices.Prism.Mvvm;

using Surge.Shared.Common;

namespace Surge.Windows8.ViewModels.MainPage
{
    public class FilterViewModel : ViewModel
    {
        private int _count;
        private Func<IEnumerable<TorrentViewModel>, int> _check;

        public FilterViewModel(Filter filter, Func<IEnumerable<TorrentViewModel>, int> check)
        {
            Filter = filter;
            _check = check;
        }

        public Filter Filter { get; private set; }

        public int Count
        {
            get
            {
                return _count;
            }
            private set
            {
                SetProperty(ref _count, value);
            }
        }

        public void Update(IEnumerable<TorrentViewModel> torrents)
        {
            Count = _check(torrents);
        }
    }
}
