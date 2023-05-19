using System.Threading.Tasks;
using FrameControlEx.Core.MainView.Scene.Outputs;

namespace FrameControlEx.Core.MainView.Scene.Sources {
    public class SameInstanceInputViewModel : VisualSourceViewModel {
        private BasicBufferOutputViewModel targetOutput;
        public BasicBufferOutputViewModel TargetOutput {
            get => this.targetOutput;
            set => this.RaisePropertyChanged(ref this.targetOutput, value);
        }

        public AsyncRelayCommand ChangeTargetCommand { get; }

        public SameInstanceInputViewModel(SourceDeckViewModel deck) : base(deck) {
            this.ChangeTargetCommand = new AsyncRelayCommand(this.ChangeTargetAction);
        }

        public async Task ChangeTargetAction() {
            BasicBufferOutputViewModel result = (BasicBufferOutputViewModel) await IoC.BufferSelector.SelectOutput(this.Deck.Scene.OutputDeck, (x) => x is BasicBufferOutputViewModel);
            if (result != null) {
                this.TargetOutput = result;
                this.OnVisualInvalidated();
            }
        }
    }
}