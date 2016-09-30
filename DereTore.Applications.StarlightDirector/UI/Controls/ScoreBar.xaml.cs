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
    public partial class ScoreBar {

        public ScoreBar() {
            InitializeComponent();
            //_horizontalLines = new List<Line>();
            //_verticalLines = new List<Line>();
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
            new FrameworkPropertyMetadata(Application.Current.FindResource(App.ResourceKeys.BarStrokeBrush), FrameworkPropertyMetadataOptions.AffectsRender, OnStrokeChanged));

        public static readonly DependencyProperty TextColumnWidthProperty = DependencyProperty.Register(nameof(TextColumnWidth), typeof(double), typeof(ScoreBar),
            new FrameworkPropertyMetadata(75d, FrameworkPropertyMetadataOptions.AffectsArrange, OnTextColumnWidthChanged));

        public static readonly DependencyProperty BarColumnWidthProperty = DependencyProperty.Register(nameof(BarColumnWidth), typeof(double), typeof(ScoreBar),
           new FrameworkPropertyMetadata(375d, FrameworkPropertyMetadataOptions.AffectsArrange, OnBarColumnWidthChanged));

        public static readonly DependencyProperty SpaceColumnWidthProperty = DependencyProperty.Register(nameof(SpaceColumnWidth), typeof(double), typeof(ScoreBar),
            new FrameworkPropertyMetadata(75d, FrameworkPropertyMetadataOptions.AffectsArrange, OnSpaceColumnWidthChanged));

        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(nameof(Bar), typeof(Bar), typeof(ScoreBar),
            new FrameworkPropertyMetadata(null, OnBarChanged));

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(ScoreBar),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnIsSelectedChanged));

        public static readonly DependencyProperty SelectedInfoBrushProperty = DependencyProperty.Register(nameof(SelectedInfoBrush), typeof(Brush), typeof(ScoreBar),
            new FrameworkPropertyMetadata(Brushes.LawnGreen, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty NormalInfoBrushProperty = DependencyProperty.Register(nameof(NormalInfoBrush), typeof(Brush), typeof(ScoreBar),
            new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty InfoBrushProperty = DependencyProperty.Register(nameof(InfoBrush), typeof(Brush), typeof(ScoreBar),
            new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender, OnInfoBrushChanged));

        public static readonly DependencyProperty GridStrokeThicknessProperty = DependencyProperty.Register(nameof(GridStrokeThickness), typeof(double), typeof(ScoreBar),
            new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty InfoStrokeThicknessProperty = DependencyProperty.Register(nameof(InfoStrokeThickness), typeof(double), typeof(ScoreBar),
            new FrameworkPropertyMetadata(3d, FrameworkPropertyMetadataOptions.AffectsMeasure));

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
            BpmLabel.Text = newBpm.ToString("F3");
        }

        public int TotalRowCount { get; private set; }

        private static void OnStrokeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var bar = obj as ScoreBar;
            Debug.Assert(bar != null, "bar != null");
            //bar.UpdateStroke();
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
            if (bar == null) {
                sb.TotalRowCount = 0;
                return;
            }

            if (newValue != oldValue) {
                sb.InvalidateMeasure();
            }
        }

        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);

            var canvas = Canvas;
            var columnCount = 5;
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
                drawingContext.DrawLine(verticalLinePen, startPoint, endPoint);
            }

            var bar = Bar;
            var stressStroke = (Brush)Application.Current.FindResource(App.ResourceKeys.BarStrokeStressBrush);
            var rowCount = TotalRowCount = bar.GetActualSignature() * bar.GetActualGridPerSignature();
            var signature = bar.GetActualSignature();
            var barStartPen = new Pen(Brushes.Red, GridStrokeThickness);
            var barSignaturePen = new Pen(stressStroke, GridStrokeThickness);
            var barNormalPen = new Pen(stroke, GridStrokeThickness);
            for (var i = 0; i <= rowCount; ++i) {
                var y = height * i / rowCount;
                var pen = i % signature == 0 ? (i == 0 ? barStartPen : barSignaturePen) : barNormalPen;
                var startPoint = new Point(0 + xOffset, y);
                var endPoint = new Point(width + xOffset, y);
                drawingContext.DrawLine(pen, startPoint, endPoint);
            }
        }

    }
}
