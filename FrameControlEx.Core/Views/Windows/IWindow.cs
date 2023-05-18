using System.Threading.Tasks;

namespace FrameControlEx.Core.Views.Windows {
    public interface IWindow : IViewBase {
        void CloseWindow();

        Task CloseWindowAsync();
    }
}