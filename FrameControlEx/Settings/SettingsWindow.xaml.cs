using System.Windows;

namespace FrameControlEx.Settings {
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window {
        public SettingsWindow() {
            this.InitializeComponent();
            this.DataContext = new SettingsViewModel();
        }
    }
}
