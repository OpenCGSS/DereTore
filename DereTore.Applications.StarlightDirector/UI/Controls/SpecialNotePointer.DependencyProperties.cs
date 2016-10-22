using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    partial class SpecialNotePointer {

        public Note Note {
            get { return (Note)GetValue(NoteProperty); }
            set { SetValue(NoteProperty, value); }
        }

        public double X {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        public double Y {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        public static readonly DependencyProperty NoteProperty = DependencyProperty.Register(nameof(Note), typeof(Note), typeof(SpecialNotePointer),
            new PropertyMetadata(null));

        public static readonly DependencyProperty XProperty = DependencyProperty.Register(nameof(X), typeof(double), typeof(SpecialNotePointer),
          new PropertyMetadata(0d, OnXChanged));

        public static readonly DependencyProperty YProperty = DependencyProperty.Register(nameof(Y), typeof(double), typeof(SpecialNotePointer),
          new PropertyMetadata(0d, OnYChanged));

        private static void OnXChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var specialNotePointer = obj as SpecialNotePointer;
            Debug.Assert(specialNotePointer != null, "specialNotePointer != null");
            if (specialNotePointer.VisualParent is Canvas) {
                var value = (double)e.NewValue;
                Canvas.SetLeft(specialNotePointer, value);
            } else {
                Debug.Print("The SpecialNotePointer is expected to be put on a Canvas.");
            }
        }

        private static void OnYChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var specialNotePointer = obj as SpecialNotePointer;
            Debug.Assert(specialNotePointer != null, "specialNotePointer != null");
            if (specialNotePointer.VisualParent is Canvas) {
                var value = (double)e.NewValue;
                Canvas.SetTop(specialNotePointer, value);
            } else {
                Debug.Print("The SpecialNotePointer is expected to be put on a Canvas.");
            }
        }

    }
}
