using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    partial class ScoreBar {

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

        public double MinimumZoomHeight {
            get { return (double)GetValue(MinimumZoomHeightProperty); }
            set { SetValue(MinimumZoomHeightProperty, value); }
        }

        public double NoteRadius {
            get { return (double)GetValue(NoteRadiusProperty); }
            set { SetValue(NoteRadiusProperty, value); }
        }

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(ScoreBar),
            new FrameworkPropertyMetadata(Application.Current.FindResource(App.ResourceKeys.BarStrokeBrush), FrameworkPropertyMetadataOptions.AffectsRender));

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

        public static readonly DependencyProperty MinimumZoomHeightProperty = DependencyProperty.Register(nameof(MinimumZoomHeight), typeof(double), typeof(ScoreBar),
            new FrameworkPropertyMetadata(30d, FrameworkPropertyMetadataOptions.AffectsMeasure, OnMinimumZoomHeightChanged));

        // See ScoreNote.
        public static readonly DependencyProperty NoteRadiusProperty = DependencyProperty.Register(nameof(NoteRadius), typeof(double), typeof(ScoreBar),
            new FrameworkPropertyMetadata(15d, FrameworkPropertyMetadataOptions.AffectsMeasure, OnNoteRadiusChanged), OnNoteRadiusValidating);

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

        private static void OnZoomHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var scoreBar = obj as ScoreBar;
            Debug.Assert(scoreBar != null, "scoreBar != null");
            var newValue = (double)e.NewValue;
            scoreBar.Height = newValue;
        }

        private static void OnMinimumZoomHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var scoreBar = obj as ScoreBar;
            Debug.Assert(scoreBar != null, "scoreBar != null");
            var newValue = (double)e.NewValue;
            if (scoreBar.Height < newValue) {
                scoreBar.Height = newValue;
            }
        }

        private static void OnNoteRadiusChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var scoreBar = obj as ScoreBar;
            Debug.Assert(scoreBar != null, "scoreBar != null");
            var newValue = (double)e.NewValue;
            if (scoreBar.MinimumZoomHeight < newValue * 2) {
                scoreBar.MinimumZoomHeight = newValue * 2;
            }
        }

        private static bool OnNoteRadiusValidating(object newValue) {
            return newValue is double && (double)newValue > 0;
        }

    }
}
