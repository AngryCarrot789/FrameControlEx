using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Models;
using FrameControlEx.Core.FrameControl.Scene;
using FrameControlEx.Core.Utils;

namespace FrameControlEx.Core.FrameControl {
    public class FrameControlViewModel : BaseViewModel {
        public SceneDeckViewModel SceneDeck { get; }
        public OutputDeckViewModel OutputDeck { get; }

        public bool UsePrecisionTimingMode {
            get => this.Model.UsePrecisionTimingMode;
            set {
                this.Model.UsePrecisionTimingMode = value;
                this.RaisePropertyChanged();
            }
        }

        public AsyncRelayCommand SwitchPrecisionTimingModeCommand { get; }

        public IFrameControlView View { get; }

        public FrameControlModel Model { get; }

        public FrameControlViewModel(IFrameControlView view, FrameControlModel model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model), "Model cannot be null");
            this.View = view ?? throw new ArgumentNullException(nameof(view));
            this.SceneDeck = new SceneDeckViewModel(this);
            this.OutputDeck = new OutputDeckViewModel(this);
            IoC.Settings.OnSettingsModified += this.OnSettingsModified;
            this.SwitchPrecisionTimingModeCommand = new AsyncRelayCommand(async () => {
                await this.SwitchPrecisionMode();
            });

            this.OnSettingsModified(IoC.Settings.ActiveSettings);
            this.Model.Timer.Start(this.UsePrecisionTimingMode);
        }

        private async Task SwitchPrecisionMode() {
            this.UsePrecisionTimingMode = !this.UsePrecisionTimingMode;
            await this.Model.Timer.RestartAsync(this.UsePrecisionTimingMode);
        }

        private void OnSettingsModified(SettingsViewModel settings) {
            this.Model.Timer.Interval = (long) Math.Round(1000d / settings.FrameRate);
        }

        public async Task OnDisposeAsync() {
            await this.Model.Timer.StopAsync();
            using (ExceptionStack stack = new ExceptionStack("Exception disposing scenes", false)) {
                this.SceneDeck.DisposeItemsAndClear(stack);
                #if DEBUG
                if (stack.TryGetException(out Exception exception)) {
                    Debug.WriteLine("Failed to dispose scenes: " + exception.GetToString());
                }
                #endif
            }

            // clear all scenes
            this.SceneDeck.DisposeAllAndClear(new List<(SceneViewModel, Exception)>());
        }
    }
}