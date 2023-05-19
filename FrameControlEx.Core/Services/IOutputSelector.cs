using System;
using System.Threading.Tasks;
using FrameControlEx.Core.MainView.Scene;
using FrameControlEx.Core.MainView.Scene.Outputs;

namespace FrameControlEx.Core.Services {
    public interface IOutputSelector {
        Task<OutputViewModel> SelectOutput(OutputDeckViewModel deck, Predicate<OutputViewModel> filter);
    }
}