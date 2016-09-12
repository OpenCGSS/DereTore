using System;

namespace DereTore {
    public static class EventExtensions {

        public static void Raise<T>(this EventHandler<T> handler, object sender, T e) where T : EventArgs {

        }

        public static void Raise(this EventHandler handler, object sender, EventArgs e) {

        }

    }
}
