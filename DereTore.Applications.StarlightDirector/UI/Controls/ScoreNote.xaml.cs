using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    /// <summary>
    /// ScoreNote.xaml 的交互逻辑
    /// </summary>
    public partial class ScoreNote : UserControl {

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

        public bool Selected {
            get { return (bool)GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
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

        public static readonly DependencyProperty SelectedProperty = DependencyProperty.Register(nameof(Selected), typeof(bool), typeof(ScoreNote),
            new PropertyMetadata(false, OnSelectedChanged));

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

        private static void OnSelectedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var note = obj as ScoreNote;
            Debug.Assert(note != null, "note != null");
            note.DropShadow.Opacity = note.Selected ? 1 : 0;
            note.Stroke = note.GetBorderBrush();
        }

        private Brush GetBorderBrush() {
            return GetBorderBrush(Party);
        }

        private Brush GetFillBrush() {
            return GetFillBrush(Party);
        }

        private Brush GetBorderBrush(Party party) {
            var app = Application.Current;
            if (Selected) {
                return Brushes.LawnGreen;
            }
            switch (party) {
                case Party.Neutral:
                    return app.FindResource(App.ResourceKeys.NeutralStrokeBrush) as Brush;
                case Party.Cute:
                    return app.FindResource(App.ResourceKeys.CuteStrokeBrush) as Brush;
                case Party.Cool:
                    return app.FindResource(App.ResourceKeys.CoolStrokeBrush) as Brush;
                case Party.Passion:
                    return app.FindResource(App.ResourceKeys.PassionStrokeBrush) as Brush;
                default:
                    throw new ArgumentOutOfRangeException(nameof(party), party, null);
            }
        }

        private Brush GetFillBrush(Party party) {
            var app = Application.Current;
            switch (party) {
                case Party.Neutral:
                    return app.FindResource(App.ResourceKeys.NeutralFillBrush) as Brush;
                case Party.Cute:
                    return app.FindResource(App.ResourceKeys.CuteFillBrush) as Brush;
                case Party.Cool:
                    return app.FindResource(App.ResourceKeys.CoolFillBrush) as Brush;
                case Party.Passion:
                    return app.FindResource(App.ResourceKeys.PassionFillBrush) as Brush;
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
