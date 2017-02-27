using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StarlightDirector.Entities;

namespace StarlightDirector.UI.Controls.Primitives {
    partial class ScoreNote {

        // Compiler restrictions: static member initialization order
        private static readonly Brush DefaultStrokeBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x22, 0x22, 0x22));

        private static readonly Brush DefaultTextStrokeBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x7f, 0x7f, 0x7f));

        private static readonly Brush SelectedStrokeBrush = Brushes.Yellow;

        public Brush Stroke {
            get { return (Brush)GetValue(StrokeProperty); }
            private set { SetValue(StrokeProperty, value); }
        }

        public Brush TextStroke {
            get { return (Brush)GetValue(TextStrokeProperty); }
            private set { SetValue(TextStrokeProperty, value); }
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

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(ScoreNote),
            new PropertyMetadata(DefaultStrokeBrush));

        public static readonly DependencyProperty TextStrokeProperty = DependencyProperty.Register(nameof(TextStroke), typeof(Brush), typeof(ScoreNote),
            new PropertyMetadata(DefaultTextStrokeBrush));

        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(nameof(Radius), typeof(double), typeof(ScoreNote),
            new PropertyMetadata(double.NaN, OnRadiusChanged));

        public static readonly DependencyProperty NoteProperty = DependencyProperty.Register(nameof(Note), typeof(Note), typeof(ScoreNote),
            new PropertyMetadata(null));

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(ScoreNote),
            new PropertyMetadata(false, OnIsSelectedChanged));

        public static readonly DependencyProperty XProperty = DependencyProperty.Register(nameof(X), typeof(double), typeof(ScoreNote),
          new PropertyMetadata(double.NaN, OnXChanged));

        public static readonly DependencyProperty YProperty = DependencyProperty.Register(nameof(Y), typeof(double), typeof(ScoreNote),
          new PropertyMetadata(double.NaN, OnYChanged));

        private static void OnRadiusChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var note = obj as ScoreNote;
            Debug.Assert(note != null, "note != null");
            note.Width = note.Height = (double)e.NewValue * 2;
        }

        private static void OnIsSelectedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var note = obj as ScoreNote;
            Debug.Assert(note != null, "note != null");
            var newValue = (bool)e.NewValue;
            note.Stroke = newValue ? SelectedStrokeBrush : DefaultStrokeBrush;
            note.TextStroke = newValue ? SelectedStrokeBrush : DefaultTextStrokeBrush;
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
