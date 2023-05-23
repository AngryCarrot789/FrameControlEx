using SkiaSharp;

namespace FrameControlEx.Core.FrameControl.Scene.Outputs {
    /// <summary>
    /// A visual output that just takes a snapshot of the fully rendered output
    /// </summary>
    public class BasicBufferOutputViewModel : AVOutputViewModel {
        public SKImage lastFrame;

        public BasicBufferOutputViewModel() {

        }

        public override void OnAcceptFrame(SKSurface surface, in SKImageInfo frameInfo) {
            base.OnAcceptFrame(surface, frameInfo);

            // the GC goes nuts because of this; like 1 gig of memory used every second
            this.lastFrame = surface.Snapshot();
        }

        protected override BaseIOViewModel CreateInstanceCore() {
            return new BasicBufferOutputViewModel();
        }
    }
}