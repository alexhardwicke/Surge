// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surge.Shared.Common.ErrorTracking
{
    public class StubErrorTracker : ErrorTracker
    {
        public override void Send(Exception caughtException, params string[] tags)
        {
            return;
        }

        public override async Task SendAsync(Exception caughtException, params string[] tags)
        {
            await Task.Delay(0);
        }
    }
}
