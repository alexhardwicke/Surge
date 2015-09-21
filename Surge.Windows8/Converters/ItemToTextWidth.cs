// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using Windows.UI.Xaml.Data;

namespace Surge.Windows8.Converters
{
    public class ItemToTextWidth : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is int))
            {
                return 0;
            }

            return 290 - (15 * (int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}