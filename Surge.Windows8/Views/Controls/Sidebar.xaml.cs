// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENCE file in the project root for full license information.

using System;

using Surge.Shared.Common;
using Surge.Windows8.ViewModels.MainPage;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Surge.Windows8.Views.Controls
{
    public sealed partial class Sidebar : UserControl
    {
        private bool _searchFocused;

        public Sidebar()
        {
            InitializeComponent();

            Loaded += (s, e) => Filter.SelectedIndex = 0;
            Search.SuggestionsRequested += SuggestionsRequested;
            Search.QuerySubmitted += SearchSubmitted;
            AppIcon.Tapped += (_, e) =>
            {
                IconTapped?.Invoke(this, EventArgs.Empty);
            };

            // Search.FocusState is ALWAYS Unfocused, so we have to track it ourselves
            Search.GotFocus += (_, e) =>
                {
                    SearchFocused?.Invoke(this, EventArgs.Empty);
                    _searchFocused = true;
                };
            Search.LostFocus += (s, e) => _searchFocused = false;
        }

        public bool SearchHasFocus
        {
            get
            {
                return _searchFocused;
            }
        }

        public event EventHandler SearchFocused;
        public event EventHandler IconTapped;

        internal void FocusSearch()
        {
            Search.Focus(FocusState.Programmatic);
        }

        internal void GoToState(UIState state)
        {
            VisualStateManager.GoToState(this, state.ToString(), true);
        }

        internal void ClearSearch()
        {
            Search.QueryText = string.Empty;
        }

        private void SuggestionsRequested(SearchBox sender, SearchBoxSuggestionsRequestedEventArgs args)
        {
            if (args.QueryText == string.Empty)
            {
                return;
            }

            var suggestionCollection = args.Request.SearchSuggestionCollection;
            var queryText = args.QueryText.ToLower();

            foreach (TorrentViewModel torrent in (DataContext as MainPageViewModel).Torrents)
            {
                if (torrent.Name.ToLower().Contains(queryText))
                {
                    suggestionCollection.AppendQuerySuggestion(torrent.Name);
                }
            }
        }

        private void SearchSubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            (DataContext as MainPageViewModel).ListFilter.SearchText = args.QueryText;
        }
    }
}
