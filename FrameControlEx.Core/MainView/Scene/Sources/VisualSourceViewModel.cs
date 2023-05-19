using System.Numerics;

namespace FrameControlEx.Core.MainView.Scene.Sources {
    /// <summary>
    /// Base class for visual sources (videos, images, but not audio)
    /// </summary>
    public abstract class VisualSourceViewModel : SourceViewModel {
        private Vector2 pos;
        private Vector2 scale;

        /// <summary>
        /// The pixel-based offset of this visual source, from the top-left corner of the view port. 0,0 is the very top left
        /// </summary>
        public Vector2 Pos {
            get => this.pos;
            set {
                this.RaisePropertyChanged(ref this.pos, value);
                this.OnVisualInvalidated();
            }
        }

        /// <summary>
        /// This visual source's scale translation. By default, this is 1,1 (meaning no scaling is applied)
        /// </summary>
        public Vector2 Scale {
            get => this.scale;
            set {
                this.RaisePropertyChanged(ref this.scale, value);
                this.OnVisualInvalidated();
            }
        }

        protected VisualSourceViewModel(SourceDeckViewModel deck) : base(deck) {
            this.pos = new Vector2(0, 0);
            this.scale = new Vector2(1, 1);
        }

        public void OnVisualInvalidated() {
            this.Deck.OnVisualSourceInvalidated(this);
        }

        /// <summary>
        /// Called just before rendering in order to tick things (e.g. the next gif frame to display)
        /// </summary>
        public virtual void OnTickVisual() {

        }
    }
}