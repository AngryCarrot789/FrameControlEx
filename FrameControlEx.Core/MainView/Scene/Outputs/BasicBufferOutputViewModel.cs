using System.Collections.Generic;
using System.Threading.Tasks;
using FrameControlEx.Core.Views.Dialogs.Message;
using FrameControlEx.Core.Views.Dialogs.UserInputs;
using SkiaSharp;

namespace FrameControlEx.Core.MainView.Scene.Outputs {
    public class SameInstanceOutputViewModel : VisualOutputViewModel {
        public SKImage lastFrame;

        public SameInstanceOutputViewModel(OutputDeckViewModel deck) : base(deck) {
            
        }

        public override void OnAcceptFrame(SKSurface surface) {
            base.OnAcceptFrame(surface);
            this.lastFrame = surface.Snapshot();
        }
    }
}