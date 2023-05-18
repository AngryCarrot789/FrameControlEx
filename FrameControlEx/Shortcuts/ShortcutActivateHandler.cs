using System.Threading.Tasks;
using FrameControlEx.Core.Shortcuts.Managing;

namespace FrameControlEx.Shortcuts {
    public delegate Task<bool> ShortcutActivateHandler(ShortcutProcessor processor, GroupedShortcut shortcut);
}