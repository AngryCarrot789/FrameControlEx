using System.Collections.Generic;
using System.Threading.Tasks;
using FrameControlEx.Core.Actions.Contexts;
using FrameControlEx.Core.AdvancedContextService;
using FrameControlEx.Core.Views.Dialogs.UserInputs;

namespace FrameControlEx.Core.MainView.Scene {
    public class SceneViewModel : BaseViewModel {
        private string readableName;
        public string ReadableName {
            get => this.readableName;
            set => this.RaisePropertyChanged(ref this.readableName, value);
        }

        public SourceDeckViewModel SourceDeck { get; }

        public OutputDeckViewModel OutputDeck { get; }

        public SceneDeckViewModel Deck { get; }

        public AsyncRelayCommand RenameCommand { get; }
        public AsyncRelayCommand RemoveCommand { get; }

        public SceneViewModel(SceneDeckViewModel deck) {
            this.Deck = deck;
            this.SourceDeck = new SourceDeckViewModel(this);
            this.OutputDeck = new OutputDeckViewModel(this);

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
            if (context.TryGetContext(out SceneViewModel vm)) {
                list.Add(new CommandContextEntry("Rename", vm.RenameCommand));
                list.Add(SeparatorEntry.Instance);
                list.Add(new CommandContextEntry("Remove", vm.RemoveCommand));
            }
        }
    }
}