using System.Numerics;
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

        public override Vector2 GetRawSize() {
            SKImage frame = this.targetOutput?.lastFrame;
            if (frame == null) {
                return Vector2.Zero;
            }

            return new Vector2(frame.Width, frame.Height);
        }

        public override void OnRender(RenderContext context) {
            base.OnRender(context);
            if (this.TargetOutput != null && this.TargetOutput.IsEnabled & this.TargetOutput.lastFrame != null) {
                Vector2 scale = this.Scale, pos = this.Pos, origin = this.ScaleOrigin;
                context.Canvas.Translate(pos.X, pos.Y);
                SKImage frame = this.TargetOutput.lastFrame;
                context.Canvas.Scale(scale.X, scale.Y, frame.Width * origin.X, frame.Height * origin.Y);
                context.Canvas.DrawImage(frame, 0, 0);
            }
        }

        protected override BaseIOViewModel CreateInstanceCore() {
            return new LoopbackSourceViewModel();
        }

        protected override void LoadThisIntoUserCopy(BaseIOViewModel vm) {
            base.LoadThisIntoUserCopy(vm);
            if (vm is LoopbackSourceViewModel l) {
                l.targetOutput = this.targetOutput;
            }
        }
    }
}