using System;
using System.Collections.Generic;

namespace DereTore.Applications.StarlightDirector.Components {
    public abstract class ObjectPool<T> where T : class, new() {

        protected ObjectPool() {
            Pool = new List<T>();
        }

        public T Request() {
            throw new NotImplementedException();
        }

        public void Remove(T t) {
            
        }

        public void Clear() {
            Pool.Clear();
        }

        protected abstract void RemoveObject(T t);

        protected void DisposeObject(T t) {
            (t as IDisposable)?.Dispose();
        }

        private List<T> Pool { get; }

    }
}
