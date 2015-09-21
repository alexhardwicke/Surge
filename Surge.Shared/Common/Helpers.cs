// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Surge.Core.Network;

using Windows.Storage;
using Windows.Storage.Streams;

namespace Surge.Shared.Common
{
    public static class Helpers
    {
        public static async Task<string> GenerateString(StorageFile file)
        {
            string base64 = "";
            using (var inputStream = await file.OpenAsync(FileAccessMode.Read))
            {
                var readStream = inputStream.GetInputStreamAt(0);
                var reader = new DataReader(readStream);
                uint fileLength = await reader.LoadAsync((uint)inputStream.Size);
                var data = new byte[fileLength];
                reader.ReadBytes(data);
                base64 = System.Convert.ToBase64String(data);
            }

            return base64;
        }

        internal static string GetStringForServerType(ServerType type)
        {
            if (type.IsDeluge)
            {
                return "Deluge";
            }
            else if (type.IsTransmission)
            {
                return "Transmission";
            }
            else if (type.IsUTorrent)
            {
                return "UTorrent";
            }

            throw new ArgumentException("Invalid server type");
        }

        internal static ServerType GetServerTypeForString(string value)
        {
            switch (value)
            {
                case "Deluge":
                    return ServerType.Deluge;
                case "Transmission":
                    return ServerType.Transmission;
                case "UTorrent":
                    return ServerType.UTorrent;
                default:
                    return null;
            }
        }

        internal async static Task<IEnumerable<TorrentFileData>> GenerateTorrentFileDataAsync(IReadOnlyList<IStorageItem> files)
        {
            var torrents = new List<TorrentFileData>();

            foreach (var file in files)
            {
                try
                {
                    if (file is StorageFile)
                    {
                        var torrent = await TorrentFileData.GenerateTorrentFileDataAsync(file as StorageFile);
                        torrents.Add(torrent);
                    }
                }
                catch (Exception) { }
            }

            return torrents;
        }
    }
}
