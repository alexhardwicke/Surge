// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Surge.Windows8.Converters
{
    public class InvertBooleanToVisibility : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, string language)
        {
            if (!(value is bool))
            {
                throw new ArgumentException("Converting non-bool to bool");
            }

            if ((bool)value)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        {
            throw new System.NotImplementedException();
        }
    }
}
