using FrameControlEx.Core.RBC;
using SkiaSharp;

namespace FrameControlEx.Core.FrameControl.Models.Scene {
    public class SceneModel : IRBESerialisable {
        /// <summary>
        /// The deck that owns this scene
        /// </summary>
        public SceneDeckModel SceneDeck { get; }

        public SourceDeckModel SourceDeck { get; }

        public string DisplayName { get; set; }

        /// <summary>
        /// Whether to clear the background on each render of this scene
        /// </summary>
        public bool ClearScreenOnRender { get; set; } = true;

        public SKColor BackgroundColour { get; set; } = SKColors.Black;

        /// <summary>
        /// Whether this scene is allow to render or not
        /// </summary>
        public bool IsActive { get; set; }

        public bool IsRenderOrderReversed { get; set; }

        public SceneModel(SceneDeckModel sceneDeck) {
            this.SceneDeck = sceneDeck;
            this.SourceDeck = new SourceDeckModel(this);
        }

        public void WriteToRBE(RBEDictionary data) {
            data.SetString(nameof(this.DisplayName), this.DisplayName);
            data.SetBool(nameof(this.ClearScreenOnRender), this.ClearScreenOnRender);
            data.SetStruct(nameof(this.BackgroundColour), this.BackgroundColour);
            data.SetBool(nameof(this.IsRenderOrderReversed), this.IsRenderOrderReversed);
            this.SourceDeck.WriteToRBE(data.GetOrCreateDictionaryElement(nameof(this.SourceDeck)));
        }

        public void ReadFromRBE(RBEDictionary data) {
            this.DisplayName = data.GetString(nameof(this.DisplayName));
            this.ClearScreenOnRender = data.GetBool(nameof(this.ClearScreenOnRender));
            this.BackgroundColour = data.GetStruct<SKColor>(nameof(this.BackgroundColour));
            this.IsRenderOrderReversed = data.GetBool(nameof(this.IsRenderOrderReversed));
            this.SourceDeck.ReadFromRBE(data.GetElement<RBEDictionary>(nameof(this.SourceDeck)));
        }
    }
}