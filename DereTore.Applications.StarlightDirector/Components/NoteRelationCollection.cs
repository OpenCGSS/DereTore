using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DereTore.Applications.StarlightDirector.UI.Controls;
using TupleType = System.Tuple<DereTore.Applications.StarlightDirector.UI.Controls.ScoreNote, DereTore.Applications.StarlightDirector.UI.Controls.ScoreNote>;
using InternalEntryType = System.Collections.Generic.KeyValuePair<System.Tuple<DereTore.Applications.StarlightDirector.UI.Controls.ScoreNote, DereTore.Applications.StarlightDirector.UI.Controls.ScoreNote>, DereTore.Applications.StarlightDirector.Components.NoteRelation>;

namespace DereTore.Applications.StarlightDirector.Components {
    public sealed class NoteRelationCollection : IEnumerable<NoteRelationCollection.Entry> {

        public NoteRelationCollection() {
            InternalDictionary = new Dictionary<TupleType, NoteRelation>();
        }

        public void Add(ScoreNote scoreNote1, ScoreNote scoreNote2, NoteRelation relation) {
            var tuple = new TupleType(scoreNote1, scoreNote2);
            InternalDictionary.Add(tuple, relation);
        }

        public int Remove(ScoreNote oneOf) {
            var contained = InternalDictionary.Where(kv => kv.Key.Item1.Equals(oneOf) || kv.Key.Item2.Equals(oneOf)).ToArray();
            var n = 0;
            foreach (var kv in contained) {
                InternalDictionary.Remove(kv.Key);
                ++n;
            }
            return n;
        }

        public int Remove(ScoreNote oneOf, NoteRelation relation) {
            var contained = InternalDictionary.Where(kv => (kv.Key.Item1.Equals(oneOf) || kv.Key.Item2.Equals(oneOf)) && kv.Value == relation).ToArray();
            var n = 0;
            foreach (var kv in contained) {
                InternalDictionary.Remove(kv.Key);
                ++n;
            }
            return n;
        }

        public void RemoveAll(Predicate<ScoreNote> match) {
            if (match == null) {
                throw new ArgumentNullException(nameof(match));
            }
            var keys = InternalDictionary.Keys.ToArray();
            foreach (var key in keys) {
                if (match(key.Item1) || match(key.Item2)) {
                    InternalDictionary.Remove(key);
                }
            }
        }

        public void RemoveAllRelated(ScoreNote scoreNote) {
            RemoveAll(s => s.Equals(scoreNote));
        }

        public bool ContainsNote(ScoreNote oneOf) {
            return InternalDictionary.Any(kv => kv.Key.Item1.Equals(oneOf) || kv.Key.Item2.Equals(oneOf));
        }

        public bool ContainsRelation(NoteRelation relation) {
            return InternalDictionary.Any(kv => kv.Value == relation);
        }

        public void Clear() {
            InternalDictionary.Clear();
        }

        public IEnumerator<Entry> GetEnumerator() {
            return new NoteRelationEnumerator(InternalDictionary.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public sealed class Entry {

            internal Entry(ScoreNote note1, ScoreNote note2, NoteRelation relation) {
                ScoreNote1 = note1;
                ScoreNote2 = note2;
                Relation = relation;
            }

            public ScoreNote ScoreNote1 { get; }
            public ScoreNote ScoreNote2 { get; }
            public NoteRelation Relation { get; }

        }

        private Dictionary<TupleType, NoteRelation> InternalDictionary { get; }

        private class NoteRelationEnumerator : IEnumerator<Entry> {

            public NoteRelationEnumerator(IEnumerator<InternalEntryType> enumerator) {
                Enumerator = enumerator;
            }

            public void Dispose() {
                Enumerator.Dispose();
            }

            public bool MoveNext() {
                return Enumerator.MoveNext();
            }

            public void Reset() {
                Enumerator.Reset();
            }

            public Entry Current {
                get {
                    var current = Enumerator.Current;
                    return new Entry(current.Key.Item1, current.Key.Item2, current.Value);
                }
            }

            object IEnumerator.Current => Current;

            private IEnumerator<InternalEntryType> Enumerator { get; }

        }

    }
}
