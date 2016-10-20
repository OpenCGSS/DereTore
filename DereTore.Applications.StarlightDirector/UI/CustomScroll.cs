using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DereTore.Applications.StarlightDirector.UI {
    // http://stackoverflow.com/questions/876994/adjust-flowdocumentreaders-scroll-increment-when-viewingmode-set-to-scroll
    public static class CustomScroll {

        public static double GetScrollSpeed(DependencyObject obj) {
            return (double)obj.GetValue(ScrollSpeedProperty);
        }

        public static void SetScrollSpeed(DependencyObject obj, double value) {
            obj.SetValue(ScrollSpeedProperty, value);
        }

        public static readonly DependencyProperty ScrollSpeedProperty = DependencyProperty.RegisterAttached("ScrollSpeed", typeof(double), typeof(CustomScroll),
            new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnScrollSpeedChanged));

        private static DependencyObject GetScrollViewer(DependencyObject obj) {
            // Return the DependencyObject if it is a ScrollViewer
            if (obj is ScrollViewer) {
                return obj;
            }
            var childrenCount = VisualTreeHelper.GetChildrenCount(obj);
            for (var i = 0; i < childrenCount; i++) {
                var child = VisualTreeHelper.GetChild(obj, i);
                var result = GetScrollViewer(child);
                if (result != null) {
                    return result;
                }
            }
            return null;
        }

        private static void OnScrollSpeedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var host = obj as UIElement;
            if (host != null) {
                host.PreviewMouseWheel += OnPreviewMouseWheelScrolled;
            }
        }

        private static void OnPreviewMouseWheelScrolled(object sender, MouseWheelEventArgs e) {
            var scrollHost = sender as DependencyObject;
            if (scrollHost == null) {
                return;
            }
            var scrollViewer = GetScrollViewer(scrollHost) as ScrollViewer;
            if (scrollViewer != null) {
                var scrollSpeed = (double)scrollViewer.GetValue(ScrollSpeedProperty);
                var offset = scrollViewer.VerticalOffset - (e.Delta * scrollSpeed / 6);
                if (offset < 0) {
                    scrollViewer.ScrollToVerticalOffset(0);
                } else if (offset > scrollViewer.ExtentHeight) {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
                } else {
                    scrollViewer.ScrollToVerticalOffset(offset);
                }
                e.Handled = true;
            } else {
                throw new NotSupportedException("ScrollSpeed Attached Property is not attached to an element containing a ScrollViewer.");
            }
        }

    }
}
