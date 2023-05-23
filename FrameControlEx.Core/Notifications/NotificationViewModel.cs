namespace FrameControlEx.Core.Notifications {
    public class NotificationViewModel : BaseViewModel {
        private string header;
        private string message;

        /// <summary>
        /// A short description of what the notification is about
        /// </summary>
        public string Header {
            get => this.header;
            set => this.RaisePropertyChanged(ref this.header, value);
        }

        /// <summary>
        /// The notification's main message
        /// </summary>
        public string Message {
            get => this.message;
            set => this.RaisePropertyChanged(ref this.message, value);
        }
    }
}