using System.Threading.Tasks;

namespace FrameControlEx.Core.FrameControl.Scene {
    /// <summary>
    /// A view model that stores information about a video or audio output
    /// </summary>
    public abstract class OutputViewModel : BaseIOViewModel {
        private OutputDeckViewModel deck;

        public OutputDeckViewModel Deck {
            get => this.deck;
            set => this.RaisePropertyChanged(ref this.deck, value);
        }

        public AsyncRelayCommand RemoveCommand { get; }

        public FrameControlViewModel FrameControl => this.Deck?.FrameControl;

        protected OutputViewModel() {
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
    }
}