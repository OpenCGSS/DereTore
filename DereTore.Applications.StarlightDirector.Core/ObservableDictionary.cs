using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DereTore.Applications.StarlightDirector {
    // https://richardwilburn.wordpress.com/2009/07/13/observable-dictionary/
    // Modifications are made based on testing.
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged {

        public ObservableDictionary() {
        }

        public ObservableDictionary(IDictionary<TKey, TValue> dictionary) {
            _dictionary = new Dictionary<TKey, TValue>(dictionary);
        }

        public ObservableDictionary(int capacity) {
            _dictionary = new Dictionary<TKey, TValue>(capacity);
        }

        public ObservableDictionary(IEqualityComparer<TKey> comparer) {
            _dictionary = new Dictionary<TKey, TValue>(comparer);
        }

        public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) {
            _dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Add(KeyValuePair<TKey, TValue> item) {
            _dictionary.Add(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Keys)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Values)));
            }
        }

        public void Clear() {
            var keysCount = _dictionary.Keys.Count;
            _dictionary.Clear();
            if (keysCount == 0) {
                return; //dont trigger changed event if there was no change.
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Keys)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Values)));
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            return _dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            _dictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            var remove = _dictionary.Remove(item);
            if (!remove) {
                return false; //don’t trigger change events if there was no change.
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Keys)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Values)));
            }
            return true;
        }

        public int Count => _dictionary.Count;

        public bool IsReadOnly => _dictionary.IsReadOnly;

        public bool ContainsKey(TKey key) {
            return _dictionary.ContainsKey(key);
        }

        public void Add(TKey key, TValue value) {
            var kv = new KeyValuePair<TKey, TValue>(key, value);
            _dictionary.Add(kv);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, kv));
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Keys)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Values)));
            }
        }

        public bool Remove(TKey key) {
            TValue value;
            _dictionary.TryGetValue(key, out value);
            var remove = _dictionary.Remove(key);
            if (!remove) {
                return false;
            }
            var kv = new KeyValuePair<TKey, TValue>(key, value);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, kv));
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Keys)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Values)));
            }
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value) {
            return _dictionary.TryGetValue(key, out value);
        }

        public TValue this[TKey key] {
            get { return _dictionary[key]; }
            set {
                var changed = !_dictionary.ContainsKey(key) || _dictionary[key].Equals(value);
                if (!changed) {
                    return; //if there are no changes then we don’t need to update the value or trigger changed events.
                }
                TValue oldValue;
                var hasOldValue = _dictionary.TryGetValue(key, out oldValue);
                _dictionary[key] = value;
                var kvNew = new KeyValuePair<TKey, TValue>(key, value);
                if (hasOldValue) {
                    var kvOld = new KeyValuePair<TKey, TValue>(key, oldValue);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, kvNew, kvOld));
                } else {
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, kvNew));
                }
                if (PropertyChanged != null) {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Keys)));
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Values)));
                }
            }
        }

        public ICollection<TKey> Keys => _dictionary.Keys;

        public ICollection<TValue> Values => _dictionary.Values;

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        private readonly IDictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

    }
}
