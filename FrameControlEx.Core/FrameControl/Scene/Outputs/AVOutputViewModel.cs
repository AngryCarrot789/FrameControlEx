namespace FrameControlEx.Core.FrameControl.Scene.Outputs {
    /// <summary>
    /// Base class for visual outputs (videos, images). These receive the fully rendered output
    /// frame and can do whatever they want with it (they really shouldn't modify the frame though)
    /// </summary>
    public abstract class AVOutputViewModel : OutputViewModel, IVisualOutput {
        protected AVOutputViewModel() {

        }

        public virtual void OnAcceptFrame(RenderContext context) {

        }
    }
}