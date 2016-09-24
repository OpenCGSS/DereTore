using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    /// <summary>
    /// ScoreBar.xaml 的交互逻辑
    /// </summary>
    partial class ScoreBar {

        public ScoreBar() {
            InitializeComponent();
            _horizontalLines = new List<Line>();
            _verticalLines = new List<Line>();
        }

        public event EventHandler<ScoreBarHitTestEventArgs> ScoreBarHitTest;

        public Brush Stroke {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        public double TextColumnWidth {
            get { return (double)GetValue(TextColumnWidthProperty); }
            set { SetValue(TextColumnWidthProperty, value); }
        }

        public double BarColumnWidth {
            get { return (double)GetValue(BarColumnWidthProperty); }
            set { SetValue(BarColumnWidthProperty, value); }
        }

        public double SpaceColumnWidth {
            get { return (double)GetValue(SpaceColumnWidthProperty); }
            set { SetValue(SpaceColumnWidthProperty, value); }
        }

        public Bar Bar {
            get { return (Bar)GetValue(BarProperty); }
            set { SetValue(BarProperty, value); }
        }

        public bool IsSelected {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public Brush SelectedInfoBrush {
            get { return (Brush)GetValue(SelectedInfoBrushProperty); }
            set { SetValue(SelectedInfoBrushProperty, value); }
        }

        public Brush NormalInfoBrush {
            get { return (Brush)GetValue(NormalInfoBrushProperty); }
            set { SetValue(NormalInfoBrushProperty, value); }
        }

        public Brush InfoBrush {
            get { return (Brush)GetValue(InfoBrushProperty); }
            set { SetValue(InfoBrushProperty, value); }
        }

        public double GridStrokeThickness {
            get { return (double)GetValue(GridStrokeThicknessProperty); }
            set { SetValue(GridStrokeThicknessProperty, value); }
        }

        public double InfoStrokeThickness {
            get { return (double)GetValue(InfoStrokeThicknessProperty); }
            set { SetValue(InfoStrokeThicknessProperty, value); }
        }

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(ScoreBar),
            new PropertyMetadata(Application.Current.FindResource(App.ResourceKeys.BarStrokeBrush), OnStrokeChanged));

        public static readonly DependencyProperty TextColumnWidthProperty = DependencyProperty.Register(nameof(TextColumnWidth), typeof(double), typeof(ScoreBar),
            new PropertyMetadata(75d, OnTextColumnWidthChanged));

        public static readonly DependencyProperty BarColumnWidthProperty = DependencyProperty.Register(nameof(BarColumnWidth), typeof(double), typeof(ScoreBar),
           new PropertyMetadata(375d, OnBarColumnWidthChanged));

        public static readonly DependencyProperty SpaceColumnWidthProperty = DependencyProperty.Register(nameof(SpaceColumnWidth), typeof(double), typeof(ScoreBar),
            new PropertyMetadata(75d, OnSpaceColumnWidthChanged));

        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(nameof(Bar), typeof(Bar), typeof(ScoreBar),
            new PropertyMetadata(null, OnBarChanged));

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(ScoreBar),
            new PropertyMetadata(false, OnIsSelectedChanged));

        public static readonly DependencyProperty SelectedInfoBrushProperty = DependencyProperty.Register(nameof(SelectedInfoBrush), typeof(Brush), typeof(ScoreBar),
            new PropertyMetadata(Brushes.LawnGreen));

        public static readonly DependencyProperty NormalInfoBrushProperty = DependencyProperty.Register(nameof(NormalInfoBrush), typeof(Brush), typeof(ScoreBar),
            new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty InfoBrushProperty = DependencyProperty.Register(nameof(InfoBrush), typeof(Brush), typeof(ScoreBar),
            new PropertyMetadata(Brushes.White, OnInfoBrushChanged));

        public static readonly DependencyProperty GridStrokeThicknessProperty = DependencyProperty.Register(nameof(GridStrokeThickness), typeof(double), typeof(ScoreBar),
            new PropertyMetadata(1d));

        public static readonly DependencyProperty InfoStrokeThicknessProperty = DependencyProperty.Register(nameof(InfoStrokeThickness), typeof(double), typeof(ScoreBar),
            new PropertyMetadata(3d));

        public ScoreBarHitTestInfo HitTest(Point pointRelativeToScoreBar) {
            if (Bar == null) {
                return new ScoreBarHitTestInfo(this, Bar, new Point(), -1, -1, false, false);
            }
            var destPoint = TranslatePoint(pointRelativeToScoreBar, Canvas);
            var width = Canvas.ActualWidth;
            var height = Canvas.ActualHeight;
            var columnCount = 5;
            var rowCount = TotalRowCount;
            double unitWidth = width / (columnCount - 1), unitHeight = height / rowCount;
            var column = (int)Math.Round(destPoint.X / unitWidth);
            var row = (int)Math.Round(destPoint.Y / unitHeight);
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
            BpmLabel.Text = newBpm.ToString("F2");
        }

        public int TotalRowCount { get; private set; }

        private static void OnStrokeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var bar = obj as ScoreBar;
            Debug.Assert(bar != null, "bar != null");
            bar.UpdateStroke();
        }

        private static void OnTextColumnWidthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var bar = obj as ScoreBar;
            Debug.Assert(bar != null, "bar != null");
            bar.TextColumnDef.Width = new GridLength((double)e.NewValue);
        }

        private static void OnBarColumnWidthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var bar = obj as ScoreBar;
            Debug.Assert(bar != null, "bar != null");
            bar.BarColumnDef.Width = new GridLength((double)e.NewValue);
        }

        private static void OnSpaceColumnWidthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var bar = obj as ScoreBar;
            Debug.Assert(bar != null, "bar != null");
            bar.SpaceColumnDef.Width = new GridLength((double)e.NewValue);
        }

        private static void OnBarChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var scoreBar = obj as ScoreBar;
            Debug.Assert(scoreBar != null, "scoreBar != null");
            var bar = e.NewValue as Bar;
            RedrawBar(scoreBar, bar, Size.Empty, new Size(scoreBar.ActualWidth, scoreBar.ActualHeight));
        }

        private static void OnIsSelectedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var scoreBar = obj as ScoreBar;
            Debug.Assert(scoreBar != null, "scoreBar != null");
            var b = (bool)e.NewValue;
            scoreBar.InfoBrush = b ? scoreBar.SelectedInfoBrush : scoreBar.NormalInfoBrush;
            scoreBar.InfoDropShadowEffect.Opacity = b ? 1 : 0;
        }

        private static void OnInfoBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var scoreBar = obj as ScoreBar;
            Debug.Assert(scoreBar != null, "scoreBar != null");
            scoreBar.InfoBorder.BorderBrush = (Brush)e.NewValue;
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
            sb.BarTimeLabel.Text = sb.BpmLabel.Text = sb.MeasureLabel.Text = string.Empty;
            if (bar == null) {
                sb.TotalRowCount = 0;
                return;
            }

            var timeSpan = TimeSpan.FromSeconds(bar.GetStartTime());
            sb.UpdateBarTimeText(timeSpan);
            sb.UpdateBpmText(bar.GetActualBpm());
            sb.UpdateBarIndexText(bar.Index);

            if (newValue != oldValue) {
                sb.UpdateAllLayouts();
            }
        }

        private void UpdateFrameLayout(double width, double height) {
            // Frame first.
            var rectShape = _frameRectangle;
            var canvas = Canvas;
            if (rectShape == null) {
                rectShape = new Rectangle();
                rectShape.Stroke = Stroke;
                rectShape.StrokeThickness = GridStrokeThickness;
                canvas.Children.Add(rectShape);
                _frameRectangle = rectShape;
            }
            rectShape.Width = width;
            rectShape.Height = height;
        }

        private void UpdateStroke() {
            var stressStroke = (Brush)Application.Current.FindResource(App.ResourceKeys.BarStrokeStressBrush);
            var stroke = Stroke;
            foreach (var lineShape in _verticalLines) {
                lineShape.Stroke = stroke;
            }
            foreach (var lineShape in _horizontalLines) {
                if (!lineShape.Stroke.Equals(stressStroke)) {
                    lineShape.Stroke = stroke;
                }
            }
            _frameRectangle.Stroke = stroke;
        }

        private void RenewMissingElements() {
            // HACK: The lines do disappear when the second resize happens (controlled by WPF), I don't know why.
            var canvas = Canvas;
            var l = _horizontalLines;
            if (l.Count > 0 && !canvas.Children.Contains(l[0])) {
                foreach (var lineShape in l) {
                    canvas.Children.Add(lineShape);
                }
            }
            l = _verticalLines;
            if (l.Count > 0 && !canvas.Children.Contains(l[0])) {
                foreach (var lineShape in l) {
                    canvas.Children.Add(lineShape);
                }
            }
        }

        private void UpdateAllLayouts() {
            var canvas = Canvas;
            var columnCount = 5;
            var width = canvas.ActualWidth;
            var height = canvas.ActualHeight;
            var stroke = Stroke;
            if (_verticalLines.Count == 0) {
                for (var i = 0; i < columnCount; ++i) {
                    var lineShape = new Line();
                    lineShape.Stroke = Brushes.White;
                    lineShape.StrokeThickness = GridStrokeThickness;
                    lineShape.Y1 = 0;
                    lineShape.Y2 = height;
                    lineShape.X1 = lineShape.X2 = height * i / (columnCount - 1);
                    canvas.Children.Add(lineShape);
                    _verticalLines.Add(lineShape);
                }
            } else {
                var i = 0;
                foreach (var lineShape in _verticalLines) {
                    if (!height.Equals(0)) {
                        lineShape.Y2 = height;
                    }
                    lineShape.X1 = lineShape.X2 = width * i / (columnCount - 1);
                    ++i;
                }
            }

            var bar = Bar;
            var stressStroke = (Brush)Application.Current.FindResource(App.ResourceKeys.BarStrokeStressBrush);
            var rowCount = TotalRowCount = bar.GetActualSignature() * bar.GetActualGridPerSignature();
            var signature = bar.GetActualSignature();
            if (rowCount + 1 != _horizontalLines.Count) {
                foreach (var horizontalLine in _horizontalLines) {
                    canvas.Children.Remove(horizontalLine);
                }
                _horizontalLines.Clear();
                for (var i = 0; i <= rowCount; ++i) {
                    var lineShape = new Line();
                    lineShape.Stroke = i % signature == 0 ? (i == 0 ? Brushes.Red : stressStroke) : stroke;
                    lineShape.StrokeThickness = GridStrokeThickness;
                    lineShape.X1 = 0;
                    lineShape.X2 = width;
                    lineShape.Y1 = lineShape.Y2 = height * i / rowCount;
                    _horizontalLines.Add(lineShape);
                    canvas.Children.Add(lineShape);
                }
            } else {
                var i = 0;
                foreach (var lineShape in _horizontalLines) {
                    if (!width.Equals(0)) {
                        lineShape.X2 = width;
                    }
                    lineShape.Y1 = lineShape.Y2 = height * i / rowCount;
                    ++i;
                }
            }
            RenewMissingElements();
            UpdateFrameLayout(width, height);
        }

        private readonly List<Line> _verticalLines;
        private readonly List<Line> _horizontalLines;
        private Rectangle _frameRectangle;

    }
}
