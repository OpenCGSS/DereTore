using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StarlightDirector.UI.Controls.Primitives {
    // http://www.thejoyofcode.com/Controllerizing_the_ScrollViewer_Thumbnail.aspx
    partial class ScrollViewerThumbnail {

        public ScrollViewer ScrollViewer {
            get { return (ScrollViewer)GetValue(ScrollViewerProperty); }
            set { SetValue(ScrollViewerProperty, value); }
        }

        public Brush HighlightFill {
            get { return (Brush)GetValue(HighlightFillProperty); }
            set { SetValue(HighlightFillProperty, value); }
        }

        public static readonly DependencyProperty ScrollViewerProperty = DependencyProperty.Register(nameof(ScrollViewer), typeof(ScrollViewer), typeof(ScrollViewerThumbnail),
            new UIPropertyMetadata(null, OnScrollViewerChanged));

        public static readonly DependencyProperty HighlightFillProperty = DependencyProperty.Register(nameof(HighlightFill), typeof(Brush), typeof(ScrollViewerThumbnail),
            new UIPropertyMetadata(new SolidColorBrush(Color.FromArgb(0x20, 0xff, 0xff, 0xff))));

        private static void OnScrollViewerChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var thumbnail = (ScrollViewerThumbnail)obj;
            var newValue = (ScrollViewer)e.NewValue;
            var oldValue = (ScrollViewer)e.OldValue;
            if (oldValue != null) {
                oldValue.ScrollChanged -= thumbnail.ScrollView_OnScrollChanged;
            }
            if (newValue != null) {
                newValue.ScrollChanged += thumbnail.ScrollView_OnScrollChanged;
            }
        }

    }
}
