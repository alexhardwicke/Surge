// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Surge.Windows8.Converters
{
    public class StateToColour : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = value as string;
            if (state == "Verifying" || state == "Resolving Magnet")
            {
                return Application.Current.Resources["DarkHighlightBrush"] as Brush;
            }
            if (state == "Error")
            {
                return Application.Current.Resources["ErrorBrush"] as Brush;
            }
            else if (state == "Incomplete")
            {
                return Application.Current.Resources["TorrentIncompleteBrush"] as Brush;
            }
            else
            {
                return Application.Current.Resources["TorrentCompleteBrush"] as Brush;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
