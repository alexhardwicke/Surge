// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using System;

using Surge.Shared.Common;

using Windows.UI.Xaml.Data;

namespace Surge.Windows8.Converters
{
    public class FilterToRotation : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Filter)
            {
                if (((Filter)value) != Filter.Downloading)
                {
                    return 0;
                }
                else
                {
                    return 180;
                }
            }

            throw new ArgumentException("Passing non-filter to Filter converter");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
