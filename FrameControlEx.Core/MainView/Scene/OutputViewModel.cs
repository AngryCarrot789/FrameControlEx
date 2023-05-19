using System.Threading.Tasks;

namespace FrameControlEx.Core.MainView.Scene {
    /// <summary>
    /// A view model that stores information about a video or audio output
    /// </summary>
    public class OutputViewModel : BaseIOViewModel {
        public OutputDeckViewModel Deck { get; }

        public AsyncRelayCommand RemoveCommand { get; }

        public FrameControlViewModel FrameControl => this.Deck.Scene.Deck.FrameControl;

        public OutputViewModel(OutputDeckViewModel deck) {
            this.Deck = deck;
            this.RemoveCommand = new AsyncRelayCommand(this.RemoveAction);
        }

        public async Task RemoveAction() {
            await this.Deck.RemoveItemAction(this);
        }
    }
}