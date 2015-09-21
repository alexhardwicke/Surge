// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;

using Windows.UI.Xaml.Data;

namespace Surge.Shared.Common.ListFilter
{
    public interface ICollectionViewEx : ICollectionView
    {
        bool CanFilter { get; }
        Predicate<object> Filter { get; set; }

        bool CanSort { get; }
        IList<SortDescription> SortDescriptions { get; }

        IEnumerable SourceCollection { get; }

        IDisposable DeferRefresh();
        void Refresh();
    }
}
