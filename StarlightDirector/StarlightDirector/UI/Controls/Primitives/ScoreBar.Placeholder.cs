using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace StarlightDirector.UI.Controls.Primitives {
    partial class ScoreBar {

        // http://stackoverflow.com/questions/24825132/how-to-blur-drawing-using-the-drawingcontext-wpf
        private sealed class Placeholder : FrameworkElement {

            public Placeholder() {
                DrawingElements = new BindingList<TextAndLocation>();
            }

            public Collection<TextAndLocation> DrawingElements { get; }

            protected override void OnRender(DrawingContext drawingContext) {
                base.OnRender(drawingContext);
                foreach (var drawingElement in DrawingElements) {
                    drawingContext.DrawText(drawingElement.Text, drawingElement.Location);
                }
            }

        }

    }
}
