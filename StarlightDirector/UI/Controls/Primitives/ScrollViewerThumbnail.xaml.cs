using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StarlightDirector.UI.Controls.Primitives {
    public partial class ScrollViewerThumbnail {

        public ScrollViewerThumbnail() {
            InitializeComponent();
        }

        private void ScrollView_OnScrollChanged(object sender, ScrollChangedEventArgs e) {
            RecalcLayout();
        }

        private void RecalcLayout() {
            if (_handlingSizeChanged) {
                return;
            }
            _handlingSizeChanged = true;
            var element = (FrameworkElement)ScrollViewer.Content;
            var contentWidth = element.ActualWidth;
            var contentHeight = element.ActualHeight;
            var scale = ActualWidth / contentWidth * 0.9;
            var totalHeightTransformed = contentHeight * scale;
            var scaleTransform = (ScaleTransform)ContentRect.RenderTransform;
            scaleTransform.ScaleX = scaleTransform.ScaleY = scale;

            var x = 0;
            double y;
            if (totalHeightTransformed <= ActualHeight) {
                y = (ActualHeight - totalHeightTransformed) / 2;
            } else {
                var scrollRatio = ScrollViewer.VerticalOffset / (contentHeight - ScrollViewer.ViewportHeight);
                var deltaHeight = totalHeightTransformed - ActualHeight;
                y = -deltaHeight * scrollRatio;
            }
            Canvas.SetLeft(ContentRect, x);
            Canvas.SetTop(ContentRect, y);
            _handlingSizeChanged = false;
        }

        private bool _handlingSizeChanged;

    }
}
