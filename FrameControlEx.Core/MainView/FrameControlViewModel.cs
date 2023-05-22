using FrameControlEx.Core.MainView.Scene;

namespace FrameControlEx.Core.MainView {
    public class FrameControlViewModel : BaseViewModel {
        private int width = 1920;
        private int height = 1080;

        public int Width {
            get => this.width;
            set => this.RaisePropertyChanged(ref this.width, value);
        }

        public int Height {
            get => this.height;
            set => this.RaisePropertyChanged(ref this.height, value);
        }

        private FrameControlSettingsViewModel settings;
        public FrameControlSettingsViewModel Settings {
            get => this.settings;
            set => this.RaisePropertyChanged(ref this.settings, value);
        }

        public SceneDeckViewModel SceneDeck { get; }

        public OutputDeckViewModel OutputDeck { get; }

        public FrameControlViewModel() {
            this.SceneDeck = new SceneDeckViewModel(this);
            this.OutputDeck = new OutputDeckViewModel(this);
        }
    }
}