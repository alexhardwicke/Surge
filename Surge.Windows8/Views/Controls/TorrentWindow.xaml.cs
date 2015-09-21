// Copyright (c) Alex Hardwicke. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

using Callisto.Controls;

using Surge.Windows8.ViewModels.MainPage;

using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Surge.Windows8.Views.Controls
{
    public sealed partial class TorrentWindow : CustomDialog
    {
        private bool _isAnimating;
        private object _lockable = new object();
        private TorrentWindowViewModel _viewModel;

        public TorrentWindow()
        {
            InitializeComponent();
            DataContextChanged += (s, e) => _viewModel = DataContext as TorrentWindowViewModel;
            LayoutUpdated += (s, e) => AdjustFavouriteList();
        }

        private async void AddURLButtonClick(object sender, RoutedEventArgs e)
        {
            lock (_lockable)
            {
                if (_isAnimating)
                {
                    return;
                }

                _isAnimating = true;
            }

            if (AddByURLPanel.Visibility == Visibility.Visible)
            {
                (Resources["HideStoryboard"] as Storyboard).Begin();
            }
            else
            {
                (Resources["ShowStoryboard"] as Storyboard).Begin();
            }

            await Task.Delay(200);

            lock (_lockable)
            {
                _isAnimating = false;
            }
        }

        private void Window_Closed(object sender, RoutedEventArgs e)
        {
            (Resources["HideStoryboard"] as Storyboard).Begin();
        }

        private void FavouriteLocationList_Opened(object sender, object e)
        {
            AdjustFavouriteList();
        }

        private void AdjustFavouriteList()
        {
            var listHeight = 200;
            var currentPoint = FavouriteLocationPopup.TransformToVisual(Window.Current.Content).TransformPoint(new Point(0, 0));
            var anchorPoint = Location.TransformToVisual(Window.Current.Content).TransformPoint(new Windows.Foundation.Point(0, 0));
            var availableSpace = Window.Current.Bounds.Height - anchorPoint.Y - Location.ActualHeight;
            var newLocation = new Point(currentPoint.X, 0);
            if (availableSpace > listHeight)
            {
                newLocation.Y = anchorPoint.Y + Location.ActualHeight;
                // show below
            }
            else
            {
                newLocation.Y = anchorPoint.Y - listHeight;
                // show above
            }

            var transform = (FavouriteLocationPopup.RenderTransform as CompositeTransform) ?? (FavouriteLocationPopup.RenderTransform = new CompositeTransform()) as CompositeTransform;
            transform.TranslateY += newLocation.Y - currentPoint.Y;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            Location.Text = e.AddedItems[0].ToString();
            _viewModel.ShowFavouriteLocations = false;
            FavouriteLocationList.SelectedItem = null;
        }
    }
}
