using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using StarlightDirector.Entities;

namespace StarlightDirector.UI.Controls.Primitives {
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
            new PropertyMetadata(null, OnNoteChanged));

        public static readonly DependencyProperty XProperty = DependencyProperty.Register(nameof(X), typeof(double), typeof(SpecialNotePointer),
          new PropertyMetadata(0d, OnXChanged));

        public static readonly DependencyProperty YProperty = DependencyProperty.Register(nameof(Y), typeof(double), typeof(SpecialNotePointer),
          new PropertyMetadata(0d, OnYChanged));

        private static void OnNoteChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var specialNotePointer = (SpecialNotePointer)obj;
            var oldNote = (Note)e.OldValue;
            var newNote = (Note)e.NewValue;
            if (oldNote != null) {
                oldNote.ExtraParamsChanged -= specialNotePointer.Note_ExtraParamsChanged;
            }
            if (newNote != null) {
                newNote.ExtraParamsChanged += specialNotePointer.Note_ExtraParamsChanged;
            }
        }

        private static void OnXChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var specialNotePointer = (SpecialNotePointer)obj;
            if (specialNotePointer.VisualParent is Canvas) {
                var value = (double)e.NewValue;
                Canvas.SetLeft(specialNotePointer, value);
            } else {
                Debug.Print("The SpecialNotePointer is expected to be put on a Canvas.");
            }
        }

        private static void OnYChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var specialNotePointer = (SpecialNotePointer)obj;
            if (specialNotePointer.VisualParent is Canvas) {
                var value = (double)e.NewValue;
                Canvas.SetTop(specialNotePointer, value);
            } else {
                Debug.Print("The SpecialNotePointer is expected to be put on a Canvas.");
            }
        }

    }
}
