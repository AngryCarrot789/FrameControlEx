using FrameControlEx.Core.FrameControl.Models.Scene.Outputs.Base;

namespace FrameControlEx.Core.FrameControl.ViewModels.Scene.Outputs {
    /// <summary>
    /// Base class for visual outputs (videos, images). These receive the fully rendered output
    /// frame and can do whatever they want with it (they really shouldn't modify the frame though)
    /// </summary>
    public abstract class AVOutputViewModel : OutputViewModel {
        public new AVOutputModel Model => (AVOutputModel) ((BaseIOViewModel) this).Model;

        protected AVOutputViewModel(AVOutputModel model) : base(model) {

        }
    }
}