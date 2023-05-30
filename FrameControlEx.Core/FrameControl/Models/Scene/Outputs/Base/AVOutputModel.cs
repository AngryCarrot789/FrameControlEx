namespace FrameControlEx.Core.FrameControl.Models.Scene.Outputs.Base {
    public abstract class AVOutputModel : OutputModel, IVisualOutput {
        protected AVOutputModel() {

        }

        public virtual void OnAcceptFrame(RenderContext context) {

        }
    }
}