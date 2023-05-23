using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FrameControlEx.Core.Utils;

namespace FrameControlEx.Core.Timing {
    /// <summary>
    /// A class for executing actions only after an input event is received after a certain delay
    /// </summary>
    public class IdleEventService {
        public delegate void BeginActionEvent();
        public event BeginActionEvent OnIdle;

        private DateTime lastInput;
        private volatile bool canFireEvent;
        private volatile bool stopTask;

        public TimeSpan InputInterval { get; set; }

        public bool CanFireNextTick {
            get => this.canFireEvent;
            set => this.canFireEvent = value;
        }

        public IdleEventService() {
            this.InputInterval = TimeSpan.FromMilliseconds(200);
            this.Start();
        }

        private void Start() {
            Task.Run(async () => {
                while (!this.stopTask) {
                    if ((DateTime.Now - this.lastInput) > this.InputInterval && this.canFireEvent) {
                        this.canFireEvent = false;
                        if (this.stopTask) {
                            break;
                        }

                        try {
                            await IoC.Dispatcher.InvokeAsync(this.FireEvent);
                        }
                        catch (ThreadAbortException) {
                            return;
                        }
                        catch (Exception e) {
                            Debug.WriteLine(e.GetToString());
                            #if DEBUG
                            throw;
                            #else
                            await IoC.MessageDialogs.ShowMessageAsync("Error", "An error occurred during an idle event service: " + e.Message);
                            #endif
                        }
                    }

                    await Task.Delay(50);
                }
            });
        }

        public void FireEvent() {
            this.OnIdle?.Invoke();
        }

        public void OnInput() {
            this.canFireEvent = true;
            this.lastInput = DateTime.Now;
        }

        public void ForceAction() {
            this.canFireEvent = false;
            this.lastInput = DateTime.Now;
            this.FireEvent();
        }

        public void Dispose() {
            this.stopTask = true;
        }
    }
}