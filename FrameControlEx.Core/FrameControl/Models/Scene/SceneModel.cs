using SkiaSharp;

namespace FrameControlEx.Core.FrameControl.ViewModels.Scene {
    public class SceneModel {
        /// <summary>
        /// The deck that owns this scene
        /// </summary>
        public SceneDeckModel Deck { get; }

        public string ReadableName { get; set; }

        /// <summary>
        /// Whether to clear the background on each render of this scene
        /// </summary>
        public bool ClearScreenOnRender { get; set; }

        public SKColor BackgroundColour { get; set; }

        /// <summary>
        /// Whether this scene is allow to render or not
        /// </summary>
        public bool IsActive { get; set; }

        public bool IsRenderOrderReversed { get; set; }

        public SceneModel(SceneDeckModel deck) {
            this.Deck = deck;
        }
    }
}