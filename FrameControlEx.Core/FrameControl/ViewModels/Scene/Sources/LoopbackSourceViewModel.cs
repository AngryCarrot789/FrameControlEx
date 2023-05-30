using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Models.Scene.Sources;
using FrameControlEx.Core.FrameControl.ViewModels.Scene.Outputs;

namespace FrameControlEx.Core.FrameControl.ViewModels.Scene.Sources {
    public class LoopbackSourceViewModel : AVSourceViewModel {
        public new LoopbackSourceModel Model => (LoopbackSourceModel) ((BaseIOViewModel) this).Model;

        private BufferedOutputViewModel targetOutput;
        public BufferedOutputViewModel TargetOutput {
            get => this.targetOutput;
            set {
                this.targetOutput?.Model.Unuse();
                this.Model.TargetOutput = value?.Model;
                value?.Model.Use();
                this.RaisePropertyChanged(ref this.targetOutput, value);
            }
        }

        public AsyncRelayCommand ChangeTargetCommand { get; }

        public LoopbackSourceViewModel(LoopbackSourceModel model) : base(model) {
            this.ChangeTargetCommand = new AsyncRelayCommand(this.ChangeTargetAction, () => this.Deck != null);
        }

        public async Task ChangeTargetAction() {
            if (!await this.CheckHasDeck())
                return;

            BufferedOutputViewModel result = (BufferedOutputViewModel) await IoC.BufferSelector.SelectOutput(this.FrameControl.OutputDeck, (x) => x is BufferedOutputViewModel);
            if (result != null) {
                this.TargetOutput = result;
                this.Model.InvalidateVisual();
            }
        }
    }
}