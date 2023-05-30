using System;
using System.Threading.Tasks;
using FrameControlEx.Core;
using FrameControlEx.Core.FrameControl.ViewModels.Scene;
using FrameControlEx.Core.Services;
using FrameControlEx.FrameControl.Views;

namespace FrameControlEx.FrameControl {
    [Service(typeof(IOutputSelector))]
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

        public async Task<SceneViewModel> SelectScene(SceneDeckViewModel deck, Predicate<SceneViewModel> filter) {
            SceneSelectorWindow selectorWindow = new SceneSelectorWindow();
            SceneSelectorViewModel vm = new SceneSelectorViewModel(deck, filter);
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