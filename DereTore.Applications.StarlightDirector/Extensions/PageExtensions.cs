using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using DereTore.Applications.StarlightDirector.UI.Pages;
using DereTore.Applications.StarlightDirector.UI.Windows;

namespace DereTore.Applications.StarlightDirector.Extensions {
    internal static class PageExtensions {

        public static MainWindow FindMainWindow<T>(this T page) where T : Page, IDirectorPage {
            var mainWindowType = typeof(MainWindow);
            var parent = page.GetVisualParent();
            while (parent != null) {
                if (parent.GetType() == mainWindowType) {
                    return parent as MainWindow;
                }
                parent = parent.GetVisualParent();
            }
            return null;
        }

        private static FrameworkElement GetVisualParent(this FrameworkElement element) {
            return VisualParentPropInfo.GetValue(element, null) as FrameworkElement;
        }

        private static readonly PropertyInfo VisualParentPropInfo = typeof(FrameworkElement).GetProperty("VisualParent", BindingFlags.Instance | BindingFlags.NonPublic);

    }
}
