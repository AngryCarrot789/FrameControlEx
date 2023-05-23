namespace FrameControlEx.Core.Notifications {
    public interface INotificationHandler {
        void PushNotification(NotificationViewModel notification);
    }
}