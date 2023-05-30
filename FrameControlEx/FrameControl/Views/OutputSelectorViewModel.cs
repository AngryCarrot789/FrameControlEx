using System;
using FrameControlEx.Core.FrameControl.ViewModels.Scene;
using FrameControlEx.Core.Views.Dialogs;

namespace FrameControlEx.FrameControl.Views {
    public class OutputSelectorViewModel : BaseConfirmableDialogViewModel {
        public OutputDeckViewModel TargetDeck { get; }

        private OutputViewModel selectedItem;
        public OutputViewModel SelectedItem {
            get => this.selectedItem;
            set {
                this.RaisePropertyChanged(ref this.selectedItem, value);
                this.ConfirmCommand.IsEnabled = value != null;
            }
        }

        private Predicate<OutputViewModel> isItemEnabled;
        public Predicate<OutputViewModel> IsItemEnabled {
            get => this.isItemEnabled;
            set => this.RaisePropertyChanged(ref this.isItemEnabled, value);
        }

        public OutputSelectorViewModel(OutputDeckViewModel deck, Predicate<OutputViewModel> filter) {
            this.TargetDeck = deck ?? throw new ArgumentNullException(nameof(deck));
            this.IsItemEnabled = filter;
            this.ConfirmCommand.IsEnabled = false;
        }
    }
}