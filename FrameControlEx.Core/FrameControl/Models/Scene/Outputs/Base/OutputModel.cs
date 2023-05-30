namespace FrameControlEx.Core.FrameControl.Models.Scene.Outputs.Base {
    public abstract class OutputModel : BaseIOModel {
        private OutputDeckModel deck;
        public OutputDeckModel Deck {
            get => this.deck;
            set {
                OutputDeckModel oldDeck = this.deck;
                this.deck = value;
                this.OnDeckChanged(oldDeck, value);
            }
        }

        protected OutputModel() {

        }

        protected virtual void OnDeckChanged(OutputDeckModel oldDeck, OutputDeckModel newDeck) {
            // things here should really be handled in the view models...
        }
    }
}