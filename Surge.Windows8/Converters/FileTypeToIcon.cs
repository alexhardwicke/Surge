// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using System;

using Surge.Shared.Common;

using Windows.UI.Xaml.Data;

namespace Surge.Windows8.Converters
{
    public class FileTypeToIcon : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, string language)
        {
            if (!(value is FileType))
            {
                throw new ArgumentException("Invalid file type: " + value);
            }

            switch ((FileType)value)
            {
                case FileType.Folder:
                    return "/Assets/folderopenicon.png";
                case FileType.Image:
                    return "/Assets/imageicon.png";
                case FileType.Video:
                    return "/Assets/videoicon.png";
                case FileType.File:
                    return "/Assets/fileicon.png";
                default:
                    throw new ArgumentException("Invalid file type: " + value);
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, string language)
        {
            throw new System.NotImplementedException();
        }
    }
}
