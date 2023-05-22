using System.Threading.Tasks;
using FrameControlEx.Core.MainView.Scene.Outputs;

namespace FrameControlEx.Core.MainView.Scene.Sources {
    public class LoopbackSourceViewModel : VisualSourceViewModel {
        private BasicBufferOutputViewModel targetOutput;
        public BasicBufferOutputViewModel TargetOutput {
            get => this.targetOutput;
            set => this.RaisePropertyChanged(ref this.targetOutput, value);
        }

        public AsyncRelayCommand ChangeTargetCommand { get; }

        public LoopbackSourceViewModel() {
            this.ChangeTargetCommand = new AsyncRelayCommand(this.ChangeTargetAction, () => this.Deck != null);
        }

        public async Task ChangeTargetAction() {
            if (!await this.CheckHasDeck())
                return;

            BasicBufferOutputViewModel result = (BasicBufferOutputViewModel) await IoC.BufferSelector.SelectOutput(this.Deck.Scene.OutputDeck, (x) => x is BasicBufferOutputViewModel);
            if (result != null) {
                this.TargetOutput = result;
                this.InvalidateVisual();
            }
        }

        protected override BaseIOViewModel CreateInstanceCore() {
            return new LoopbackSourceViewModel();
        }

        protected override void LoadThisIntoCopy(BaseIOViewModel vm) {
            base.LoadThisIntoCopy(vm);
            if (vm is LoopbackSourceViewModel si) {
                si.targetOutput = this.targetOutput;
            }
        }
    }
}