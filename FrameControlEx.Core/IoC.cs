using System;
using FrameControlEx.Core.Actions;
using FrameControlEx.Core.FrameControl;
using FrameControlEx.Core.Services;
using FrameControlEx.Core.Settings;
using FrameControlEx.Core.Shortcuts.Dialogs;
using FrameControlEx.Core.Shortcuts.Managing;
using FrameControlEx.Core.Views.Dialogs.FilePicking;
using FrameControlEx.Core.Views.Dialogs.Message;
using FrameControlEx.Core.Views.Dialogs.UserInputs;

namespace FrameControlEx.Core {
    public static class IoC {
        private static volatile bool isAppRunning = true;

        public static SimpleIoC Instance { get; } = new SimpleIoC();

        public static ActionManager ActionManager { get; } = new ActionManager();
        public static ShortcutManager ShortcutManager { get; set; }
        public static IShortcutManagerDialogService ShortcutManagerDialog { get; set; }
        public static Action<string> OnShortcutModified { get; set; }
        public static Action<string> BroadcastShortcutActivity { get; set; }

        public static IDispatcher Dispatcher { get; set; }
        public static IClipboardService Clipboard { get; set; }
        public static IMessageDialogService MessageDialogs { get; set; }
        public static IFilePickDialogService FilePicker { get; set; }
        public static IUserInputDialogService UserInput { get; set; }
        public static IExplorerService ExplorerService { get; set; }
        public static IKeyboardDialogService KeyboardDialogs { get; set; }
        public static IMouseDialogService MouseDialogs { get; set; }
        public static IOutputSelector BufferSelector { get; set; }

        public static SettingsManagerViewModel Settings { get; } = new SettingsManagerViewModel();

        static IoC() {
        }

        public static bool IsAppRunning {
            get => isAppRunning;
            set => isAppRunning = value;
        }
    }
}