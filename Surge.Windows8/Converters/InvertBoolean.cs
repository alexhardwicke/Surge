// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using Windows.UI.Xaml.Data;

namespace Surge.Windows8.Converters
{
    public class InvertBoolean : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, string language)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        {
            throw new System.NotImplementedException();
        }
    }
}
