using System;

namespace DereTore {
    public static class EventExtensions {

        public static void Raise<T>(this EventHandler<T> handler, object sender, T e) where T : EventArgs {
            handler?.Invoke(sender, e);
        }

        public static void Raise(this EventHandler handler, object sender, EventArgs e) {
            handler?.Invoke(sender, e);
        }

        public static IAsyncResult RaiseAsync<T>(this EventHandler<T> handler, object sender, T e) where T : EventArgs {
            return handler?.BeginInvoke(sender, e, handler.EndInvoke, null);
        }

        public static IAsyncResult RaiseAsync(this EventHandler handler, object sender, EventArgs e) {
            return handler?.BeginInvoke(sender, e, handler.EndInvoke, null);
        }

    }
}
