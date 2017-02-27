using System;
using System.Globalization;
using System.Windows;
using DereTore;

namespace StarlightDirector.Entities {
    public sealed class NoteExtraParams : DependencyObject {

        public event EventHandler<EventArgs> ParamsChanged;

        public double NewBpm {
            get { return (double)GetValue(NewBpmProperty); }
            set { SetValue(NewBpmProperty, value); }
        }

        public static readonly DependencyProperty NewBpmProperty = DependencyProperty.Register(nameof(NewBpm), typeof(double), typeof(NoteExtraParams),
            new PropertyMetadata(120d, OnNewBpmChanged));

        public string GetDataString() {
            switch (Note.Type) {
                case NoteType.VariantBpm:
                    return NewBpm.ToString(CultureInfo.InvariantCulture);
                default:
                    throw new ArgumentOutOfRangeException(nameof(Note.Type));
            }
        }

        public void UpdateByDataString(string s) {
            UpdateByDataString(s, Note);
        }

        public void UpdateByDataString(string s, Note note) {
            Note = note;
            switch (note.Type) {
                case NoteType.VariantBpm:
                    NewBpm = double.Parse(s);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(note.Type));
            }
        }

        public static NoteExtraParams FromDataString(string str, Note note) {
            if (string.IsNullOrEmpty(str)) {
                return null;
            }
            var p = new NoteExtraParams {
                Note = note
            };
            switch (note.Type) {
                case NoteType.VariantBpm:
                    p.NewBpm = double.Parse(str);
                    break;
                default:
                    break;
            }
            return p;
        }

        public Note Note { get; internal set; }

        private static void OnNewBpmChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var p = (NoteExtraParams)obj;
            p.Note.Bar.Score.Project.IsChanged = true;
            p.ParamsChanged.Raise(p, EventArgs.Empty);
        }

    }
}
