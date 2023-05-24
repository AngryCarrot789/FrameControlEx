using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Scene;
using FrameControlEx.Core.Utils;

namespace FrameControlEx.Core.FrameControl {
    public class FrameControlViewModel : BaseViewModel {
        public SceneDeckViewModel SceneDeck { get; }

        public OutputDeckViewModel OutputDeck { get; }

        public Action RenderCallback { get; set; }

        private volatile bool usePrecisionTimingMode = true;
        public bool UsePrecisionTimingMode {
            get => this.usePrecisionTimingMode;
            set {
                this.usePrecisionTimingMode = value;
                this.RaisePropertyChanged();
            }
        }

        private volatile bool isRenderLoopRunning;
        private readonly Task renderTask;
        private long targetIntervalTicks;

        public IFrameControlView View { get; }

        public FrameControlViewModel(IFrameControlView view) {
            this.View = view ?? throw new ArgumentNullException(nameof(view));
            this.SceneDeck = new SceneDeckViewModel(this);
            this.OutputDeck = new OutputDeckViewModel(this);
            IoC.Settings.OnSettingsModified += this.OnSettingsModified;
            this.OnSettingsModified(IoC.Settings.ActiveSettings);

            this.isRenderLoopRunning = true;
            this.renderTask = Task.Factory.StartNew(this.RenderMain, TaskCreationOptions.LongRunning);
        }

        private void OnSettingsModified(SettingsViewModel settings) {
            this.targetIntervalTicks = (long) Math.Floor((1000d / settings.FrameRate) * Time.TICK_PER_MILLIS);
        }

        public async Task OnDisposeAsync() {
            this.isRenderLoopRunning = false;
            if (this.renderTask != null && !this.renderTask.IsCanceled && !this.renderTask.IsCompleted) {
                await this.renderTask;
            }

            using (ExceptionStack stack = new ExceptionStack("Exception disposing scenes", false)) {
                this.SceneDeck.DisposeItemsAndClear(stack);
                #if DEBUG
                if (stack.TryGetException(out Exception exception)) {
                    Debug.WriteLine($"Failed to dispose scenes: " + exception.GetToString());
                }
                #endif
            }
            // clear all scenes
            this.SceneDeck.DisposeAllAndClear(new List<(SceneViewModel, Exception)>());
        }

        private const long MILLIS_PER_THREAD_SPLICE = 16; // 16.4
        private static readonly long THREAD_SPLICE_IN_TICKS = (long) (16.4d * Time.TICK_PER_MILLIS);
        // 1.71ms to 2.3ms is the max yield interval i found
        // private static readonly long YIELD_MILLIS_IN_TICKS = (long) (1.71d * Time.TICK_PER_MILLIS);
        private static readonly long YIELD_MILLIS_IN_TICKS = Time.TICK_PER_MILLIS / 10;

        private long nextTickTime;

        #if DEBUG
        private static long MIN_YIELD_TIME;
        private static long MAX_YIELD_TIME;
        #endif

        private void RenderMain() {
            while (this.isRenderLoopRunning) {
                #if DEBUG // helpful for debugging the timing
                long tickTime = Time.GetSystemTicks();
                #endif

                long target = this.nextTickTime;
                if (this.usePrecisionTimingMode) {
                    while ((target - Time.GetSystemTicks()) > THREAD_SPLICE_IN_TICKS) {
                        Thread.Sleep(1);
                    }

                    while ((target - Time.GetSystemTicks()) > YIELD_MILLIS_IN_TICKS) {
                        Thread.Yield();
                    }

                    // CPU intensive wait
                    long time = Time.GetSystemTicks();
                    while (time < target) {
                        Thread.SpinWait(16); // SpinWait may result in more precise timing
                        time = Time.GetSystemTicks();
                    }

                    this.nextTickTime = Time.GetSystemTicks() + this.targetIntervalTicks;
                    this.RenderCallback?.Invoke();
                }
                else {
                    // long timeUntilTick = target - Time.GetSystemTicks();
                    // if (timeUntilTick < THREAD_SPLICE_IN_TICKS && timeUntilTick > YIELD_MILLIS_IN_TICKS) {
                    //     do {
                    //         if (!Thread.Yield())
                    //             Thread.Sleep(0);
                    //     } while ((target - Time.GetSystemTicks()) > YIELD_MILLIS_IN_TICKS);
                    // }

                    // Less precision; 55 fps at 60 fps target
                    if (target <= Time.GetSystemTicks()) {
                        this.nextTickTime = Time.GetSystemTicks() + this.targetIntervalTicks;
                        this.RenderCallback?.Invoke();
                    }
                    // else {
                    // }

                    Thread.Sleep(1);
                    // while ((target - Time.GetSystemTicks()) > YIELD_MILLIS_IN_TICKS) {
                    //     Thread.Sleep(1);
                    // }
                }

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

        /*

            private void RenderMain() {
            while (this.isRenderLoopRunning) {
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
                if (this.usePrecisionTimingMode) {
                    while ((target - Time.GetSystemTicks()) > YIELD_MILLIS_IN_TICKS) {
                        // Thread.Sleep(1);
                        // Yield may result in more precise timing

                        #if DEBUG
                        long a = Time.GetSystemTicks();
                        if (!Thread.Yield())
                            Thread.Sleep(1);
                        long b = Time.GetSystemTicks() - a;
                        if (b > MAX_YIELD_TIME)
                            MAX_YIELD_TIME = b;
                        if (b < MIN_YIELD_TIME)
                            MIN_YIELD_TIME = b;
                        #else
                        Thread.Sleep(0);
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
                }
                else {
                    if (target <= Time.GetSystemTicks()) {
                        this.nextTickTime = Time.GetSystemTicks() + this.targetIntervalTicks;
                        this.RenderCallback?.Invoke();
                    }

                    // while ((target - Time.GetSystemTicks()) > YIELD_MILLIS_IN_TICKS) {
                    //     Thread.Sleep(1);
                    // }
                }

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



         */
    }
}