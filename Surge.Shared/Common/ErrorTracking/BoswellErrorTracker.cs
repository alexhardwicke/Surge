// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Boswell.Client;

namespace Surge.Shared.Common.ErrorTracking
{
    public class BoswellErrorTracker : ErrorTracker
    {
        private BoswellClient _client;

        public BoswellErrorTracker(string url, string apiKey)
        {
            if (!debug)
            {
                _client = new BoswellClient(url, apiKey);
            }
        }

        public override void Send(Exception caughtException, params string[] tags)
        {
            if (!debug)
                _client.SendError(caughtException, tags);
        }

        public override async Task SendAsync(Exception caughtException, params string[] tags)
        {
            if (!debug)
                await _client.SendErrorAsync(caughtException, tags);
        }
    }
}
