using System.Windows.Input;

namespace DereTore.Applications.StarlightDirector.Extensions {
    public static class CommandExtensions {

        public static void RaiseCanExecuteChanged<T>(this T command) where T : ICommand {
            //var type = typeof(T);
            //var ev = type.GetEvent("CanExecute");
            //var method = ev?.GetRaiseMethod(true);
            //method?.Invoke(command, NullObjects);
            // TODO: This method is quite "heavy". It causes all commands to reevaluate there CanExecute status.
            CommandManager.InvalidateRequerySuggested();
        }

        //private static readonly object[] NullObjects = new object[0];

    }
}
