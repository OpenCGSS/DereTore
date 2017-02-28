using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StarlightDirector {
    public class InternalList<T> : IList<T> {

        public InternalList() {
            _list = new List<T>();
        }

        public InternalList(int capacity) {
            _list = new List<T>(capacity);
        }

        public InternalList(IEnumerable<T> collection) {
            _list = new List<T>(collection);
        }

        public ReadOnlyCollection<T> AsReadOnly() {
            return _list.AsReadOnly();
        }

        public int BinarySearch(T item) {
            return _list.BinarySearch(item);
        }

        public int BinarySearch(T item, IComparer<T> comparer) {
            return _list.BinarySearch(item, comparer);
        }

        public int BinarySearch(int index, int count, T item, IComparer<T> comparer) {
            return _list.BinarySearch(index, count, item, comparer);
        }

        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter) {
            return _list.ConvertAll(converter);
        }

        public bool Exists(Predicate<T> match) {
            return _list.Exists(match);
        }

        public T Find(Predicate<T> match) {
            return _list.Find(match);
        }

        public IList<T> FindAll(Predicate<T> match) {
            return _list.FindAll(match);
        }

        public int FindIndex(Predicate<T> match) {
            return _list.FindIndex(match);
        }

        public int FindIndex(int startIndex, Predicate<T> match) {
            return _list.FindIndex(startIndex, match);
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match) {
            return _list.FindIndex(startIndex, count, match);
        }

        public T FindLast(Predicate<T> match) {
            return _list.FindLast(match);
        }

        public int FindLastIndex(Predicate<T> match) {
            return _list.FindLastIndex(match);
        }

        public int FindLastIndex(int startIndex, Predicate<T> match) {
            return _list.FindLastIndex(startIndex, match);
        }

        public int FindLastIndex(int startIndex, int count, Predicate<T> match) {
            return _list.FindLastIndex(startIndex, count, match);
        }

        public void ForEach(Action<T> action) {
            _list.ForEach(action);
        }

        public IList<T> GetRange(int index, int count) {
            return _list.GetRange(index, count);
        }

        public int IndexOf(T item, int index) {
            return _list.IndexOf(item, index);
        }

        public int IndexOf(T item, int index, int count) {
            return _list.IndexOf(item, index, count);
        }

        public void InsertRange(int index, IEnumerable<T> collection) {
            _list.InsertRange(index, collection);
        }

        public int LastIndexOf(T item) {
            return _list.LastIndexOf(item);
        }

        public int LastIndexOf(T item, int index) {
            return _list.LastIndexOf(item, index);
        }

        public int LastIndexOf(T item, int index, int count) {
            return _list.LastIndexOf(item, index, count);
        }

        public int RemoveAll(Predicate<T> match) {
            return _list.RemoveAll(match);
        }

        public void RemoveRange(int index, int count) {
            _list.RemoveRange(index, count);
        }

        public void Reverse() {
            _list.Reverse();
        }

        public void Reverse(int index, int count) {
            _list.Reverse(index, count);
        }

        public void Sort() {
            _list.Sort();
        }

        public void Sort(Comparison<T> comparison) {
            _list.Sort(comparison);
        }

        public void Sort(IComparer<T> comparer) {
            _list.Sort(comparer);
        }

        public void Sort(int index, int count, IComparer<T> comparer) {
            _list.Sort(index, count, comparer);
        }

        public T[] ToArray() {
            return _list.ToArray();
        }

        public void TrimExcess() {
            _list.TrimExcess();
        }

        public bool TrueForAll(Predicate<T> match) {
            return _list.TrueForAll(match);
        }

        public bool Contains(T item) {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array) {
            _list.CopyTo(array);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            _list.CopyTo(array, arrayIndex);
        }

        public void CopyTo(int index, T[] array, int arrayIndex, int count) {
            _list.CopyTo(index, array, arrayIndex, count);
        }

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public int IndexOf(T item) {
            return _list.IndexOf(item);
        }

        public T this[int index] {
            get { return _list[index]; }
            internal set { _list[index] = value; }
        }

        public IEnumerator<T> GetEnumerator() {
            return _list.GetEnumerator();
        }

        internal void Add(T item) {
            _list.Add(item);
        }

        internal void AddRange(IEnumerable<T> collection) {
            _list.AddRange(collection);
        }

        internal void Clear() {
            _list.Clear();
        }

        internal bool Remove(T item) {
            return _list.Remove(item);
        }

        internal void Insert(int index, T item) {
            _list.Insert(index, item);
        }

        internal void RemoveAt(int index) {
            _list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        void ICollection<T>.Add(T item) {
            Add(item);
        }

        void ICollection<T>.Clear() {
            Clear();
        }

        bool ICollection<T>.Remove(T item) {
            return Remove(item);
        }

        void IList<T>.Insert(int index, T item) {
            Insert(index, item);
        }

        void IList<T>.RemoveAt(int index) {
            RemoveAt(index);
        }

        T IList<T>.this[int index] {
            get { return this[index]; }
            set { this[index] = value; }
        }

        private readonly List<T> _list;

    }
}

