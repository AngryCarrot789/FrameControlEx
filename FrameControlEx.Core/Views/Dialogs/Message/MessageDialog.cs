using System.Threading.Tasks;
using FrameControlEx.Core.Views.Dialogs.Modal;

namespace FrameControlEx.Core.Views.Dialogs.Message {
    /// <summary>
    /// A helper view model for managing message dialogs that can have multiple buttons
    /// </summary>
    public partial class MessageDialog : BaseDynamicDialogViewModel {
        public MessageDialog(string primaryResult = null, string defaultResult = null) : base(primaryResult, defaultResult) {
        }

        /// <summary>
        /// Creates a clone of this dialog. The returned instance will not be read only, allowing it to be further modified
        /// </summary>
        /// <returns></returns>
        public MessageDialog Clone() {
            MessageDialog dialog = new MessageDialog() {
                titlebar = this.titlebar,
                header = this.header,
                message = this.message,
                automaticResult = this.automaticResult,
                ShowAlwaysUseNextResultOption = this.ShowAlwaysUseNextResultOption,
                IsAlwaysUseThisOptionChecked = this.IsAlwaysUseThisOptionChecked,
                primaryResult = this.primaryResult,
                defaultResult = this.defaultResult
            };

            foreach (DialogButton button in this.buttons)
                dialog.buttons.Add(button.Clone(dialog));
            return dialog;
        }


        protected override Task<bool?> ShowDialogAsync() {
            return IoC.MessageDialogs.ShowDialogAsync(this);
        }

        public override BaseDynamicDialogViewModel CloneCore() {
            return this.Clone();
        }

        /// <summary>
        /// Creates a disposable usage/state of this message dialog which, if <see cref="IsAlwaysUseNextResultForCurrentQueueChecked"/> is true,
        /// allows the buttons and auto-result to be restored once the usage instance is disposed
        /// <para>
        /// This only needs to be used if you intend on modifying the state of the current <see cref="MessageDialog"/> during some
        /// sort of "queue/collection based" work, and want to restore those changes once you're finished
        /// <para>
        /// An example is loading files; you realistically need to use this in order to restore the <see cref="AutomaticResult"/> to the previous
        /// value if <see cref="IsAlwaysUseNextResultForCurrentQueueChecked"/> is true)
        /// </para>
        /// </para>
        /// </summary>
        /// <returns></returns>
        public MessageDialogUsage Use() {
            return new MessageDialogUsage(this);
        }
    }
}