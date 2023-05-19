using System;
using FrameControlEx.Core.MainView.Scene;
using FrameControlEx.Core.Views.Dialogs;

namespace FrameControlEx.MainView.Views {
    public class BBOutputSelectorViewModel : BaseDialogViewModel {
        public OutputDeckViewModel TargetDeck { get; }

        private OutputViewModel selectedItem;
        public OutputViewModel SelectedItem {
            get => this.selectedItem;
            set => this.RaisePropertyChanged(ref this.selectedItem, value);
        }

        private Predicate<OutputViewModel> isItemEnabled;
        public Predicate<OutputViewModel> IsItemEnabled {
            get => this.isItemEnabled;
            set => this.RaisePropertyChanged(ref this.isItemEnabled, value);
        }

        public BBOutputSelectorViewModel(OutputDeckViewModel deck, Predicate<OutputViewModel> filter) {
            this.TargetDeck = deck ?? throw new ArgumentNullException(nameof(deck));
            this.IsItemEnabled = filter;
        }
    }
}