using System;
using SkiaSharp;

namespace FrameControlEx.Core.MainView.Scene.Outputs {
    /// <summary>
    /// Base class for visual outputs (videos, images, but not audio). These receive the fully rendered output
    /// frame and can do whatever they want with it (they really shouldn't modify the frame though)
    /// </summary>
    public class VisualOutputViewModel : OutputViewModel {
        public VisualOutputViewModel(OutputDeckViewModel deck) : base(deck) {

        }

        public virtual void OnAcceptFrame(SKSurface surface) {

        }
    }
}