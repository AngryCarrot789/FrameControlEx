using System.Collections.Generic;
using System.Threading.Tasks;
using FrameControlEx.Core.Actions.Contexts;
using FrameControlEx.Core.AdvancedContextService;

namespace FrameControlEx.Core.MainView.Scene {
    /// <summary>
    /// A view model that stores information about a video or audio output
    /// </summary>
    public abstract class OutputViewModel : BaseIOViewModel {
        private OutputDeckViewModel deck;

        public OutputDeckViewModel Deck {
            get => this.deck;
            set => this.RaisePropertyChanged(ref this.deck, value);
        }

        public AsyncRelayCommand RemoveCommand { get; }

        public FrameControlViewModel FrameControl => this.Deck?.FrameControl;

        protected OutputViewModel() {
            this.RemoveCommand = new AsyncRelayCommand(this.RemoveAction, () => this.Deck != null);
        }

        public async Task RemoveAction() {
            if (await this.CheckHasDeck()) {
                await this.Deck.RemoveItemAction(this);
            }
        }

        public async Task<bool> CheckHasDeck() {
            if (this.Deck == null) {
                await IoC.MessageDialogs.ShowMessageAsync("No deck", "This source is not in a deck yet; a target output cannot be selected");
                return false;
            }

            return true;
        }
    }

    public class OutputContextGenerator : IContextGenerator {
        public static OutputContextGenerator Instance { get; } = new OutputContextGenerator();

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (context.TryGetContext(out OutputViewModel output)) {
                list.Add(new CommandContextEntry("Rename", output.RenameCommand));
                list.Add(SeparatorEntry.Instance);
                if (output.IsEnabled) {
                    list.Add(new CommandContextEntry("Disable", output.DisableCommand));
                }
                else {
                    list.Add(new CommandContextEntry("Enable", output.EnableCommand));
                }

                if (output.Deck != null) {
                    list.Add(SeparatorEntry.Instance);
                    list.Add(new CommandContextEntry("Remove", output.RemoveCommand));
                }

                if (context.TryGetContext(out OutputDeckViewModel deck)) {
                    list.Add(SeparatorEntry.Instance);
                    this.AddNewItemsContext(list, deck);
                }
                else if (output.Deck != null) {
                    list.Add(SeparatorEntry.Instance);
                    this.AddNewItemsContext(list, output.Deck);
                }
            }
            else if (context.TryGetContext(out OutputDeckViewModel deck)) {
                this.AddNewItemsContext(list, deck);
            }
        }

        public void AddNewItemsContext(List<IContextEntry> list, OutputDeckViewModel deck) {
            list.Add(new CommandContextEntry("Add buffered output (used for loopback)", deck.AddBufferedOutputCommand));
        }
    }
}