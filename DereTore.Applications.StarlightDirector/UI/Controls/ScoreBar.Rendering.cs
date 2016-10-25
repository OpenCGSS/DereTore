using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Entities.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    partial class ScoreBar {

        public static readonly double GridStrokeThickness = 1d;

        private void DrawBar(DrawingContext context) {
            var canvas = Canvas;
            const int columnCount = 5;
            var width = canvas.ActualWidth;
            var height = canvas.ActualHeight;

            var canvasOrigin = canvas.TranslatePoint(new Point(), this);
            var xOffset = canvasOrigin.X;
            for (var i = 0; i < columnCount; ++i) {
                var x = width * i / (columnCount - 1);
                var startPoint = new Point(x + xOffset, 0);
                var endPoint = new Point(x + xOffset, height);
                context.DrawLine(VerticalLinesPen, startPoint, endPoint);
            }

            var bar = Bar;
            var totalGridCount = bar.GetTotalGridCount();
            var gridPerSignature = bar.GetActualGridPerSignature();
            var zoomMod = GetBestFitZoomMod();
            var typeFace = _tickTypeface;
            var culture = CultureInfo.CurrentUICulture;
            var flowDirection = FlowDirection;
            const double textEmSize = 16;
            const double textMargin = 15;
            var placeholder = _bufferPlaceholder;
            var visualBrush = _bufferPlaceholderBrush;
            placeholder.DrawingElements.Clear();
            var maxTextWidth = double.MinValue;
            var maxHeight = double.MinValue;
            var textLineHeight = double.NaN;
            var visibleGridCountPerSignature = gridPerSignature / zoomMod;
            var hasPartialGrids = visibleGridCountPerSignature * zoomMod != gridPerSignature;

            if (visibleGridCountPerSignature <= 0) {
                return;
            }

            for (var i = 0; i <= totalGridCount; ++i) {
                if (i % zoomMod != 0 && i != totalGridCount) {
                    // Only draws key beats.
                    continue;
                }
                var y = height * i / totalGridCount;
                Pen pen;
                if (i == 0) {
                    pen = FirstGridLinePen;
                } else if (i % gridPerSignature == 0) {
                    pen = Level1BeatPen;
                } else if ((i * 2) % gridPerSignature == 0) {
                    pen = Level2BeatPen;
                } else if ((i * 4) % gridPerSignature == 0) {
                    pen = Level3BeatPen;
                } else {
                    pen = Level4BeatPen;
                }
                var startPoint = new Point(0 + xOffset, y);
                var endPoint = new Point(width + xOffset, y);
                context.DrawLine(pen, startPoint, endPoint);
                if (i >= totalGridCount) {
                    // We are not drawing any text for the last grid line.
                    continue;
                }
                if (hasPartialGrids && (i % gridPerSignature) != 0) {
                    continue;
                }
                var gridIndexInVisibleGrids = (i / zoomMod) % visibleGridCountPerSignature;
                var formattedText = new FormattedText($"{gridIndexInVisibleGrids + 1}/{visibleGridCountPerSignature}", culture, flowDirection, typeFace, textEmSize, pen.Brush);
                maxTextWidth = Math.Max(maxTextWidth, formattedText.Width);
                var location = new Point(xOffset - textMargin - formattedText.Width, y - textEmSize / 2);
                if (double.IsNaN(textLineHeight)) {
                    textLineHeight = formattedText.Height;
                }
                // http://stackoverflow.com/questions/24825132/how-to-blur-drawing-using-the-drawingcontext-wpf
                // Later https://blogs.msdn.microsoft.com/jaimer/2009/07/03/rendertargetbitmap-tips/ (final version)
                placeholder.DrawingElements.Add(new TextAndLocation {
                    Text = formattedText,
                    Location = location
                });
                maxHeight = Math.Max(maxHeight, location.Y + formattedText.Height);
            }

            if (placeholder.DrawingElements.Count > 0) {
                foreach (var drawingElement in placeholder.DrawingElements) {
                    var location = drawingElement.Location;
                    location.Y = maxHeight - location.Y;
                    drawingElement.Location = location;
                }
                placeholder.InvalidateVisual();
                context.DrawRectangle(visualBrush, null, new Rect(xOffset - textMargin - maxTextWidth, -textLineHeight / 2, maxTextWidth, maxHeight));
            }
        }

        private static readonly Pen VerticalLinesPen = new Pen(Brushes.White, 1);
        private static readonly Pen FirstGridLinePen = new Pen(new SolidColorBrush(Color.FromArgb(0xff, 0xf6, 0xf6, 0x0a)), 2);
        private static readonly Pen Level1BeatPen = new Pen(new SolidColorBrush(Color.FromArgb(0xff, 0xfd, 0xaf, 0xc9)), 2); // E.g. 4th grid lne for a 4/4 song
        private static readonly Pen Level2BeatPen = new Pen(new SolidColorBrush(Color.FromArgb(0xff, 0xc6, 0xc0, 0xe2)), 2); // E.g. 8th grid line for a 4/4 song
        private static readonly Pen Level3BeatPen = new Pen(new SolidColorBrush(Color.FromArgb(0xff, 0xd5, 0xd5, 0xd5)), 1); // E.g. 16th grid line for a 4/4 song
        private static readonly Pen Level4BeatPen = new Pen(new SolidColorBrush(Color.FromArgb(0xff, 0x56, 0x56, 0x56)), 1); // Other grid lines

        private readonly Placeholder _bufferPlaceholder;
        private readonly VisualBrush _bufferPlaceholderBrush;

        private readonly Typeface _tickTypeface;

    }
}
