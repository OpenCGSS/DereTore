using System;
using System.Windows;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    partial class ScorePreviewer {

        public bool IsPreviewing {
            get {
                if (CheckAccess()) {
                    return (bool)GetValue(IsPreviewingProperty);
                } else {
                    if (_getIsPreviewing == null) {
                        _getIsPreviewing = () => (bool)GetValue(IsPreviewingProperty);
                    }
                    return (bool)Dispatcher.Invoke(_getIsPreviewing);
                }
            }
            internal set {
                if (CheckAccess()) {
                    SetValue(IsPreviewingProperty, value);
                } else {
                    if (_setIsPreviewing == null) {
                        _setIsPreviewing = v => SetValue(IsPreviewingProperty, v);
                    }
                    Dispatcher.Invoke(_setIsPreviewing, value);
                }
            }
        }

        public static readonly DependencyProperty IsPreviewingProperty = DependencyProperty.Register(nameof(IsPreviewing), typeof(bool), typeof(ScorePreviewer),
            new PropertyMetadata(false, OnIsPreviewingChanged));

        private static void OnIsPreviewingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var previewer = (ScorePreviewer)obj;
            previewer._isPreviewing = (bool)e.NewValue;
        }

        private Func<bool> _getIsPreviewing;
        private Action<bool> _setIsPreviewing;

    }
}
