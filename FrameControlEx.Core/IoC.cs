using System;
using System.Diagnostics;
using System.Reflection;
using FrameControlEx.Core.Actions;
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

        public static T Provide<T>() {
            return Instance.Provide<T>();
        }

        public static ActionManager ActionManager { get; } = new ActionManager();
        public static ShortcutManager ShortcutManager { get; set; }
        public static IShortcutManagerDialogService ShortcutManagerDialog => Provide<IShortcutManagerDialogService>();
        public static Action<string> OnShortcutModified { get; set; }
        public static Action<string> BroadcastShortcutActivity { get; set; }

        public static IDispatcher Dispatcher { get; set; }
        public static IClipboardService Clipboard => Provide<IClipboardService>();
        public static IMessageDialogService MessageDialogs => Provide<IMessageDialogService>();
        public static IFilePickDialogService FilePicker => Provide<IFilePickDialogService>();
        public static IUserInputDialogService UserInput => Provide<IUserInputDialogService>();
        public static IExplorerService ExplorerService => Provide<IExplorerService>();
        public static IKeyboardDialogService KeyboardDialogs => Provide<IKeyboardDialogService>();
        public static IMouseDialogService MouseDialogs => Provide<IMouseDialogService>();
        public static IOutputSelector BufferSelector => Provide<IOutputSelector>();

        public static SettingsManagerViewModel Settings { get; } = new SettingsManagerViewModel();

        static IoC() {
        }

        public static void Init() {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (TypeInfo typeInfo in assembly.DefinedTypes) {
                    ServiceAttribute attribute = typeInfo.GetCustomAttribute<ServiceAttribute>();
                    if (attribute != null && attribute.Type != null) {
                        object instance;
                        try {
                            instance = Activator.CreateInstance(typeInfo);
                        }
                        catch (Exception e) {
                            Debug.WriteLine($"Failed to create implementation of {attribute.Type} as {typeInfo}");
                            Debug.WriteLine(e);
                            continue;
                        }

                        Instance.Register(attribute.Type, instance);
                    }
                }
            }
        }

        public static bool IsAppRunning {
            get => isAppRunning;
            set => isAppRunning = value;
        }
    }
}