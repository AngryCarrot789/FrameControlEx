using SkiaSharp;

namespace FrameControlEx.Core.MainView.Scene.Outputs {
    /// <summary>
    /// A visual output that just takes a snapshot of the fully rendered output
    /// </summary>
    public class BasicBufferOutputViewModel : VisualOutputViewModel {
        public SKImage lastFrame;

        public BasicBufferOutputViewModel() {

        }

        public override void OnAcceptFrame(SKSurface surface) {
            base.OnAcceptFrame(surface);
            this.lastFrame = surface.Snapshot();
        }

        protected override BaseIOViewModel CreateInstanceCore() {
            return new BasicBufferOutputViewModel();
        }
    }
}