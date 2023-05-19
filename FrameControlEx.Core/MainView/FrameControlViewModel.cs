using FrameControlEx.Core.MainView.Scene;

namespace FrameControlEx.Core.MainView {
    public class FrameControlViewModel : BaseViewModel {
        public SceneDeckViewModel SceneDeck { get; }

        public IRenderSurface PreviewRenderSurface { get; }

        public FrameControlViewModel(IRenderSurface preview) {
            this.PreviewRenderSurface = preview;
            this.SceneDeck = new SceneDeckViewModel(this);
        }
    }
}