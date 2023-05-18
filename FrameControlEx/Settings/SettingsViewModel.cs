using FrameControlEx.Core;
using FrameControlEx.Core.Shortcuts.Managing;
using FrameControlEx.Core.Shortcuts.ViewModels;

namespace FrameControlEx.Settings {
    public class SettingsViewModel : BaseViewModel {
        private ShortcutManagerViewModel shortcutsManager;
        public ShortcutManagerViewModel ShortcutsManager {
            get => this.shortcutsManager;
            set => this.RaisePropertyChanged(ref this.shortcutsManager, value);
        }

        public SettingsViewModel() {
            this.ShortcutsManager = new ShortcutManagerViewModel(ShortcutManager.Instance);
        }
    }
}
