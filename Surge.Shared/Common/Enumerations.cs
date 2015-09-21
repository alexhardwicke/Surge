// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Surge.Shared.Common
{
    public enum Filter
    {
        All,
        Active,
        Downloading,
        Seeding,
        Paused,
        Error
    }

    public enum UIState
    {
        Expanded,
        Narrow
    }

    public enum FileType
    {
        File,
        Folder,
        Image,
        Video
    }
}
