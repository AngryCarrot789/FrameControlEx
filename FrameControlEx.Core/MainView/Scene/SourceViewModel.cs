using System.Threading.Tasks;
using FrameControlEx.Core.Views.Dialogs.UserInputs;

namespace FrameControlEx.Core.MainView.Scene {
    /// <summary>
    /// A view model that stores information about a video or audio source/input
    /// </summary>
    public class SourceViewModel : BaseIOViewModel {
        public SourceDeckViewModel Deck { get; }

        public AsyncRelayCommand RemoveCommand { get; }

        public FrameControlViewModel FrameControl => this.Deck.Scene.Deck.FrameControl;

        public SourceViewModel(SourceDeckViewModel deck) {
            this.Deck = deck;
            this.RemoveCommand = new AsyncRelayCommand(this.RemoveAction);
        }

        public async Task RemoveAction() {
            await this.Deck.RemoveItemAction(this);
        }
    }
}