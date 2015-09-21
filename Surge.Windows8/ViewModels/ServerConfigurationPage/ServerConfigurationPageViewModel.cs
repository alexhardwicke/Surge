// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Windows.Input;

using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;

using Surge.Core.Common;
using Surge.Core.Network;
using Surge.Shared.Common;

namespace Surge.Windows8.ViewModels.ServerConfigurationPage
{
    public class ServerConfigurationPageViewModel : ViewModel
    {
        private bool _isConfigured;
        private bool _isTesting;
        private string _url;
        private string _port;
        private string _username;
        private string _password;
        private ServerState _serverState;
        private ServerType _serverType;
        private SettingsHelper _settingsHelper;
        private INavigationService _navigationService;

        public ServerConfigurationPageViewModel(SettingsHelper settingsHelper, INavigationService navigationService)
        {
            _settingsHelper = settingsHelper;
            _navigationService = navigationService;
            InitialiseCommands();
        }

        internal void Initialise(ServerState state)
        {
            CanCancel = state.IsWorking;
            IsNewServer = state.IsUnconfigured;
            ServerState = state;

            if (_settingsHelper.HaveServer)
            {
                var server = _settingsHelper.GetServer();
                Username = server.Username == null ? "" : server.Username;
                Password = server.Password == null ? "" : server.Password;
                URL = server.URL == null ? "" : server.URL;
                Port = string.IsNullOrEmpty(server.Port) ? Port : server.Port;
                ServerType = server.ServerType;
            }
            else
            {
                Username = string.Empty;
                Password = string.Empty;
                URL = string.Empty;
                Port = string.Empty;
                ServerType = ServerType.Transmission;
            }
        }

        public bool CanCancel { get; set; }
        public bool IsNewServer { get; set; }

        public ICommand TestConnectionCommand { get; set; }
        public ICommand SaveServerCommand { get; set; }
        public ICommand CancelEditCommand { get; set; }

        public string URL
        {
            get
            {
                return _url;
            }
            set
            {
                SetProperty(ref _url, value);
                OnPropertyChanged(nameof(URI));
                OnPropertyChanged(nameof(CanTest));
                IsConfigured = false;
            }
        }

        public string Port
        {
            get
            {
                return _port;
            }
            set
            {
                SetProperty(ref _port, value);
                OnPropertyChanged(nameof(URI));
                OnPropertyChanged(nameof(CanTest));
                IsConfigured = false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CC0013:Use ternary operator", Justification = "<Pending>")]
        public Uri URI
        {
            get
            {
                try
                {
                    // Add the protocol if it doesn't exist
                    if (URL.IndexOf("://", StringComparison.Ordinal) != -1)
                    {
                        return new UriBuilder(URL + ":" + Port).Uri;
                    }
                    else
                    {
                        return new UriBuilder("http://" + URL + ":" + Port).Uri;
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                SetProperty(ref _username, value);
                IsConfigured = false;
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                SetProperty(ref _password, value);
                IsConfigured = false;
            }
        }

        public ServerType ServerType
        {
            get
            {
                return _serverType;
            }
            set
            {
                if (value == null)
                {
                    return;
                }
                SetProperty(ref _serverType, value);

                if (ServerType == ServerType.Deluge)
                {
                    Port = "8112";
                }
                else if (ServerType == ServerType.Transmission)
                {
                    Port = "9091";
                }
                else if (ServerType == ServerType.UTorrent)
                {
                    Port = "";
                }
            }
        }

        public ServerState ServerState
        {
            get
            {
                return _serverState;
            }
            set
            {
                SetProperty(ref _serverState, value);
                OnPropertyChanged(nameof(CanTest));
            }
        }

        public bool IsTesting
        {
            get
            {
                return _isTesting;
            }
            set
            {
                SetProperty(ref _isTesting, value);
                OnPropertyChanged(nameof(CanTest));
            }
        }

        public bool CanTest
        {
            get
            {
                return URI != null && !IsTesting;
            }
        }

        public bool IsConfigured
        {
            get
            {
                return _isConfigured;
            }
            set
            {
                SetProperty(ref _isConfigured, value);
            }
        }

        private void InitialiseCommands()
        {
            TestConnectionCommand = new DelegateCommand(async () =>
            {
                IsTesting = true;
                var updater = Initialiser.GetTestTorrentClient(new Server(URL, Port, Username, Password, ServerType));
                ServerState = await updater.CheckConnectionAsync();
                IsConfigured = ServerState == ServerState.Working;
                IsTesting = false;
                (updater as IDisposable).Dispose();
            });
            SaveServerCommand = new DelegateCommand(() =>
            {
                var server = new Server(URL, Port, Username, Password, ServerType);
                _settingsHelper.SetServer(server);
                _navigationService.ClearHistory();
                _navigationService.Navigate("Main");
                _navigationService.ClearHistory();
            });
            CancelEditCommand = new DelegateCommand(() =>
            {
                if (_navigationService.CanGoBack())
                {
                    _navigationService.GoBack();
                }
                else
                {
                    _navigationService.Navigate("Main");
                }
            });
        }
    }
}
