// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using Surge.Shared.Common;

using Windows.UI.Xaml.Data;

namespace Surge.Windows8.Converters
{
    public class FilterToIcon : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, string language)
        {
            if (!(value is Filter))
            {
                return "57627";
            }

            switch ((Filter)value)
            {
                case Filter.All:
                    return "57656";
                case Filter.Active:
                    return "57716";
                case Filter.Downloading:
                    return "57616";
                case Filter.Seeding:
                    return "57616";
                case Filter.Paused:
                    return "57603";
                case Filter.Error:
                    return "57713";
                default:
                    return "57627";
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        {
            throw new System.NotImplementedException();
        }
    }
}
