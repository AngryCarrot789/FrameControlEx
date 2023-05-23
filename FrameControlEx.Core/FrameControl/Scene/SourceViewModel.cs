using System.Collections.Generic;
using System.Threading.Tasks;
using FrameControlEx.Core.Actions.Contexts;
using FrameControlEx.Core.AdvancedContextService;

namespace FrameControlEx.Core.MainView.Scene {
    /// <summary>
    /// A view model that stores information about a video or audio source/input
    /// </summary>
    public abstract class SourceViewModel : BaseIOViewModel {
        private SourceDeckViewModel deck;
        public SourceDeckViewModel Deck {
            get => this.deck;
            set => this.RaisePropertyChanged(ref this.deck, value);
        }

        public AsyncRelayCommand RemoveCommand { get; }

        public FrameControlViewModel FrameControl => this.Deck?.Scene.Deck.FrameControl;

        protected SourceViewModel() {
            this.RemoveCommand = new AsyncRelayCommand(this.RemoveAction, () => this.Deck != null);
        }

        protected override void OnIsEnabledChanged() {
            base.OnIsEnabledChanged();
            this.Deck?.OnIsEnabledChanged(this);
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

    public class SourceContextGenerator : IContextGenerator {
        public static SourceContextGenerator Instance { get; } = new SourceContextGenerator();

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (context.TryGetContext(out SourceViewModel source)) {
                list.Add(new CommandContextEntry("Rename", source.RenameCommand));
                list.Add(SeparatorEntry.Instance);
                if (source.IsEnabled) {
                    list.Add(new CommandContextEntry("Disable", source.DisableCommand));
                }
                else {
                    list.Add(new CommandContextEntry("Enable", source.EnableCommand));
                }

                if (source.Deck != null) {
                    list.Add(SeparatorEntry.Instance);
                    list.Add(new CommandContextEntry("Remove", source.RemoveCommand));
                }

                if (context.TryGetContext(out SourceDeckViewModel deck)) {
                    list.Add(SeparatorEntry.Instance);
                    this.AddNewItemsContext(list, deck);
                }
                else if (source.Deck != null) {
                    list.Add(SeparatorEntry.Instance);
                    this.AddNewItemsContext(list, source.Deck);
                }
            }
            else if (context.TryGetContext(out SourceDeckViewModel deck)) {
                this.AddNewItemsContext(list, deck);
            }
        }

        public void AddNewItemsContext(List<IContextEntry> list, SourceDeckViewModel deck) {
            list.Add(new CommandContextEntry("Add Image", deck.AddImageCommand));
            list.Add(new CommandContextEntry("Add output-loopback", deck.AddLoopbackInput));
        }
    }
}