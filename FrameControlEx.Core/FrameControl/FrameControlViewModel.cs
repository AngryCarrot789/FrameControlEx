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
        private readonly Task renderTask;
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
        // 1.71 is the average yield interval
        // private static readonly long YIELD_MILLIS_IN_TICKS = (long) (1.71d * Time.TICK_PER_MILLIS);
        private static readonly long YIELD_MILLIS_IN_TICKS = Time.TICK_PER_MILLIS / 20;

        private long nextTickTime;

        #if DEBUG
        private static long MIN_YIELD_TIME;
        private static long MAX_YIELD_TIME;
        #endif

        private void RenderMain() {
            while (this.isThreadRunning) {
                // Get the target time that the action should be executed
                // While the waiting time is larger than the thread splice time
                // (target - time) == duration to wait

                #if DEBUG // helpful for debugging the timing
                long tickTime = Time.GetSystemTicks();
                #endif

                long target = this.nextTickTime;
                while ((target - Time.GetSystemTicks()) > THREAD_SPLICE_IN_TICKS) {
                    Thread.Sleep(1); // sleep for roughly 15-16ms, on windows at least
                }

                // targetTime will likely be larger than GetSystemTicks(), e.g the interval is 20ms
                // and we delayed for about 16ms, so extraWaitTime is about 4ms
                while ((target - Time.GetSystemTicks()) > YIELD_MILLIS_IN_TICKS) {
                    // Thread.Sleep(1);
                    // Yield may result in more precise timing

                    #if DEBUG
                    long a = Time.GetSystemTicks();
                    // Thread.Yield();
                    Thread.Sleep(0);
                    long b = Time.GetSystemTicks() - a;
                    if (b > MAX_YIELD_TIME)
                        MAX_YIELD_TIME = b;
                    if (b < MIN_YIELD_TIME)
                        MIN_YIELD_TIME = b;
                    #else
                    Thread.Yield();
                    #endif
                }

                // CPU intensive wait
                #if DEBUG
                long time = Time.GetSystemTicks();
                while (time < target) {
                    Thread.SpinWait(16); // SpinWait may result in more precise timing
                    time = Time.GetSystemTicks();
                }
                #else
                while (Time.GetSystemTicks() < target) {
                    Thread.SpinWait(16); // SpinWait may result in more precise timing
                }
                #endif

                this.nextTickTime = Time.GetSystemTicks() + this.targetIntervalTicks;
                this.RenderCallback?.Invoke();

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