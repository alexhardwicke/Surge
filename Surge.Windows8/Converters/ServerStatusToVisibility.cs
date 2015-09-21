// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using Surge.Core.Network;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Surge.Windows8.Converters
{
    public class ServerStatusToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var status = value as ServerState;
            return status == ServerState.Working || status == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
