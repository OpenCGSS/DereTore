using System;
using System.Windows;
using System.Windows.Controls;
using DereTore.Applications.StarlightDirector.UI.Pages;
using DereTore.Applications.StarlightDirector.UI.Windows;

namespace DereTore.Applications.StarlightDirector.Extensions {
    internal static class PageExtensions {

        public static MainWindow FindMainWindow<T>(this T page) where T : Page, IDirectorPage {
            var mainWindowType = typeof(MainWindow);
            var parent = page.Parent as FrameworkElement;
            while (parent != null) {
                if (parent.GetType() == mainWindowType) {
                    return parent as MainWindow;
                }
                parent = parent.Parent as FrameworkElement;
            }
            return null;
        }

    }
}
