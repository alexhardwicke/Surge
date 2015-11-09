// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;

using Surge.Core.Network;
using Surge.Shared.Common;
using Surge.Shared.Common.ErrorTracking;
using Surge.Windows8.Views;

using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Surge.Windows8
{
    public sealed partial class App : MvvmAppBase
    {
        private Container _container;
        private SettingsHelper _settingsHelper;
        private IEventAggregator _eventAggregator;

        public App()
        {
#if DEBUG
            ErrorTracker = new StubErrorTracker();
#else
            // Comment this line out if you want to test in release
            ErrorTracker = new BoswellErrorTracker(RELEASE_URL, RELEASE_API_KEY);
#endif

            InitializeComponent();

            _settingsHelper = new SettingsHelper();
            _eventAggregator = new EventAggregator();

            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
            {
                var viewName = viewType.Name;
                var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
                var viewModelName = string.Format(CultureInfo.InvariantCulture, "Surge.Windows8.ViewModels.{0}.{0}ViewModel, {1}", viewName, viewAssemblyName);
                return Type.GetType(viewModelName);
            });
        }

        public ErrorTracker ErrorTracker { get; }

        #region AppLaunch
        protected override async Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            await InitializeFrameAsync(args);
            (Window.Current.Content as Frame).NavigationFailed += (s, ex) => ErrorTracker.Send(ex.Exception, "Navigation Failed");
        }

        protected async override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);

            if (NavigationService == null)
            {
                await InitializeFrameAsync(args);
            }

            if (args is ProtocolActivatedEventArgs)
            {
                var uri = (args as ProtocolActivatedEventArgs).Uri.AbsoluteUri;

                _eventAggregator.GetEvent<URIActivated>().Publish(uri);
            }
        }

        protected async override void OnFileActivated(FileActivatedEventArgs args)
        {
            base.OnFileActivated(args);

            if (NavigationService == null)
            {
                await InitializeFrameAsync(args);
            }

            var torrents = await Helpers.GenerateTorrentFileDataAsync(args.Files);

            _eventAggregator.GetEvent<FileActivated>().Publish(torrents);
        }
        #endregion

        #region Container
        protected override object Resolve(Type type)
        {
            return _container.Resolve(type);
        }

#pragma warning disable CS1998
        protected async override Task OnInitializeAsync(IActivatedEventArgs args)
#pragma warning restore CS1998
        {
            _container = new Container(ErrorTracker, NavigationService, _eventAggregator);

            if (_settingsHelper.HaveServer)
            {
                NavigationService.Navigate("Main");
            }
            else
            {
                NavigationService.Navigate("ServerConfiguration", ServerState.Unconfigured.ToString());
            }

            Window.Current.Activate();
        }
        #endregion
    }
}