using FrameControlEx.Core.MainView.Scene;

namespace FrameControlEx.Core.MainView {
    public class FrameControlViewModel : BaseViewModel {
        public SceneDeckViewModel SceneDeck { get; }

        public FrameControlViewModel() {
            this.SceneDeck = new SceneDeckViewModel(this);
        }
    }
}