// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Surge.Windows8.Views.Controls
{
    public sealed partial class About : SettingsFlyout
    {
        public About()
        {
            InitializeComponent();

            ScrollViewer.Height = Window.Current.Bounds.Height - 80;
        }
    }
}