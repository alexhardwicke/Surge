// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surge.Shared.Common.ErrorTracking
{
    public abstract class ErrorTracker
    {
        protected bool debug;

        protected ErrorTracker()
        {
#if DEBUG
            debug = true;
#endif
        }

        public abstract void Send(Exception caughtException, params string[] tags);
        public abstract Task SendAsync(Exception caughtException, params string[] tags);
    }
}
