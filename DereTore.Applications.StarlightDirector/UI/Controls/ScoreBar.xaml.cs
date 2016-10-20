using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Entities.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    public partial class ScoreBar {

        public ScoreBar() {
            InitializeComponent();
        }

        public event EventHandler<ScoreBarHitTestEventArgs> ScoreBarHitTest;

        public ScoreBarHitTestInfo HitTest(Point pointRelativeToScoreBar) {
            if (Bar == null) {
                return new ScoreBarHitTestInfo(this, Bar, new Point(), -1, -1, false, false);
            }
            var destPoint = TranslatePoint(pointRelativeToScoreBar, Canvas);
            var width = Canvas.ActualWidth;
            var height = Canvas.ActualHeight;
            const int columnCount = 5;
            var rowCount = TotalRowCount;
            double unitWidth = width / (columnCount - 1), unitHeight = height / rowCount;
            var column = (int)Math.Round(destPoint.X / unitWidth);
            var row = (int)Math.Round(destPoint.Y / unitHeight);
            var zoomMod = GetBestFitZoomMod();
            row = (int)Math.Round((double)row / zoomMod) * zoomMod;
            var gridCrossingPosition = new Point(column * unitWidth, row * unitHeight);
            var distance = Point.Subtract(gridCrossingPosition, destPoint);
            if (distance.Length > 2 * NoteRadius) {
                return new ScoreBarHitTestInfo(this, Bar, new Point(), -1, -1, false, false);
            }
            if (column < 0 || column > columnCount - 1) {
                return new ScoreBarHitTestInfo(this, Bar, new Point(), -1, -1, false, false);
            }
            if (row < 0 || row >= Bar.GetTotalGridCount()) {
                return new ScoreBarHitTestInfo(this, Bar, pointRelativeToScoreBar, -1, -1, true, false);
            }
            return new ScoreBarHitTestInfo(this, Bar, pointRelativeToScoreBar, column, row, false, true);
        }

        public void SetGlobalBpm(double bpm) {
            Bar.SquashParams();
            if (Bar.Params == null) {
                UpdateBpmText(bpm);
            }
        }

        public void SetPrivateBpm(double bpm) {
            var bar = Bar;
            if (bar.Params == null) {
                bar.Params = new BarParams();
            }
            bar.Params.UserDefinedBpm = bpm;
            UpdateBpmText(bpm);
        }

        public void UpdateBarTimeText() {
            // TODO: Bar.GetStartTime() is EXTREMELY time expensive (O(n), so it's easy to be O(n^2) when calling it in a loop). Avoid using it.
            UpdateBarTimeText(TimeSpan.FromSeconds(Bar.GetStartTime()));
        }

        public void UpdateBarTimeText(TimeSpan timeSpan) {
            // TODO: Use binding.
            BarTimeLabel.Text = $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}.{timeSpan.Milliseconds:000}";
        }

        public void UpdateBarIndexText() {
            UpdateBarIndexText(Bar.Index);
        }

        public void UpdateBarIndexText(int newIndex) {
            // TODO: Use binding.
            MeasureLabel.Text = (newIndex + 1).ToString();
        }

        public void UpdateBpmText() {
            UpdateBpmText(Bar.GetActualBpm());
        }

        public void UpdateBpmText(double newBpm) {
            // TODO: Use binding.
            BpmLabel.Text = newBpm.ToString("F3");
        }

        public void ZoomIn() {
            Height *= ZoomFactor;
        }

        public void ZoomOut() {
            if (Height > MinimumZoomHeight) {
                Height /= ZoomFactor;
            }
        }

        public void ZoomToHeight(double height) {
            if (height > MinimumZoomHeight) {
                Height = height;
            }
        }

        public int TotalRowCount { get; private set; }

        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);
            DrawBar(drawingContext);
        }

        private void DrawBar(DrawingContext context) {
            var canvas = Canvas;
            const int columnCount = 5;
            var width = canvas.ActualWidth;
            var height = canvas.ActualHeight;
            var stroke = Stroke;

            var canvasOrigin = canvas.TranslatePoint(new Point(), this);
            var xOffset = canvasOrigin.X;
            var verticalLinePen = new Pen(Brushes.White, GridStrokeThickness);
            for (var i = 0; i < columnCount; ++i) {
                var x = width * i / (columnCount - 1);
                var startPoint = new Point(x + xOffset, 0);
                var endPoint = new Point(x + xOffset, height);
                context.DrawLine(verticalLinePen, startPoint, endPoint);
            }

            var bar = Bar;
            var stressStroke = (Brush)Application.Current.FindResource(App.ResourceKeys.BarStrokeStressBrush);
            var rowCount = TotalRowCount = bar.GetActualSignature() * bar.GetActualGridPerSignature();
            var signature = bar.GetActualSignature();
            var signatureInterval = SignatureBase / signature;
            var barStartBrush = Brushes.Red;
            var barSignatureBrush = stressStroke;
            var barNormalBrush = stroke;
            var barStartPen = new Pen(barStartBrush, GridStrokeThickness);
            var barSignaturePen = new Pen(barSignatureBrush, GridStrokeThickness);
            var barNormalPen = new Pen(barNormalBrush, GridStrokeThickness);
            var zoomMod = GetBestFitZoomMod();
            var typeFace = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
            var culture = CultureInfo.CurrentUICulture;
            var flowDirection = FlowDirection;
            const double textEmSize = 16;
            const double textMargin = 15;
            var placeholder = new Placeholder {
                LayoutTransform = new ScaleTransform(1, -1),
                RenderTransformOrigin = new Point(0.5, 0.5)
            };
            var visualBrush = new VisualBrush(placeholder);
            var maxTextWidth = double.MinValue;
            var maxHeight = double.MinValue;
            var textLineHeight = double.NaN;
            for (var i = 0; i <= rowCount; ++i) {
                if (i % zoomMod != 0 && i != rowCount) {
                    // Only draws key beats.
                    continue;
                }
                var y = height * i / rowCount;
                var pen = i % signatureInterval == 0 ? (i == 0 ? barStartPen : barSignaturePen) : barNormalPen;
                var startPoint = new Point(0 + xOffset, y);
                var endPoint = new Point(width + xOffset, y);
                context.DrawLine(pen, startPoint, endPoint);
                if (i < rowCount) {
                    var formattedText = new FormattedText($"{i + 1}/{SignatureBase}", culture, flowDirection, typeFace, textEmSize, pen.Brush);
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

        private void BarGridContainer_OnSizeChanged(object sender, SizeChangedEventArgs e) {
            RedrawBar(this, Bar, e.PreviousSize, e.NewSize);
        }

        private void MouseArea_OnPreviewMouseDown(object sender, MouseButtonEventArgs e) {
            var info = HitTest(e.GetPosition(this));
            ScoreBarHitTest?.Invoke(this, new ScoreBarHitTestEventArgs(info, e));
        }

        private void MouseArea_OnPreviewMouseUp(object sender, MouseButtonEventArgs e) {
            var info = HitTest(e.GetPosition(this));
            ScoreBarHitTest?.Invoke(this, new ScoreBarHitTestEventArgs(info, e));
        }

        private static void RedrawBar(ScoreBar sb, Bar bar, Size oldValue, Size newValue) {
            var canvas = sb.Canvas;
            canvas.Children.Clear();
            if (bar == null) {
                sb.TotalRowCount = 0;
                return;
            }

            if (newValue != oldValue) {
                sb.InvalidateMeasure();
            }
        }

        private int GetBestFitZoomLevel() {
            var noteDiameter = NoteRadius * 2;
            var zoomHeight = ActualHeight;
            var maxItems = zoomHeight / noteDiameter;
            var zoomLevels = ZoomLevels;
            for (var i = 0; i < zoomLevels.Length; ++i) {
                if (zoomLevels[i] < maxItems) {
                    return zoomLevels[i];
                }
            }
            return zoomLevels[zoomLevels.Length - 1];
        }

        private int GetBestFitZoomMod() {
            return SignatureBase / GetBestFitZoomLevel();
        }

        public static readonly double DefaultHeight = 550;
        public static readonly int[] ZoomLevels = { 384, 128, 96, 48, 32, 24, 16, 12, 4, 1 };
        public static readonly int SignatureBase = ScoreSettings.DefaultGlobalGridPerSignature * ScoreSettings.DefaultGlobalSignature;
        public static readonly double ZoomFactor = 1.2;

    }
}
