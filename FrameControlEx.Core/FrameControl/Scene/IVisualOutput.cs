using SkiaSharp;

namespace FrameControlEx.Core.FrameControl.Scene {
    public interface IVisualOutput {
        /// <summary>
        /// Accepts the given skia surface frame, including the frame size/info
        /// </summary>
        /// <param name="surface">The surface containing a fully rendered surface</param>
        /// <param name="frameInfo">(Raw) information about the frame, such as width and height</param>
        void OnAcceptFrame(SKSurface surface, in SKImageInfo frameInfo);
    }
}