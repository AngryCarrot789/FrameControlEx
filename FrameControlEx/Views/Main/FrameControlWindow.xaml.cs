using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using FrameControlEx.Core.FrameControl;
using FrameControlEx.Core.FrameControl.Scene;
using FrameControlEx.Core.Notifications;
using FrameControlEx.Core.Utils;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace FrameControlEx.Views.Main {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class FrameControlWindow : WindowEx, IFrameControlView {
        public FrameControlViewModel FrameControl { get; }

        private int buffer_index;
        private readonly double[] buffer_time;
        private DateTime lastRenderTime = DateTime.Now;

        private volatile int shouldRenderFrame;

        public FrameControlWindow() {
            this.InitializeComponent();
            this.DataContext = this.FrameControl = new FrameControlViewModel(this);

            this.FrameControl.SceneDeck.AddNewScene("Scene 1");
            this.FrameControl.RenderCallback = () => {
                try {
                    this.shouldRenderFrame = 1;
                    this.Dispatcher.Invoke(this.ViewPortElement.InvalidateVisual, DispatcherPriority.Loaded);
                }
                catch (TaskCanceledException) {
                    // prevents visual studios catching/breakpointing when the main window closes
                }
            };

            this.buffer_time = new double[20];

            // DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render);
            // timer.Interval = TimeSpan.FromMilliseconds(1);
            // timer.Tick += (sender, args) => {
            //     this.shouldRenderFrame = 1;
            //     this.ViewPortElement.InvalidateVisual();
            // };
            // timer.Start();
        }

        protected override void OnActivated(EventArgs e) {
            base.OnActivated(e);
        }

        protected override void OnDeactivated(EventArgs e) {
            base.OnDeactivated(e);
        }

        protected override async Task<bool> OnClosingAsync() {
            this.PushNotification(new NotificationViewModel() {
                Message = "Stopping render thread and cleaning up resources..."
            });

            // so that the message shows in the window
            await Task.Run(async () => {
                await this.Dispatcher.Invoke(this.FrameControl.OnDisposeAsync, DispatcherPriority.Background);
            });

            return true;
        }

        public void AddDateTime(double time) {
            int index = this.buffer_index;
            if (index >= this.buffer_time.Length) {
                this.buffer_index = index = 0;
            }
            else {
                ++this.buffer_index;
            }

            this.buffer_time[index] = time;
        }

        public double GetAverageTime() {
            return this.buffer_time.Sum() / this.buffer_time.Length;
        }

        private void OnSceneSourceOrOutputSelectionChanged(object sender, SelectionChangedEventArgs e) {
            IList selection = e.AddedItems;
            if (selection == null || selection.Count != 1) {
                this.SettingsPanel.DataContext = null;
            }
            else if (selection[0] is FrameworkElement element) {
                this.SettingsPanel.DataContext = element.DataContext;
            }
            else {
                this.SettingsPanel.DataContext = selection[0];
            }
        }

        private void ListBox_GotFocus(object sender, RoutedEventArgs e) {
            if (sender is ListBox lb) {
                object selected = lb.SelectedItem;
                if (selected is FrameworkElement element) {
                    this.SettingsPanel.DataContext = element.DataContext;
                }
                else if (selected != null) {
                    this.SettingsPanel.DataContext = selected;
                }
            }
        }

        private void ViewPortElement_OnPaintSurface(object sender, SKPaintSurfaceEventArgs e) {
            if (Interlocked.CompareExchange(ref this.shouldRenderFrame, 0, 1) == 0)
                return;

            DateTime now = DateTime.Now;
            TimeSpan diff = now - this.lastRenderTime;
            this.lastRenderTime = now;
            this.AddDateTime(diff.TotalMilliseconds);
            double interval = this.GetAverageTime();
            this.AverageTime.Text = $"INTERVAL: {Math.Round(interval, 2).ToString().FitLength(6)}\t ({Math.Round(1000d / interval, 2).ToString().FitLength(8)} FPS)";

            FrameControlViewModel frameControl = this.FrameControl ?? throw new Exception($"No {nameof(FrameControlViewModel)} available");
            SKSurface surface = e.Surface;
            SKImageInfo rawImageInfo = e.RawInfo;

            SceneViewModel active = frameControl.SceneDeck.PrimarySelectedItem;
            if (active == null) {
                // // draw colourful background when no scenes are active
                // float x1 = 0, y1 = 0;
                // float x2 = rawImageInfo.Width, y2 = rawImageInfo.Height;
                // 
                // surface.Canvas.DrawVertices(SKVertexMode.Triangles, new SKPoint[] {
                //     new SKPoint(x1, y1),
                //     new SKPoint(x2, y1),
                //     new SKPoint(x2, y2),
                //     new SKPoint(x1, y1),
                //     new SKPoint(x2, y2),
                //     new SKPoint(x1, y2)
                // }, new SKColor[] {
                //     SKColors.Red,
                //     SKColors.Green,
                //     SKColors.Blue,
                //     SKColors.Red,
                //     SKColors.Blue,
                //     SKColors.White,
                // }, new SKPaint() {
                // });

                // surface.Canvas.Clear(SKColors.Black);
                return;
            }

            RenderContext context = new RenderContext(frameControl, surface, surface.Canvas, rawImageInfo);
            context.RenderScene(active);
            // TODO: Maybe move this code somewhere else... maybe? dunno

            e.Surface.Flush();
            foreach (OutputViewModel output in frameControl.OutputDeck.Items) {
                if (output.IsEnabled && output is IVisualOutput visual) {
                    visual.OnAcceptFrame(context);
                }
            }
        }

        public void CloseWindow() {
            this.Close();
        }

        public Task CloseWindowAsync() {
            return this.CloseAsync();
        }

        public void PushNotification(NotificationViewModel notification) {
            if (string.IsNullOrWhiteSpace(notification.Header)) {
                if (string.IsNullOrWhiteSpace(notification.Message)) {
                    return;
                }
                else {
                    this.ActivityBarTextBlock.Text = notification.Message;
                }
            }
            else if (string.IsNullOrWhiteSpace(notification.Message)) {
                this.ActivityBarTextBlock.Text = notification.Header;
            }
            else {
                this.ActivityBarTextBlock.Text = notification.Header + ": " + notification.Message;
            }
        }
    }
}
