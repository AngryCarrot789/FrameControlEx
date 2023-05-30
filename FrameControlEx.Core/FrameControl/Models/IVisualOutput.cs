namespace FrameControlEx.Core.FrameControl.Scene {
    public interface IVisualOutput {
        /// <summary>
        /// Accepts the given render context, containing the information about a rendered frame, including the frame size/info
        /// </summary>
        /// <param name="context">The rendering context</param>
        void OnAcceptFrame(RenderContext context);
    }
}