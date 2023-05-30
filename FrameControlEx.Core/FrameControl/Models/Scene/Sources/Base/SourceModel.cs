namespace FrameControlEx.Core.FrameControl.Models.Scene.Sources.Base {
    /// <summary>
    /// A source; can provide video, audio, or both
    /// </summary>
    public abstract class SourceModel : BaseIOModel {
        private SourceDeckModel deck;
        public SourceDeckModel Deck {
            get => this.deck;
            set {
                SourceDeckModel oldDeck = this.deck;
                this.deck = value;
                this.OnDeckChanged(oldDeck, value);
            }
        }

        protected SourceModel() {

        }

        protected virtual void OnDeckChanged(SourceDeckModel oldDeck, SourceDeckModel newDeck) {

        }
    }
}