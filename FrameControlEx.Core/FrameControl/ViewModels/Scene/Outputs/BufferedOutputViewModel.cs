using FrameControlEx.Core.FrameControl.Models.Scene.Outputs;

namespace FrameControlEx.Core.FrameControl.ViewModels.Scene.Outputs {
    /// <summary>
    /// A visual output that just takes a snapshot of the fully rendered output
    /// </summary>
    public class BufferedOutputViewModel : AVOutputViewModel {
        public new BufferedOutputModel Model => (BufferedOutputModel) base.Model;

        public BufferedOutputViewModel(BufferedOutputModel model) : base(model) {

        }
    }
}