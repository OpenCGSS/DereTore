using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    partial class ScoreNote {

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
            new PropertyMetadata(Application.Current.FindResource(App.ResourceKeys.NeutralFillBrush)));

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(ScoreNote),
            new PropertyMetadata(Application.Current.FindResource(App.ResourceKeys.NeutralStrokeBrush)));

        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(nameof(Image), typeof(ImageSource), typeof(ScoreNote),
            new PropertyMetadata(null, OnImageChanged));

        public static readonly DependencyProperty PartyProperty = DependencyProperty.Register(nameof(Party), typeof(Party), typeof(ScoreNote),
            new PropertyMetadata(Party.Neutral, OnPartyChanged));

        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(nameof(Radius), typeof(double), typeof(ScoreNote),
            new PropertyMetadata(0d, OnRadiusChanged));

        public static readonly DependencyProperty NoteProperty = DependencyProperty.Register(nameof(Note), typeof(Note), typeof(ScoreNote),
            new PropertyMetadata(null));

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(ScoreNote),
            new PropertyMetadata(false, OnIsSelectedChanged));

        public static readonly DependencyProperty XProperty = DependencyProperty.Register(nameof(X), typeof(double), typeof(ScoreNote),
          new PropertyMetadata(0d, OnXChanged));

        public static readonly DependencyProperty YProperty = DependencyProperty.Register(nameof(Y), typeof(double), typeof(ScoreNote),
          new PropertyMetadata(0d, OnYChanged));

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

    }
}
