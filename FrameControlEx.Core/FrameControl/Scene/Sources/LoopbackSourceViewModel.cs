using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Scene.Outputs;
using SkiaSharp;

namespace FrameControlEx.Core.FrameControl.Scene.Sources {
    public class LoopbackSourceViewModel : AVSourceViewModel {
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

            BasicBufferOutputViewModel result = (BasicBufferOutputViewModel) await IoC.BufferSelector.SelectOutput(this.FrameControl.OutputDeck, (x) => x is BasicBufferOutputViewModel);
            if (result != null) {
                this.TargetOutput = result;
                this.InvalidateVisual();
            }
        }

        public override void OnRender(SKSurface surface, SKCanvas canvas, SKImageInfo frameInfo) {
            base.OnRender(surface, canvas, frameInfo);
            if (this.TargetOutput != null && this.TargetOutput.IsEnabled & this.TargetOutput.lastFrame != null) {
                SKMatrix matrix = canvas.TotalMatrix;
                canvas.Translate(this.PosX, this.PosY);
                SKImage frame = this.TargetOutput.lastFrame;
                canvas.Scale(this.ScaleX, this.ScaleY, frame.Width * this.ScaleOriginX, frame.Height * this.ScaleOriginY);
                canvas.DrawImage(frame, 0, 0);
                canvas.SetMatrix(matrix);
            }
        }

        protected override BaseIOViewModel CreateInstanceCore() {
            return new LoopbackSourceViewModel();
        }

        protected override void LoadThisIntoCopy(BaseIOViewModel vm) {
            base.LoadThisIntoCopy(vm);
            if (vm is LoopbackSourceViewModel l) {
                l.targetOutput = this.targetOutput;
            }
        }
    }
}