using System;
using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Models.Scene;
using FrameControlEx.Core.Utils;
using SkiaSharp;

namespace FrameControlEx.Core.FrameControl.ViewModels.Scene {
    public class SceneViewModel : BaseViewModel, IDisposable {
        public string DisplayName {
            get => this.Model.DisplayName;
            set {
                this.Model.DisplayName = value;
                this.RaisePropertyChanged();
            }
        }

        public bool ClearScreenOnRender {
            get => this.Model.ClearScreenOnRender;
            set {
                this.Model.ClearScreenOnRender = value;
                this.RaisePropertyChanged();
            }
        }

        public SKColor BackgroundColour {
            get => this.Model.BackgroundColour;
            set {
                this.Model.BackgroundColour = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsActive {
            get => this.Model.IsActive;
            set {
                this.Model.IsActive = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsRenderOrderReversed {
            get => this.Model.IsRenderOrderReversed;
            set {
                this.Model.IsRenderOrderReversed = value;
                this.RaisePropertyChanged();
            }
        }

        public SceneDeckViewModel Deck { get; }

        public SourceDeckViewModel SourceDeck { get; }

        public AsyncRelayCommand RenameCommand { get; }
        public AsyncRelayCommand RemoveCommand { get; }

        public SceneModel Model { get; }

        public SceneViewModel(SceneDeckViewModel deck, SceneModel model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model), "Model cannot be null");
            this.Deck = deck;
            this.SourceDeck = new SourceDeckViewModel(this, model.SourceDeck);
            this.RenameCommand = new AsyncRelayCommand(this.RenameAction);
            this.RemoveCommand = new AsyncRelayCommand(this.RemoveAction);
        }

        public async Task RenameAction() {
            string name = await IoC.UserInput.ShowSingleInputDialogAsync("Rename scene", "Input a new name for this scene:", this.DisplayName, this.Deck.RenameValidator);
            if (name != null) {
                this.DisplayName = name;
            }
        }

        public async Task RemoveAction() {
            await this.Deck.RemoveItemsAction(this);
        }

        /// <summary>
        /// Disposes this scene and all sources that it defines
        /// </summary>
        public void Dispose() {
            using (ExceptionStack stack = new ExceptionStack($"Exception while disposing {this.GetType()}")) {
                try {
                    this.DisposeCore(stack);
                }
                catch (Exception e) {
                    stack.Push(new Exception($"Unexpected exception while invoking {nameof(this.DisposeCore)}", e));
                }
            }
        }

        protected virtual void DisposeCore(ExceptionStack e) {
            using (ExceptionStack stack = new ExceptionStack("Failed to fully dispose one or more source deck items", false)) {
                this.SourceDeck.DisposeItemsAndClear(stack);
                if (stack.TryGetException(out Exception exception)) {
                    e.Push(exception);
                }
            }
        }
    }
}