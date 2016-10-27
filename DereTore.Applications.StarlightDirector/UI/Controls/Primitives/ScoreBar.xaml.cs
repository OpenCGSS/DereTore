using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Entities.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Controls.Primitives {
    public partial class ScoreBar {

        public ScoreBar() {
            InitializeComponent();
            _tickTypeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
            _bufferPlaceholder = new Placeholder {
                LayoutTransform = new ScaleTransform(1, -1),
                RenderTransformOrigin = new Point(0.5, 0.5)
            };
            _bufferPlaceholderBrush = new VisualBrush(_bufferPlaceholder);
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
            var totalGridCount = Bar.GetTotalGridCount();
            double unitWidth = width / (columnCount - 1), unitHeight = height / totalGridCount;
            var column = (int)Math.Round(destPoint.X / unitWidth);
            var row = (int)Math.Round(destPoint.Y / unitHeight);
            var zoomMod = GetBestFitZoomMod();
            row = (int)Math.Round((double)row / zoomMod) * zoomMod;
            var gridCrossingPosition = new Point(column * unitWidth, row * unitHeight);
            var distance = Point.Subtract(gridCrossingPosition, destPoint);
            if (distance.Length > 2 * NoteRadius) {
                return new ScoreBarHitTestInfo(this, Bar, new Point(), column, row, false, false);
            }
            if (column < 0 || column > columnCount - 1) {
                return new ScoreBarHitTestInfo(this, Bar, new Point(), column, row, false, false);
            }
            if (row < 0 || row >= Bar.GetTotalGridCount()) {
                return new ScoreBarHitTestInfo(this, Bar, pointRelativeToScoreBar, column, row, true, false);
            }
            return new ScoreBarHitTestInfo(this, Bar, pointRelativeToScoreBar, column, row, false, true);
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

        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);
            DrawBar(drawingContext);
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

        private static void RedrawBar(ScoreBar scoreBar, Bar bar, Size oldValue, Size newValue) {
            var canvas = scoreBar.Canvas;
            canvas.Children.Clear();
            if (bar == null) {
                return;
            }
            if (newValue != oldValue) {
                scoreBar.InvalidateMeasure();
            }
        }

        private int GetBestFitZoomLevel() {
            var noteDiameter = NoteRadius * 2;
            var zoomHeight = ActualHeight;
            var maxItems = zoomHeight / noteDiameter;
            var zoomLevels = ZoomLevels;
            foreach (var zoomLevel in zoomLevels) {
                if (zoomLevel < maxItems) {
                    return zoomLevel;
                }
            }
            return zoomLevels[zoomLevels.Length - 1];
        }

        private int GetBestFitZoomMod() {
            return SignatureBase / GetBestFitZoomLevel();
        }

        public static readonly double DefaultHeight = 550d;

        public static readonly int[] ZoomLevels = { 96, 48, 32, 24, 16, 12, 8, 6, 4, 3, 2, 1 };
        public static readonly int SignatureBase = ScoreSettings.DefaultGlobalGridPerSignature * ScoreSettings.DefaultGlobalSignature;
        public static readonly double ZoomFactor = 1.2;

    }
}
