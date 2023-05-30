using System;
using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Models.Scene;
using FrameControlEx.Core.Utils;
using FrameControlEx.Core.Views.Dialogs.UserInputs;

namespace FrameControlEx.Core.FrameControl.ViewModels.Scene {
    /// <summary>
    /// Base class for sources and outputs
    /// </summary>
    public abstract class BaseIOViewModel : BaseViewModel, IDisposable {
        public string ReadableName {
            get => this.Model.ReadableName;
            set {
                this.Model.ReadableName = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsEnabled {
            get => this.Model.IsEnabled;
            set {
                this.Model.IsEnabled = value;
                this.RaisePropertyChanged();
                this.OnIsEnabledChanged();
            }
        }

        public AsyncRelayCommand RenameCommand { get; }
        public RelayCommand EnableCommand { get; }
        public RelayCommand DisableCommand { get; }
        public RelayCommand ToggleEnabledCommand { get; }

        public BaseIOModel Model { get; }

        protected BaseIOViewModel(BaseIOModel model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model), "Model cannot be null");
            this.RenameCommand = new AsyncRelayCommand(this.RenameAction);
            this.EnableCommand = new RelayCommand(() => this.IsEnabled = true, () => !this.IsEnabled);
            this.DisableCommand = new RelayCommand(() => this.IsEnabled = false, () => this.IsEnabled);
            this.ToggleEnabledCommand = new RelayCommand(() => this.IsEnabled = !this.IsEnabled);
        }

        protected virtual void OnIsEnabledChanged() {
            this.EnableCommand.RaiseCanExecuteChanged();
            this.DisableCommand.RaiseCanExecuteChanged();
            this.ToggleEnabledCommand.RaiseCanExecuteChanged();
        }

        public virtual async Task RenameAction() {
            string name = await IoC.UserInput.ShowSingleInputDialogAsync("Rename", "Input a new name:", this.ReadableName ?? "name here", Validators.ForNonEmptyString("Name cannot be empty"));
            if (!string.IsNullOrEmpty(name)) {
                this.ReadableName = name;
            }
        }

        public void Dispose() {
            using (ExceptionStack stack = new ExceptionStack($"Exception while disposing {this.GetType()}")) {
                try {
                    this.DisposeCore(stack);
                }
                catch (Exception e) {
                    stack.Push(new Exception($"Unexpected exception while invoking {nameof(this.DisposeCore)}", e));
                }
            }
        }

        protected virtual void DisposeCore(ExceptionStack stack) {
            try {
                this.Model.Dispose();
            }
            catch (Exception e) {
                stack.Push(new Exception("Failed to dispose model", e));
            }
        }
    }
}
