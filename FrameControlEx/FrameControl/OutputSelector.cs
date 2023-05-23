using System;
using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Scene;
using FrameControlEx.Core.Services;
using FrameControlEx.MainView.Views;

namespace FrameControlEx.MainView {
    public class OutputSelector : IOutputSelector {
        public async Task<OutputViewModel> SelectOutput(OutputDeckViewModel deck, Predicate<OutputViewModel> filter) {
            OutputSelectorWindow selectorWindow = new OutputSelectorWindow();
            OutputSelectorViewModel vm = new OutputSelectorViewModel(deck, filter);
            selectorWindow.DataContext = vm;
            if (selectorWindow.ShowDialog() != true) {
                return null;
            }

            if (filter == null || filter(vm.SelectedItem)) {
                return vm.SelectedItem;
            }

            return null;
        }
    }
}