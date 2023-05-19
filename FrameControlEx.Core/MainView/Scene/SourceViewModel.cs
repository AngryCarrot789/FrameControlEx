namespace FrameControlEx.Core.MainView.Scene.InOuts {
    /// <summary>
    /// A view model that stores information about a video or audio source/input
    /// </summary>
    public class SourceViewModel : BaseViewModel {
        private string readableName;
        public string ReadableName {
            get => this.readableName;
            set => this.RaisePropertyChanged(ref this.readableName, value);
        }

        public SourceDeckViewModel Deck { get; }

        public SourceViewModel(SourceDeckViewModel deck) {
            this.Deck = deck;
        }
    }
}