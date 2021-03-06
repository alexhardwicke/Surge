﻿// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using Surge.Windows8.ViewModels.MainPage;

using Windows.UI.Xaml.Controls;

namespace Surge.Windows8.Views.Controls
{
    public sealed partial class Settings : SettingsFlyout
    {
        public Settings(SettingsViewModel settingsVM)
        {
            InitializeComponent();
            DataContext = settingsVM;
        }
    }
}
