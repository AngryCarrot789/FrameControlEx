using FrameControlEx.Core.Utils;

namespace FrameControlEx.Core.FrameControl.ViewModels {
    /// <summary>
    /// Contains settings for the frame control app. These settings can be templated so that users can easily switch between templates (e.g. different resolutions, fps, etc)
    /// </summary>
    public class SettingsViewModel : BaseViewModel {
        private Vec2i viewPortSize;
        private int frameRate;

        /// <summary>
        /// The width of the output
        /// </summary>
        public int Width {
            get => this.viewPortSize.X;
            set => this.ViewPortSize = new Vec2i(value, this.viewPortSize.Y);
        }

        /// <summary>
        /// The height of the output
        /// </summary>
        public int Height {
            get => this.viewPortSize.Y;
            set => this.ViewPortSize = new Vec2i(this.viewPortSize.X, value);
        }

        public Vec2i ViewPortSize {
            get => this.viewPortSize;
            set {
                this.RaisePropertyChanged(ref this.viewPortSize, value);
                this.RaisePropertyChanged(nameof(this.Width));
                this.RaisePropertyChanged(nameof(this.Height));
            }
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
                viewPortSize = this.viewPortSize,
                frameRate = this.frameRate
            };
        }
    }
}