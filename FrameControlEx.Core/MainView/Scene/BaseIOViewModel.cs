﻿using System;
using System.Threading.Tasks;
using FrameControlEx.Core.Utils;
using FrameControlEx.Core.Views.Dialogs.UserInputs;

namespace FrameControlEx.Core.MainView.Scene {
    /// <summary>
    /// Base class for sources and outputs
    /// </summary>
    public abstract class BaseIOViewModel : BaseViewModel, IDisposable {
        private string readableName;
        public string ReadableName {
            get => this.readableName;
            set => this.RaisePropertyChanged(ref this.readableName, value);
        }

        private bool isEnabled = true;
        public bool IsEnabled {
            get => this.isEnabled;
            set => this.RaisePropertyChanged(ref this.isEnabled, value);
        }

        public AsyncRelayCommand RenameCommand { get; }
        public RelayCommand EnableCommand { get; }
        public RelayCommand DisableCommand { get; }
        public RelayCommand ToggleEnabledCommand { get; }

        protected BaseIOViewModel() {
            this.RenameCommand = new AsyncRelayCommand(this.RenameAction);
            this.EnableCommand = new RelayCommand(() => this.IsEnabled = true, () => !this.IsEnabled);
            this.DisableCommand = new RelayCommand(() => this.IsEnabled = true, () => !this.IsEnabled);
            this.ToggleEnabledCommand = new RelayCommand(() => this.IsEnabled = !this.IsEnabled);
        }

        public virtual async Task RenameAction() {
            string name = await IoC.UserInput.ShowSingleInputDialogAsync("Rename", "Input a new name:", this.ReadableName ?? "name here", Validators.ForNonEmptyString("Name cannot be empty"));
            if (!string.IsNullOrEmpty(name)) {
                this.ReadableName = name;
            }
        }

        // The disposal is not designed to work in the destructor/finalizer

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

        /// <summary>
        /// The core method for disposing of sources and outputs. This method really should not throw,
        /// and instead, exceptions should be added to the given <see cref="ExceptionStack"/>
        /// </summary>
        /// <param name="e">The exception stack in which exception should be added into when encountered during disposal</param>
        /// <param name="isDisposing"></param>
        protected virtual void DisposeCore(ExceptionStack e) {

        }
    }
}
