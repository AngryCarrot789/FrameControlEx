namespace FrameControlEx.Core.FrameControl.Scene {
    public interface IVisualOutput {
        /// <summary>
        /// Accepts the given skia surface frame, including the frame size/info
        /// </summary>
        /// <param name="context"></param>
        void OnAcceptFrame(RenderContext context);
    }
}