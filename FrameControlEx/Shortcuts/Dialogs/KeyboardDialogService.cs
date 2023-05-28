using FrameControlEx.Core;
using FrameControlEx.Core.Shortcuts.Dialogs;
using FrameControlEx.Core.Shortcuts.Inputs;

namespace FrameControlEx.Shortcuts.Dialogs {
    [Service(typeof(IKeyboardDialogService))]
    public class KeyboardDialogService : IKeyboardDialogService {
        public KeyStroke? ShowGetKeyStrokeDialog() {
            KeyStrokeInputWindow window = new KeyStrokeInputWindow();
            if (window.ShowDialog() != true || window.Stroke.Equals(default)) {
                return null;
            }
            else {
                return window.Stroke;
            }
        }
    }
}