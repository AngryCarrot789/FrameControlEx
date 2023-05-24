using System;
using System.Threading.Tasks;
using FrameControlEx.Core.Utils;
using FrameControlEx.Core.Views.Dialogs.UserInputs;
using SkiaSharp;

namespace FrameControlEx.Core.FrameControl.Scene {
    public class SceneViewModel : BaseViewModel, IDisposable {
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

        private bool isActive;
        public bool IsActive {
            get => this.isActive;
            set => this.RaisePropertyChanged(ref this.isActive, value);
        }

        private bool isRenderOrderReversed;
        public bool IsRenderOrderReversed {
            get => this.isRenderOrderReversed;
            set => this.RaisePropertyChanged(ref this.isRenderOrderReversed, value);
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
            using (ExceptionStack stack = new ExceptionStack("Failed to dispose source deck items", false)) {
                this.SourceDeck.DisposeItemsAndClear(stack);
                if (stack.TryGetException(out var exception)) {
                    e.Push(exception);
                }
            }
        }
    }
}