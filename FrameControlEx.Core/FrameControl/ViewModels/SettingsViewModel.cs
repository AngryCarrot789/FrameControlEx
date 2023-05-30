namespace FrameControlEx.Core.FrameControl.ViewModels {
    /// <summary>
    /// Contains settings for the frame control app. These settings can be templated so that users can easily switch between templates (e.g. different resolutions, fps, etc)
    /// </summary>
    public class SettingsViewModel : BaseViewModel {
        private int width;
        private int height;
        private int frameRate;

        /// <summary>
        /// The width of the output
        /// </summary>
        public int Width {
            get => this.width;
            set => this.RaisePropertyChanged(ref this.width, value);
        }

        /// <summary>
        /// The height of the output
        /// </summary>
        public int Height {
            get => this.height;
            set => this.RaisePropertyChanged(ref this.height, value);
        }

        /// <summary>
        /// The rendering frame rate
        /// </summary>
        public int FrameRate {
            get => this.frameRate;
            set => this.RaisePropertyChanged(ref this.frameRate, value);
        }

        public SettingsViewModel Clone() {
            return new SettingsViewModel() {
                width = this.width,
                height = this.height,
                frameRate = this.frameRate
            };
        }
    }
}