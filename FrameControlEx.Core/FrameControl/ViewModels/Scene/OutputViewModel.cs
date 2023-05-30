using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Models.Scene.Outputs.Base;

namespace FrameControlEx.Core.FrameControl.ViewModels.Scene {
    /// <summary>
    /// A view model that stores information about an output
    /// </summary>
    public abstract class OutputViewModel : BaseIOViewModel {
        public new OutputModel Model => (OutputModel) base.Model;

        private OutputDeckViewModel deck;
        public OutputDeckViewModel Deck {
            get => this.deck;
            set {
                OutputDeckViewModel old = this.deck;
                this.RaisePropertyChanged(ref this.deck, value);
                this.OnDeckChanged(old, value);
            }
        }

        public AsyncRelayCommand RemoveCommand { get; }

        public FrameControlViewModel FrameControl => this.Deck?.FrameControl;

        protected OutputViewModel(OutputModel model) : base(model) {
            this.RemoveCommand = new AsyncRelayCommand(this.RemoveAction, () => this.Deck != null);
        }

        public async Task RemoveAction() {
            if (await this.CheckHasDeck()) {
                await this.Deck.RemoveItemsAction(this);
            }
        }

        public async Task<bool> CheckHasDeck() {
            if (this.Deck == null) {
                await IoC.MessageDialogs.ShowMessageAsync("No deck", "This source is not in a deck yet; a target output cannot be selected");
                return false;
            }

            return true;
        }

        protected virtual void OnDeckChanged(OutputDeckViewModel oldDeck, OutputDeckViewModel newDeck) {
            this.Model.Deck = newDeck?.Model;
        }
    }
}