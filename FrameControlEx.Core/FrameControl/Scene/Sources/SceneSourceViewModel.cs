using System.Numerics;
using System.Threading.Tasks;

namespace FrameControlEx.Core.FrameControl.Scene.Sources {
    /// <summary>
    /// A source that renders a target scene
    /// </summary>
    public class SceneSourceViewModel : AVSourceViewModel {
        private SceneViewModel targetScene;
        public SceneViewModel TargetScene {
            get => this.targetScene;
            private set => this.RaisePropertyChanged(ref this.targetScene, value);
        }

        public AsyncRelayCommand SelectTargetCommand { get; }

        public SceneSourceViewModel() {
            this.SelectTargetCommand = new AsyncRelayCommand(this.SelectTargetAction);
        }

        public async Task SelectTargetAction() {
            if (!await this.CheckHasDeck())
                return;

            // cannot render our owning scene because that would result in a stack overflow as it tries to infinitely render
            // could maybe just skip the 2nd render?
            SceneViewModel result = await IoC.BufferSelector.SelectScene(this.FrameControl.SceneDeck, (x) => !ReferenceEquals(x, this.Deck.Scene));
            if (result != null && !ReferenceEquals(result, this.Deck.Scene)) {
                this.TargetScene = result;
                this.InvalidateVisual();
            }
        }

        public override void OnRender(RenderContext context) {
            base.OnRender(context);
            if (this.TargetScene != null) {
                // doens't work
                Vector2 scale = this.Scale, pos = this.Pos, origin = this.ScaleOrigin;
                context.Canvas.Save();
                context.Canvas.ClipRect(SkiaSharp.SKRect.Create(pos.X, pos.Y, context.FrameInfo.Width, context.FrameInfo.Height));
                context.Canvas.Scale(scale.X, scale.Y, context.FrameInfo.Width * origin.X, context.FrameInfo.Height * origin.Y);
                context.Canvas.Translate(pos.X, pos.Y);
                context.RenderScene(this.TargetScene);
                context.Canvas.Restore();
            }
        }

        protected override BaseIOViewModel CreateInstanceCore() {
            return new SceneSourceViewModel();
        }

        protected override void LoadThisIntoUserCopy(BaseIOViewModel vm) {
            base.LoadThisIntoUserCopy(vm);
            if (vm is SceneSourceViewModel src) {
                src.targetScene = this.targetScene;
            }
        }

        protected override void OnDeckChanged(SourceDeckViewModel oldDeck, SourceDeckViewModel newDeck) {
            base.OnDeckChanged(oldDeck, newDeck);
            if (this.TargetScene != null && newDeck != null && ReferenceEquals(this.TargetScene, newDeck.Scene)) {
                this.TargetScene = null;
            }
        }
    }
}