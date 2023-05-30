using System.Threading;
using FrameControlEx.Core.FrameControl.Models.Scene.Outputs.Base;
using SkiaSharp;

namespace FrameControlEx.Core.FrameControl.Models.Scene.Outputs {
    /// <summary>
    /// An output that saves the output frame into a snapshot image that a source can access
    /// </summary>
    public class BufferedOutputModel : AVOutputModel {
        public SKImage lastFrame;

        private volatile int usageCount;

        public void Use() {
            Interlocked.Increment(ref this.usageCount);
        }

        public void Unuse() {
            Interlocked.Decrement(ref this.usageCount);
        }

        public override void OnAcceptFrame(RenderContext context) {
            base.OnAcceptFrame(context);

            // the GC goes nuts because of this; like 1 gig of memory used every second
            if (this.usageCount > 0) {
                this.lastFrame = context.Surface.Snapshot();
            }
            else if (this.usageCount < 0) { // just in case..
                this.usageCount = 0;
            }
        }
    }
}