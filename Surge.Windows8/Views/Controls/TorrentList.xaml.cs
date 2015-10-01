// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using System;

using Surge.Shared.Common;

using Windows.UI.Xaml.Controls;

namespace Surge.Windows8.Views.Controls
{
    public sealed partial class TorrentList : UserControl
    {
        public TorrentList()
        {
            InitializeComponent();
            SizeChanged += (s, e) => List.MaxHeight = ActualHeight - 140;
            List.SelectionChanged += (s, e) =>
            {
                TorrentSelectionChanged?.Invoke(this, List.SelectedItems.Count);
            };
        }

        public event EventHandler<int> TorrentSelectionChanged;
    }
}
