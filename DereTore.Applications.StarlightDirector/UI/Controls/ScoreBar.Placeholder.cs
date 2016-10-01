using System.Windows;
using System.Windows.Media;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    partial class ScoreBar {

        // http://stackoverflow.com/questions/24825132/how-to-blur-drawing-using-the-drawingcontext-wpf
        private sealed class Placeholder : FrameworkElement {

            public FormattedText FormattedText { get; set; }

            protected override void OnRender(DrawingContext drawingContext) {
                base.OnRender(drawingContext);
                drawingContext.DrawText(FormattedText, new Point());
            }

        }

    }
}
