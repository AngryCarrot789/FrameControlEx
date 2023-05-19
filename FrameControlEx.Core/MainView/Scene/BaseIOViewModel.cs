namespace FrameControlEx.Core.MainView.Scene {
    public class BaseIOViewModel : BaseViewModel {
        private string readableName;
        public string ReadableName {
            get => this.readableName;
            set => this.RaisePropertyChanged(ref this.readableName, value);
        }
    }
}
