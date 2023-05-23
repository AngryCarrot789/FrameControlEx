using System;
using FrameControlEx.Core.FrameControl;

namespace FrameControlEx.Core.Settings {
    public class SettingsManagerViewModel : BaseViewModel {
        public delegate void UpdateSettingsEventHandler(SettingsViewModel settings);

        private SettingsViewModel activeSettings;
        public SettingsViewModel ActiveSettings {
            get => this.activeSettings;
            private set {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                this.RaisePropertyChanged(ref this.activeSettings, value);
                this.OnSettingsModified?.Invoke(value);
            }
        }

        public event UpdateSettingsEventHandler OnSettingsModified;

        public SettingsManagerViewModel() {
            this.ActiveSettings = new SettingsViewModel {
                Width = 1920,
                Height = 1080,
                FrameRate = 60
            };
        }
    }
}