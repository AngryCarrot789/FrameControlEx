using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Models.Scene.Sources;

namespace FrameControlEx.Core.FrameControl.Scene.Sources {
    /// <summary>
    /// A source that renders a target scene
    /// </summary>
    public class SceneSourceViewModel : AVSourceViewModel {
        public new SceneSourceModel Model => (SceneSourceModel) ((BaseIOViewModel) this).Model;

        private SceneViewModel targetScene;
        public SceneViewModel TargetScene {
            get => this.targetScene;
            private set {
                this.Model.TargetScene = value?.Model;
                this.RaisePropertyChanged(ref this.targetScene, value);
            }
        }

        public AsyncRelayCommand SelectTargetCommand { get; }

        public SceneSourceViewModel(SceneSourceModel model) : base(model) {
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
                this.Model.InvalidateVisual();
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