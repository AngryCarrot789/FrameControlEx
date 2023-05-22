using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using FrameControlEx.Core.MainView.Scene;
using FrameControlEx.Core.Utils;

namespace FrameControlEx.Core.MainView {
    public class FrameControlViewModel : BaseViewModel {
        private FrameControlSettingsViewModel settings;

        public FrameControlSettingsViewModel Settings {
            get => this.settings;
            set => this.RaisePropertyChanged(ref this.settings, value);
        }

        public SceneDeckViewModel SceneDeck { get; }

        public OutputDeckViewModel OutputDeck { get; }

        public Action RenderCallback { get; set; }

        private volatile bool isThreadRunning;
        private volatile bool isRenderQueued;
        private readonly Task renderTask;
        private long lastRender;
        private long targetIntervalTicks;

        public FrameControlViewModel() {
            this.SceneDeck = new SceneDeckViewModel(this);
            this.OutputDeck = new OutputDeckViewModel(this);
            this.settings = new FrameControlSettingsViewModel();
            this.settings.PropertyChanged += this.SettingsOnPropertyChanged;
            this.settings.Width = 1920;
            this.settings.Height = 1080;
            this.settings.FrameRate = 60;

            this.isThreadRunning = true;
            this.renderTask = Task.Factory.StartNew(this.RenderMain, TaskCreationOptions.LongRunning);
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(FrameControlSettingsViewModel.FrameRate): {
                    this.targetIntervalTicks = (long) Math.Floor((1000d / this.settings.FrameRate) * Time.TICK_PER_MILLIS);
                    break;
                }
            }
        }

        public void OnDispose() {
            this.isThreadRunning = false;
        }

        private const long MILLIS_PER_THREAD_SPLICE = 16;
        private static readonly long THREAD_SPLICE_IN_TICKS = 16L * Time.TICK_PER_MILLIS;
        private static readonly long YIELD_MILLIS_IN_TICKS = 3L * Time.TICK_PER_MILLIS;

        private long nextTickTime;

        private void RenderMain() {
            while (this.isThreadRunning) {
                // Get the target time that the action should be executed
                // While the waiting time is larger than the thread splice time
                // (target - time) == duration to wait
                long tickTime = Time.GetSystemTicks();
                long target = this.nextTickTime;
                while ((target - Time.GetSystemTicks()) > THREAD_SPLICE_IN_TICKS) {
                    Thread.Sleep(1); // sleep for roughly 15-16ms, on windows at least
                }

                // targetTime will likely be larger than GetSystemTicks(), e.g the interval is 20ms
                // and we delayed for about 16ms, so extraWaitTime is about 4ms
                while ((target - Time.GetSystemTicks()) > YIELD_MILLIS_IN_TICKS) {
                    // Thread.Sleep(1);
                    // Yield may result in more precise timing
                    Thread.Yield();
                }

                // CPU intensive wait
                while (Time.GetSystemTicks() < target) {
                    Thread.SpinWait(16);
                    // SpinWait may result in more precise timing
                    // Thread.Yield();
                }

                this.nextTickTime = Time.GetSystemTicks() + this.targetIntervalTicks;
                Action callback = this.RenderCallback;
                callback?.Invoke();

                // long time = Time.GetSystemMillis();
                // long difference = time - this.lastRender;
                // if (difference >= this.targetInterval) {
                //     Func<Task> callback = this.RenderCallback;
                //     if (callback != null) {
                //         await callback();
                //         this.lastRender = time;
                //     }
                // }
            }
        }
    }
}