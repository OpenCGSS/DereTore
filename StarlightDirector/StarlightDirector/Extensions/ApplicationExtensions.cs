using System.Windows;

namespace StarlightDirector.Extensions {
    public static class ApplicationExtensions {

        public static T FindResource<T>(this Application application, string key) {
            return (T)application.FindResource(key);
        }

        public static T TryFindResource<T>(this Application application, string key) where T : class {
            return application.TryFindResource(key) as T;
        }

    }
}
