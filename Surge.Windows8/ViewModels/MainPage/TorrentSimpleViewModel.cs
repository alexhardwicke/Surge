// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using System;
using System.Windows.Input;

using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Surge.Windows8.ViewModels.MainPage
{
    public class TorrentSimpleViewModel : ViewModel
    {
        private bool _isFile;
        private string _displayName;
        private string _displayPath;

        public event EventHandler OnRemoveItem;

        private void InitialiseCommands()
        {
            RemoveCommand = new DelegateCommand(() => OnRemoveItem?.Invoke(this, EventArgs.Empty));
        }

        public TorrentSimpleViewModel(string name, string location, int id) : base()
        {
            IsAdd = false;
            _displayName = name;
            _displayPath = location;
            Id = id;
            InitialiseCommands();
        }

        public TorrentSimpleViewModel(string uri) : base()
        {
            IsAdd = true;
            _isFile = false;
            int lastSlashPos = uri.LastIndexOf('/') + 1;
            _displayName = uri.Substring(lastSlashPos);
            _displayPath = uri.Substring(0, lastSlashPos);
            InitialiseCommands();
            TorrentData = uri;
        }

        public TorrentSimpleViewModel(string identifier, string name, string path) : base()
        {
            IsAdd = true;
            _isFile = true;
            _displayName = name;
            _displayPath = path;
            TorrentData = identifier;
            InitialiseCommands();
        }

        public bool IsAdd { get; private set; }
        public int Id { get; private set; }

        public ICommand RemoveCommand { get; private set; }

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                SetProperty(ref _displayName, value);
            }
        }

        public string DisplayPath
        {
            get
            {
                return _displayPath;
            }
            set
            {
                SetProperty(ref _displayPath, value);
            }
        }

        public string TorrentData { get; }

        public bool IsFile
        {
            get
            {
                return _isFile;
            }
            set
            {
                SetProperty(ref _isFile, value);
            }
        }
    }
}
