using System.Threading.Tasks;
using FrameControlEx.Core.MainView.Scene.Outputs;

namespace FrameControlEx.Core.Services {
    public interface IOutputSelector {
        Task<BasicBufferOutputViewModel> SelectOutput();
    }
}