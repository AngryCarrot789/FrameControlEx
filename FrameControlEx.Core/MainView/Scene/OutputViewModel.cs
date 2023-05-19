namespace FrameControlEx.Core.MainView.Scene.InOuts {
    /// <summary>
    /// A view model that stores information about a video or audio output
    /// </summary>
    public class OutputViewModel : BaseViewModel {
        private string readableName;
        public string ReadableName {
            get => this.readableName;
            set => this.RaisePropertyChanged(ref this.readableName, value);
        }

        public OutputDeckViewModel Deck { get; }

        public OutputViewModel(OutputDeckViewModel deck) {
            this.Deck = deck;
        }
    }
}