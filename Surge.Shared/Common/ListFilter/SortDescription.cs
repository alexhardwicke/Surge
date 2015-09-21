// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Surge.Shared.Common.ListFilter
{
    public class SortDescription
    {
        public SortDescription(string propertyName, ListSortDirection direction)
        {
            PropertyName = propertyName;
            Direction = direction;
        }

        public string PropertyName { get; set; }
        public ListSortDirection Direction { get; set; }
    }
}
