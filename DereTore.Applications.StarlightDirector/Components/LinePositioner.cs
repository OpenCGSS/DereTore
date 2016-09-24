using System.Windows;
using DereTore.Applications.StarlightDirector.UI.Controls;

namespace DereTore.Applications.StarlightDirector.Components {
    public sealed class LinePositioner : DependencyObject {

        public static readonly DependencyProperty ScoreNote1Property = DependencyProperty.RegisterAttached("ScoreNote1", typeof(ScoreNote), typeof(LinePositioner),
            new PropertyMetadata(null));

        public static readonly DependencyProperty ScoreNote2Property = DependencyProperty.RegisterAttached("ScoreNote2", typeof(ScoreNote), typeof(LinePositioner),
           new PropertyMetadata(null));

        public static void SetScoreNote1(UIElement element, ScoreNote value) {
            element.SetValue(ScoreNote1Property, value);
        }

        public static ScoreNote GetScoreNote1(UIElement element) {
            return (ScoreNote)element.GetValue(ScoreNote1Property);
        }

        public static void SetScoreNote2(UIElement element, ScoreNote value) {
            element.SetValue(ScoreNote2Property, value);
        }

        public static ScoreNote GetScoreNote2(UIElement element) {
            return (ScoreNote)element.GetValue(ScoreNote2Property);
        }

    }
}
