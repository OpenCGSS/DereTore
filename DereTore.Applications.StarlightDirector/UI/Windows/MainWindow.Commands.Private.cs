using System;
using System.Linq;
using System.Windows.Input;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    partial class MainWindow {

        private static RoutedCommand RC(params string[] gestures) {
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

    }
}
