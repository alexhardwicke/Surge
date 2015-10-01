// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.StoreApps;

using Surge.Shared.Common.ErrorTracking;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Surge.Windows8.Views
{
    public abstract class BasePage : VisualStateAwarePage
    {
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }
    }
}
