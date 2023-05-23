using SkiaSharp;

namespace FrameControlEx.Core.FrameControl.Scene.Outputs {
    /// <summary>
    /// Base class for visual outputs (videos, images). These receive the fully rendered output
    /// frame and can do whatever they want with it (they really shouldn't modify the frame though)
    /// </summary>
    public abstract class VisualOutputViewModel : OutputViewModel, IVisualOutput {
        protected VisualOutputViewModel() {

        }

        public virtual void OnAcceptFrame(SKSurface surface, in SKImageInfo frameInfo) {

        }
    }
}