using System.Collections.Generic;
using System.Threading.Tasks;
using FrameControlEx.Core.Views.Dialogs.Message;
using FrameControlEx.Core.Views.Dialogs.UserInputs;
using SkiaSharp;

namespace FrameControlEx.Core.MainView.Scene.Outputs {
    /// <summary>
    /// A visual output that just takes a snapshot of the fully rendered output
    /// </summary>
    public class BasicBufferOutputViewModel : VisualOutputViewModel {
        public SKImage lastFrame;

        public BasicBufferOutputViewModel(OutputDeckViewModel deck) : base(deck) {

        }

        public override void OnAcceptFrame(SKSurface surface) {
            base.OnAcceptFrame(surface);
            this.lastFrame = surface.Snapshot();
        }
    }
}