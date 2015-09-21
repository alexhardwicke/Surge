// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using Surge.Core.Network;
using Surge.Shared.Common;

using Windows.UI.Xaml.Data;

namespace Surge.Windows8.Converters
{
    public class ServerTypeToBoolean : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, string language)
        {
            switch (parameter.ToString())
            {
                case "Deluge":
                    return value as ServerType == ServerType.Deluge;
                case "Transmission":
                    return value as ServerType == ServerType.Transmission;
                case "UTorrent":
                    return value as ServerType == ServerType.UTorrent;
                default:
                    throw new ArgumentException("Invalid server type");
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        {
            if ((bool)value)
            {
                return Helpers.GetServerTypeForString(parameter as string);
            }
            else
            {
                return null;
            }
        }
    }
}
