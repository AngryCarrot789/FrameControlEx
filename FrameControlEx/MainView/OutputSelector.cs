using System;
using System.Threading.Tasks;
using FrameControlEx.Core.MainView.Scene;
using FrameControlEx.Core.MainView.Scene.Outputs;
using FrameControlEx.Core.Services;
using FrameControlEx.MainView.Views;

namespace FrameControlEx.MainView {
    public class BufferOutputSelector : IOutputSelector {
        public Task<BasicBufferOutputViewModel> SelectOutput(OutputDeckViewModel deck, Predicate<OutputViewModel> filter) {
            OutputSelectorWindow selectorWindow = new OutputSelectorWindow();
            OutputSelectorViewModel vm = new OutputSelectorViewModel()

            if (selectorWindow.ShowDialog() != true) {
                return Task.FromResult<BasicBufferOutputViewModel>(null);
            }


        }
    }
}