namespace FrameControlEx.Core.MainView {
    /// <summary>
    /// Contains settings for the frame control app. These settings can be templated so that users can easily switch between templates (e.g. different resolutions, fps, etc)
    /// </summary>
    public class FrameControlSettingsViewModel : BaseViewModel {
        private int width;
        private int height;

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
    }
}