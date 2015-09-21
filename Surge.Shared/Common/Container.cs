// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using Microsoft.Practices.Prism.Mvvm.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Unity;

using Surge.Shared.Common.ErrorTracking;

namespace Surge.Shared.Common
{
    internal class Container
    {
        private UnityContainer _container;

        public Container(ErrorTracker errorTracker, INavigationService navigationService, IEventAggregator eventAggregator)
        {
            _container = new UnityContainer();

            //container.RegisterInstance<ISessionStateService>(SessionStateService);
            _container.RegisterInstance<ErrorTracker>(errorTracker);
            _container.RegisterInstance<INavigationService>(navigationService);
            _container.RegisterInstance<IEventAggregator>(eventAggregator);
        }

        internal object Resolve(Type type)
        {
            return _container.Resolve(type);
        }

        internal T Resolve<T>()
        {
            return _container.Resolve<T>();
        }
    }
}
