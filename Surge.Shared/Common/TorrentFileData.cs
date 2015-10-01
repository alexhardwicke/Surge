// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Surge.Shared.Common.ErrorTracking;

using Windows.Storage;

namespace Surge.Shared.Common
{
    public class TorrentFileData
    {
        private TorrentFileData(string base64, string name, string path)
        {
            Base64 = base64;
            Name = name;
            Path = path;
        }

        public string Base64 { get; }
        public string Name { get; }
        public string Path { get; }

        public async static Task<TorrentFileData> GenerateTorrentFileDataAsync(StorageFile file)
        {
            var base64 = await Helpers.GenerateString(file);
            var torrentFileData = new TorrentFileData(base64, file.Name, file.Path);
            return torrentFileData;
        }
    }
}
