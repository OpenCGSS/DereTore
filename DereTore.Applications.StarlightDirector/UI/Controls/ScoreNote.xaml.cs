using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    /// <summary>
    /// ScoreNote.xaml 的交互逻辑
    /// </summary>
    public partial class ScoreNote {

        public ScoreNote() {
            InitializeComponent();
            Fill = Circle.Fill;
            Stroke = Circle.Stroke;
        }

        public Party Party {
            get { return (Party)GetValue(PartyProperty); }
            set { SetValue(PartyProperty, value); }
        }

        public Brush Fill {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public Brush Stroke {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        public ImageSource Image {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public double Radius {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public Note Note {
            get { return (Note)GetValue(NoteProperty); }
            internal set { SetValue(NoteProperty, value); }
        }

        public bool IsSelected {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public double X {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        public double Y {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(nameof(Fill), typeof(Brush), typeof(ScoreNote),
            new PropertyMetadata(Application.Current.FindResource(App.ResourceKeys.NeutralFillBrush), OnFillChanged));

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(ScoreNote),
            new PropertyMetadata(Application.Current.FindResource(App.ResourceKeys.NeutralStrokeBrush), OnBorderChanged));

        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(nameof(Image), typeof(ImageSource), typeof(ScoreNote),
            new PropertyMetadata(null, OnImageChanged));

        public static readonly DependencyProperty PartyProperty = DependencyProperty.Register(nameof(Party), typeof(Party), typeof(ScoreNote),
            new PropertyMetadata(Party.Neutral, OnPartyChanged));

        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(nameof(Radius), typeof(double), typeof(ScoreNote),
            new PropertyMetadata(50d, OnRadiusChanged));

        public static readonly DependencyProperty NoteProperty = DependencyProperty.Register(nameof(Note), typeof(Note), typeof(ScoreNote),
            new PropertyMetadata(null, OnNoteChanged));

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(ScoreNote),
            new PropertyMetadata(false, OnIsSelectedChanged));

        public static readonly DependencyProperty XProperty = DependencyProperty.Register(nameof(X), typeof(double), typeof(ScoreNote),
          new PropertyMetadata(0d, OnXChanged));

        public static readonly DependencyProperty YProperty = DependencyProperty.Register(nameof(Y), typeof(double), typeof(ScoreNote),
          new PropertyMetadata(0d, OnYChanged));

        private static void OnFillChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var note = obj as ScoreNote;
            Debug.Assert(note != null, "note != null");
            note.Circle.Fill = (Brush)e.NewValue;
        }

        private static void OnBorderChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var note = obj as ScoreNote;
            Debug.Assert(note != null, "note != null");
            note.Circle.Stroke = (Brush)e.NewValue;
        }

        private static void OnImageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var note = obj as ScoreNote;
            Debug.Assert(note != null, "note != null");
            note.ImageContent.Source = (ImageSource)e.NewValue;
        }

        private static void OnPartyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var note = obj as ScoreNote;
            Debug.Assert(note != null, "note != null");
            var value = (Party)e.NewValue;
            note.Fill = note.GetFillBrush(value);
            note.Stroke = note.GetBorderBrush(value);
        }

        private static void OnRadiusChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var note = obj as ScoreNote;
            Debug.Assert(note != null, "note != null");
            note.Width = note.Height = (double)e.NewValue * 2;
        }

        private static void OnNoteChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
        }

        private static void OnIsSelectedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var note = obj as ScoreNote;
            Debug.Assert(note != null, "note != null");
            note.DropShadow.Opacity = note.IsSelected ? 1 : 0;
            note.Stroke = note.GetBorderBrush();
        }

        private static void OnXChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var note = obj as ScoreNote;
            Debug.Assert(note != null, "note != null");
            if (note.VisualParent is Canvas) {
                var value = (double)e.NewValue;
                Canvas.SetLeft(note, value - note.Radius);
            } else {
                Debug.Print("The ScoreNote is expected to be put on a Canvas.");
            }
        }

        private static void OnYChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var note = obj as ScoreNote;
            Debug.Assert(note != null, "note != null");
            if (note.VisualParent is Canvas) {
                var value = (double)e.NewValue;
                Canvas.SetTop(note, value - note.Radius);
            } else {
                Debug.Print("The ScoreNote is expected to be put on a Canvas.");
            }
        }

        private Brush GetBorderBrush() {
            return GetBorderBrush(Party);
        }

        private Brush GetFillBrush() {
            return GetFillBrush(Party);
        }

        private Brush GetBorderBrush(Party party) {
            var app = Application.Current;
            if (IsSelected) {
                return Brushes.LawnGreen;
            }
            switch (party) {
                case Party.Neutral:
                    return app.FindResource<Brush>(App.ResourceKeys.NeutralStrokeBrush);
                case Party.Cute:
                    return app.FindResource<Brush>(App.ResourceKeys.CuteStrokeBrush);
                case Party.Cool:
                    return app.FindResource<Brush>(App.ResourceKeys.CoolStrokeBrush);
                case Party.Passion:
                    return app.FindResource<Brush>(App.ResourceKeys.PassionStrokeBrush);
                default:
                    throw new ArgumentOutOfRangeException(nameof(party), party, null);
            }
        }

        private Brush GetFillBrush(Party party) {
            var app = Application.Current;
            switch (party) {
                case Party.Neutral:
                    return app.FindResource<Brush>(App.ResourceKeys.NeutralFillBrush);
                case Party.Cute:
                    return app.FindResource<Brush>(App.ResourceKeys.CuteFillBrush);
                case Party.Cool:
                    return app.FindResource<Brush>(App.ResourceKeys.CoolFillBrush);
                case Party.Passion:
                    return app.FindResource<Brush>(App.ResourceKeys.PassionFillBrush);
                default:
                    throw new ArgumentOutOfRangeException(nameof(party), party, null);
            }
        }

        private void ScoreNote_OnSizeChanged(object sender, SizeChangedEventArgs e) {
            var clip = ImageClipper.Clip as EllipseGeometry;
            var width = ImageClipper.ActualWidth;
            var height = ImageClipper.ActualHeight;
            var margin = ImageClipper.Margin;
            Debug.Assert(clip != null, "clip != null");
            clip.Center = new Point((width - margin.Left - margin.Right) / 2 + margin.Left, (height - margin.Top - margin.Bottom) / 2 + margin.Top);
            clip.RadiusX = width / 2;
            clip.RadiusY = height / 2;

            clip = CircleInnerGlow.Clip as EllipseGeometry;
            width = CircleInnerGlow.ActualWidth;
            height = CircleInnerGlow.ActualHeight;
            margin = CircleInnerGlow.Margin;
            Debug.Assert(clip != null, "clip != null");
            clip.Center = new Point((width - margin.Left - margin.Right) / 2 + margin.Left, (height - margin.Top - margin.Bottom) / 2 + margin.Top);
            clip.RadiusX = width / 2;
            clip.RadiusY = height / 2;
        }

    }
}
