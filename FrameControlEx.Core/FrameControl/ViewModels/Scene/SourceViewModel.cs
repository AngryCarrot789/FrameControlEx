using System.Collections.Generic;
using System.Threading.Tasks;
using FrameControlEx.Core.Actions.Contexts;
using FrameControlEx.Core.AdvancedContextService;
using FrameControlEx.Core.FrameControl.Models.Scene.Sources.Base;
using FrameControlEx.Core.FrameControl.ViewModels.Scene.Sources;

namespace FrameControlEx.Core.FrameControl.ViewModels.Scene {
    /// <summary>
    /// A view model that stores information about a video or audio source/input
    /// </summary>
    public abstract class SourceViewModel : BaseIOViewModel {
        public new SourceModel Model => (SourceModel) base.Model;

        public FrameControlViewModel FrameControl => this.Deck?.Scene.Deck.FrameControl;

        private SourceDeckViewModel deck;
        public SourceDeckViewModel Deck {
            get => this.deck;
            set {
                SourceDeckViewModel old = this.deck;
                this.Model.Deck = value?.Model;
                this.RaisePropertyChanged(ref this.deck, value);
                this.OnDeckChanged(old, value);
            }
        }

        public AsyncRelayCommand RemoveCommand { get; }

        protected SourceViewModel(SourceModel model) : base(model) {
            this.RemoveCommand = new AsyncRelayCommand(this.RemoveAction, () => this.Deck != null);
        }

        protected override void OnIsEnabledChanged() {
            base.OnIsEnabledChanged();
            this.Deck?.OnIsEnabledChanged(this);
        }

        public async Task RemoveAction() {
            if (await this.CheckHasDeck()) {
                await this.Deck.RemoveItemsAction(this);
            }
        }

        public async Task<bool> CheckHasDeck() {
            if (this.Deck == null) {
                await IoC.MessageDialogs.ShowMessageAsync("No deck", "This source is not in a deck yet; a target output cannot be selected");
                return false;
            }

            return true;
        }

        protected virtual void OnDeckChanged(SourceDeckViewModel oldDeck, SourceDeckViewModel newDeck) {
            this.Model.Deck = newDeck?.Model;
        }
    }

    public class SourceContextGenerator : IContextGenerator {
        public static SourceContextGenerator Instance { get; } = new SourceContextGenerator();

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (context.TryGetContext(out SourceViewModel source)) {
                if (context.TryGetContext(out SourceDeckViewModel deck)) {
                    this.AddNewItemsContext(list, deck);
                    list.Add(SeparatorEntry.Instance);
                }
                else if (source.Deck != null) {
                    this.AddNewItemsContext(list, source.Deck);
                    list.Add(SeparatorEntry.Instance);
                }

                list.Add(new CommandContextEntry("Rename", source.RenameCommand));
                if (source is ImageSourceViewModel img) {
                    list.Add(new CommandContextEntry("Open Image...", img.SelectFileCommand));
                }

                list.Add(SeparatorEntry.Instance);
                if (source.IsEnabled) {
                    list.Add(new CommandContextEntry("Disable", source.DisableCommand));
                }
                else {
                    list.Add(new CommandContextEntry("Enable", source.EnableCommand));
                }

                if (source.Deck != null) {
                    list.Add(new CommandContextEntry("Remove", source.RemoveCommand));
                }
            }
            else if (context.TryGetContext(out SourceDeckViewModel deck)) {
                this.AddNewItemsContext(list, deck);
            }
        }

        public void AddNewItemsContext(List<IContextEntry> list, SourceDeckViewModel deck) {
            List<IContextEntry> children = new List<IContextEntry>();
            GroupContextEntry group = new GroupContextEntry("Add", "Shows all of the possible items to add", children);
            list.Add(group);
            list = children;

            list.Add(new CommandContextEntry("Image", deck.AddImageCommand));
            list.Add(new CommandContextEntry("MemMapFile Source", deck.AddMemMapFileCommand));
            list.Add(new CommandContextEntry("Scene Render", deck.AddSceneSourceCommand));
            list.Add(SeparatorEntry.Instance);
            list.Add(new CommandContextEntry("Output Loopback", deck.AddLoopbackInputCommand) {
                Description = "Takes the rendered output and uses it as an input for the next frame"
            });
        }
    }
}