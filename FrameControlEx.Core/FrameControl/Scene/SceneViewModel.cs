using System.Collections.Generic;
using System.Threading.Tasks;
using FrameControlEx.Core.Actions.Contexts;
using FrameControlEx.Core.AdvancedContextService;
using FrameControlEx.Core.Views.Dialogs.UserInputs;
using SkiaSharp;

namespace FrameControlEx.Core.MainView.Scene {
    public class SceneViewModel : BaseViewModel {
        private string readableName;
        public string ReadableName {
            get => this.readableName;
            set => this.RaisePropertyChanged(ref this.readableName, value);
        }

        private bool clearScreenOnRender = true;
        public bool ClearScreenOnRender {
            get => this.clearScreenOnRender;
            set => this.RaisePropertyChanged(ref this.clearScreenOnRender, value);
        }

        private SKColor backgroundColour = SKColors.Black;
        public SKColor BackgroundColour {
            get => this.backgroundColour;
            set => this.RaisePropertyChanged(ref this.backgroundColour, value);
        }

        public SourceDeckViewModel SourceDeck { get; }

        public SceneDeckViewModel Deck { get; }

        public AsyncRelayCommand RenameCommand { get; }
        public AsyncRelayCommand RemoveCommand { get; }

        public SceneViewModel(SceneDeckViewModel deck) {
            this.Deck = deck;
            this.SourceDeck = new SourceDeckViewModel(this);
            this.RenameCommand = new AsyncRelayCommand(this.RenameAction);
            this.RemoveCommand = new AsyncRelayCommand(this.RemoveAction);
        }

        public async Task RenameAction() {
            string name = await IoC.UserInput.ShowSingleInputDialogAsync("Rename scene", "Input a new name for this scene:", this.ReadableName, Validators.ForNonEmptyString("Name cannot be empty"));
            if (!string.IsNullOrEmpty(name)) {
                this.ReadableName = name;
            }
        }

        public async Task RemoveAction() {
            await this.Deck.RemoveItemAction(this);
        }
    }

    public class SceneViewModelContextMenuGenerator : IContextGenerator {
        public static SceneViewModelContextMenuGenerator Instance { get; } = new SceneViewModelContextMenuGenerator();

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (context.TryGetContext(out SceneViewModel scene)) {
                list.Add(new CommandContextEntry("Rename Scene", scene.RenameCommand));
                if (scene.Deck != null) {
                    list.Add(SeparatorEntry.Instance);
                    list.Add(new CommandContextEntry("Add New Scene", scene.Deck.AddCommand));
                    list.Add(new CommandContextEntry("Remove Scene", scene.RemoveCommand));
                }
            }
            else if (context.TryGetContext(out SceneDeckViewModel deck)) {
                list.Add(new CommandContextEntry("Add Scene", deck.AddCommand));
            }
        }
    }
}