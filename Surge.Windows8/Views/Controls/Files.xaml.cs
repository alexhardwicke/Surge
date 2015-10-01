// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using Windows.UI.Xaml;

namespace Surge.Windows8.Views.Controls
{
    public sealed partial class Files
    {
        public Files()
        {
            InitializeComponent();
            Width = 500;
            SetHeight();
            Window.Current.SizeChanged += (s, e) => SetHeight();
        }

        private void SetHeight()
        {
            Flyout.Height = Window.Current.Bounds.Height;
            ListView.Height = Flyout.Height - 80;
        }

        // TODO: Set up folder collapsing for files
    }
}