using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using StarlightDirector.UI.Windows;

namespace StarlightDirector {
    public static class CommandHelper {

        public static RoutedCommand RegisterCommand(params string[] gestures) {
            var command = new RoutedCommand(Guid.NewGuid().ToString(), typeof(MainWindow));
            if (gestures.Length <= 0) {
                return command;
            }
            foreach (var gesture in gestures) {
                Key key;
                var modifierKeys = ModifierKeys.None;
                var parts = gesture.Split('+');
                if (parts.Length > 1) {
                    foreach (var part in parts.Take(parts.Length - 1)) {
                        var lowerCasePart = part.ToLowerInvariant();
                        switch (lowerCasePart) {
                            case "ctrl":
                                modifierKeys |= ModifierKeys.Control;
                                break;
                            case "win":
                                modifierKeys |= ModifierKeys.Windows;
                                break;
                            default:
                                var mod = (ModifierKeys)Enum.Parse(typeof(ModifierKeys), lowerCasePart, true);
                                modifierKeys |= mod;
                                break;
                        }
                    }
                }
                var lastPart = parts[parts.Length - 1];
                uint dummy;
                if (uint.TryParse(lastPart, out dummy) && dummy <= 9) {
                    key = (Key)((int)Key.D0 + dummy);
                } else {
                    key = (Key)Enum.Parse(typeof(Key), lastPart, true);
                }
                command.InputGestures.Add(new KeyGesture(key, modifierKeys));
            }
            return command;
        }

        public static void InitializeCommandBindings(FrameworkElement element) {
            var cb = element.CommandBindings;

            var thisType = element.GetType();
            var icommandType = typeof(ICommand);
            var commandFields = thisType.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var commandField in commandFields) {
                if (commandField.FieldType != icommandType && !commandField.FieldType.IsSubclassOf(icommandType)) {
                    continue;
                }
                var command = (ICommand)commandField.GetValue(null);
                var name = commandField.Name;
                var executedHandlerInfo = thisType.GetMethod(name + "_Executed", BindingFlags.NonPublic | BindingFlags.Instance);
                var executedHandler = (ExecutedRoutedEventHandler)Delegate.CreateDelegate(typeof(ExecutedRoutedEventHandler), element, executedHandlerInfo);
                var canExecuteHandlerInfo = thisType.GetMethod(name + "_CanExecute", BindingFlags.NonPublic | BindingFlags.Instance);
                var canExecuteHandler = (CanExecuteRoutedEventHandler)Delegate.CreateDelegate(typeof(CanExecuteRoutedEventHandler), element, canExecuteHandlerInfo);
                cb.Add(new CommandBinding(command, executedHandler, canExecuteHandler));
            }
        }

    }
}
