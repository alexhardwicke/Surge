// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Surge.Windows8.Converters
{
    public class URLToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var text = (string)value;
            if (text == "127.0.0.1" || text == "localhost")
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
