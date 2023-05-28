using FrameControlEx.Core;
using FrameControlEx.Core.Shortcuts.Dialogs;
using FrameControlEx.Core.Shortcuts.Inputs;

namespace FrameControlEx.Shortcuts.Dialogs {
    [Service(typeof(IMouseDialogService))]
    public class MouseDialogService : IMouseDialogService {
        public MouseStroke? ShowGetMouseStrokeDialog() {
            MouseStrokeInputWindow window = new MouseStrokeInputWindow();
            if (window.ShowDialog() != true || window.Stroke.Equals(default)) {
                return null;
            }
            else {
                return window.Stroke;
            }
        }
    }
}