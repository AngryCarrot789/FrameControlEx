using System;
using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Scene;

namespace FrameControlEx.Core.Services {
    public interface IOutputSelector {
        Task<OutputViewModel> SelectOutput(OutputDeckViewModel deck, Predicate<OutputViewModel> filter);
    }
}