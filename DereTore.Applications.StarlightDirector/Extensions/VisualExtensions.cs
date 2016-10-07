using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.UI.Pages;
using DereTore.Applications.StarlightDirector.UI.Windows;

namespace DereTore.Applications.StarlightDirector.Extensions {
    internal static class VisualExtensions {

        public static TParent FindVisualParent<TParent>(this FrameworkElement element) where TParent : FrameworkElement {
            var mainWindowType = typeof(TParent);
            var parent = element.GetVisualParent();
            while (parent != null) {
                if (parent.GetType() == mainWindowType) {
                    return parent as TParent;
                }
                parent = parent.GetVisualParent();
            }
            return null;
        }

        public static MainWindow FindMainWindow<T>(this T page) where T : Page, IDirectorPage {
            return FindVisualParent<MainWindow>(page);
        }

        private static FrameworkElement GetVisualParent(this FrameworkElement element) {
            return VisualParentPropInfo.GetValue(element, null) as FrameworkElement;
        }

        private static readonly PropertyInfo VisualParentPropInfo = typeof(FrameworkElement).GetProperty("VisualParent", BindingFlags.Instance | BindingFlags.NonPublic);

    }
}
