// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using Surge.Core.Network;

using Windows.UI.Xaml.Data;

namespace Surge.Windows8.Converters
{
    public class ServerStatusToErrorString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var status = value as ServerState;
            if (status == ServerState.Working || status == null)
            {
                return string.Empty;
            }
            else if (status == ServerState.Unconfigured)
            {
                return "";
            }
            else if (status.IsServerError)
            {
                var error = status.GetError;

                if (error.IsBadRequest)
                {
                    return "The server did not accept Surge's connection as it detected the request as invalid, malformed or deceptive. Please try again, and if it continues not to work, make sure you don't have strange proxy settings.";
                }
                else if (error.IsConnection)
                {
                    return "Surge cannot connect to the server. Make sure the details are correct and try again.";
                }
                else if (error.IsCredential)
                {
                    return "The username and password are being declined. Please enter the correct details.";
                }
                else if (error.IsNonJSON)
                {
                    return @"This doesn't appear to be a BitTorrent server. Please provide the details for your server before trying to download torrents.\n
                           \n
                           You are not able to download torrents without a configured server. Surge is not a torrent client itself.";
                }
                else if (error.IsNotFound)
                {
                    return "The provided URL could not be found (Error 404).";
                }
                else if (error.IsOther)
                {
                    return "An unexpected error has occured. Make sure your computer has access to the correct network and appropriate permissions.";
                }
                else if (error.IsVersion)
                {
                    // TODO
                    // return "Surge has detected that your server is running an unsupported server version. Surge supports the following versions: Transmission 2.4+, µTorrent TODO+, Deluge TODO+"
                    return "Surge has detected that your server is running an unsupported server version. Surge supports Transmission 2.4 and higher.";
                }

                throw new ArgumentException("Invalid internal error");
            }

            throw new ArgumentException("Invalid server state");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
