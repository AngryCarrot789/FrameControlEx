using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using FrameControlEx.Core.Utils;

namespace FrameControlEx.Core.Views.Dialogs.Modal {
    /// <summary>
    /// A helper view model for managing message dialogs that can have multiple buttons
    /// </summary>
    public abstract class BaseWindowDialogViewModel : BaseDynamicDialogViewModel {
        protected string header;
        protected string message;
        protected bool showAlwaysUseNextResultOption;
        protected bool isAlwaysUseThisOptionChecked;
        protected bool canShowAlwaysUseNextResultForCurrentQueueOption;
        protected bool isAlwaysUseThisOptionForCurrentQueueChecked;

        public string Header {
            get => this.header;
            set => this.RaisePropertyChanged(ref this.header, value);
        }

        public string Message {
            get => this.message;
            set => this.RaisePropertyChanged(ref this.message, value);
        }

        /// <summary>
        /// Whether or not to show the "always use next result option" in the GUI
        /// </summary>
        public bool ShowAlwaysUseNextResultOption { // dialog will show "Always use this option"
            get => this.showAlwaysUseNextResultOption;
            set {
                this.EnsureNotReadOnly();
                this.RaisePropertyChanged(ref this.showAlwaysUseNextResultOption, value);
                if (!value && this.IsAlwaysUseThisOptionChecked) {
                    this.IsAlwaysUseThisOptionChecked = false;
                }
            }
        }

        /// <summary>
        /// Whether or not the GUI option to use the next outcome as an automatic result is checked
        /// </summary>
        [Bindable(true)]
        public bool IsAlwaysUseThisOptionChecked {
            get => this.isAlwaysUseThisOptionChecked;
            set {
                this.EnsureNotReadOnly();
                this.isAlwaysUseThisOptionChecked = value && this.ShowAlwaysUseNextResultOption;
                this.RaisePropertyChanged();
                if (!this.isAlwaysUseThisOptionChecked && this.IsAlwaysUseThisOptionForCurrentQueueChecked) {
                    this.IsAlwaysUseThisOptionForCurrentQueueChecked = false;
                }

                this.UpdateButtons();
            }
        }

        public bool CanShowAlwaysUseNextResultForCurrentQueueOption {
            get => this.canShowAlwaysUseNextResultForCurrentQueueOption;
            set {
                this.EnsureNotReadOnly();
                this.RaisePropertyChanged(ref this.canShowAlwaysUseNextResultForCurrentQueueOption, value);
                if (!value && this.IsAlwaysUseThisOptionForCurrentQueueChecked) {
                    this.IsAlwaysUseThisOptionForCurrentQueueChecked = false;
                }
            }
        }

        /// <summary>
        /// Whether or not the GUI option to use the next outcome as an automatic result, but only for the current queue/usage, is checked
        /// </summary>
        [Bindable(true)]
        public bool IsAlwaysUseThisOptionForCurrentQueueChecked {
            get => this.isAlwaysUseThisOptionForCurrentQueueChecked;
            set {
                this.EnsureNotReadOnly();
                this.RaisePropertyChanged(ref this.isAlwaysUseThisOptionForCurrentQueueChecked, value && this.CanShowAlwaysUseNextResultForCurrentQueueOption);
                this.UpdateButtons();
            }
        }

        protected BaseWindowDialogViewModel(string primaryResult = null, string defaultResult = null) : base(primaryResult, defaultResult) {
        }

        public Task<string> ShowAsync(string titlebar, string header, string message) {
            if (this.AutomaticResult != null) {
                return Task.FromResult(this.AutomaticResult);
            }

            if (titlebar != null)
                this.Titlebar = titlebar;
            if (header != null)
                this.Header = header;
            if  (message != null)
                this.Message = message;
            return this.ShowAsync();
        }

        public Task<string> ShowAsync(string titlebar, string message) {
            return this.ShowAsync(titlebar, null, message);
        }

        public Task<string> ShowAsync(string message) {
            return this.ShowAsync(null, message);
        }

        public async Task<string> ShowAsync() {
            if (this.AutomaticResult != null) {
                return this.AutomaticResult;
            }

            string output;
            this.UpdateButtons();
            bool? result = await this.ShowDialogAsync();
            DialogButton button = this.lastClickedButton;
            this.lastClickedButton = null;
            if (result == true) {
                if (button != null) {
                    output = button.ActionType;
                    if (output != null && this.IsAlwaysUseThisOptionChecked) { // (output != null || this.AllowNullButtonActionForAutoResult)
                        this.AutomaticResult = output;
                    }
                }
                else {
                    output = this.PrimaryResult;
                }
            }
            else {
                output = this.DefaultResult;
            }
            return output;
        }

        protected abstract Task<bool?> ShowDialogAsync();
    }
}