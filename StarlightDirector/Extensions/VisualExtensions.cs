using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using StarlightDirector.UI.Controls.Pages;
using StarlightDirector.UI.Windows;

namespace StarlightDirector.Extensions {
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

        public static MainWindow GetMainWindow<T>(this T page) where T : UserControl, IDirectorPage {
            return FindVisualParent<MainWindow>(page);
        }

        private static FrameworkElement GetVisualParent(this FrameworkElement element) {
            return VisualParentPropInfo.GetValue(element, null) as FrameworkElement;
        }

        private static readonly PropertyInfo VisualParentPropInfo = typeof(FrameworkElement).GetProperty("VisualParent", BindingFlags.Instance | BindingFlags.NonPublic);

    }
}
