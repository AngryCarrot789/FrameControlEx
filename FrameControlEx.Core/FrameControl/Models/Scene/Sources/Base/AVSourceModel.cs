using System.Numerics;
using FrameControlEx.Core.RBC;

namespace FrameControlEx.Core.FrameControl.Models.Scene.Sources.Base {
    /// <summary>
    /// An audio-visual source; capable of providing audio and video (duh). This contains
    /// a position, scale, scale origin, etc, to represent the standard controls for an input/source
    /// </summary>
    public abstract class AVSourceModel : SourceModel, IAudioSource, IVisualSource {
        public Vector2 Pos { get; set; }
        public Vector2 Scale { get; set; }
        public Vector2 ScaleOrigin { get; set; }

        public event AVInvalidatedEventHandler OnVisualInvalidated;

        protected AVSourceModel() {
            this.Pos = new Vector2(0, 0);
            this.Scale = new Vector2(1, 1);
            this.ScaleOrigin = new Vector2(0.5f, 0.5f);
        }

        /// <summary>
        /// Invokes the visual invalidation events for this instance and its associated deck (if present), causing the visual
        /// source to be rendered (may also result in all sources being rendered, or may do nothing if the UI rendering is tick based)
        /// </summary>
        public void InvalidateVisual() {
            this.OnVisualInvalidated?.Invoke(this);
            this.Deck?.InvalidateVisual();
        }

        public abstract Vector2 GetSize();

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetStruct(nameof(this.Pos), this.Pos);
            data.SetStruct(nameof(this.Scale), this.Scale);
            data.SetStruct(nameof(this.ScaleOrigin), this.ScaleOrigin);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.Pos = data.GetStruct<Vector2>(nameof(this.Pos));
            this.Scale = data.GetStruct<Vector2>(nameof(this.Scale));
            this.ScaleOrigin = data.GetStruct<Vector2>(nameof(this.ScaleOrigin));
        }

        // was originally going to make rendering event based, but that would just add extra overhead
        // because there may be multiple sources whose frames aren't necessarily event base-able (e.g.
        // capturing desktop; you do that event 60th of a sec or whatever the refresh rate is)

        // so instead, there's a timer that ticks at the project frame rate. It does use
        // a bit of extra CPU and GPU... but that's probably alright

        public virtual void OnTickVisual() {

        }

        public virtual void OnRender(RenderContext context) {

        }
    }
}