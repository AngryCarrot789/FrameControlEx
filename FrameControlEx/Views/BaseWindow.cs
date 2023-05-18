using System.Threading.Tasks;
using FrameControlEx.Core.Views.Windows;

namespace FrameControlEx.Views {
    public class BaseWindow : WindowViewBase, IWindow {
        public void CloseWindow() {
            this.Close();
        }

        public Task CloseWindowAsync() {
            return base.CloseAsync();
        }
    }
}