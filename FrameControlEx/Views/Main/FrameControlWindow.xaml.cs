using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FrameControlEx.Core.FrameControl;
using FrameControlEx.Core.FrameControl.Models;
using FrameControlEx.Core.FrameControl.Models.Scene;
using FrameControlEx.Core.FrameControl.Models.Scene.Outputs.Base;
using FrameControlEx.Core.FrameControl.ViewModels;
using FrameControlEx.Core.Notifications;
using FrameControlEx.Core.Utils;
using FrameControlEx.Utils;
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
            this.DataContext = this.FrameControl = new FrameControlViewModel(this, new FrameControlModel());
            this.FrameControl.Model.RenderCallback = () => {
                try {
                    if (Interlocked.CompareExchange(ref this.shouldRenderFrame, 1, 0) == 0) {
                        // this.ViewPortElement.FireRenderAsync();
                        this.Dispatcher.Invoke(this.ViewPortElement.InvalidateVisual);
                    }
                }
                catch (TaskCanceledException) { // prevents visual studios catch this when the main window closes
                    this.shouldRenderFrame = 0;
                }
            };

            // this.ViewPortElement.OnRenderAsync = this.Render;
            this.buffer_time = new double[20];

            // DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render);
            // timer.Interval = TimeSpan.FromMilliseconds(1);
            // timer.Tick += (sender, args) => {
            //     this.shouldRenderFrame = 1;
            //     this.ViewPortElement.InvalidateVisual();
            // };
            // timer.Start();
        }

        protected override async Task<bool> OnClosingAsync() {
            this.PushNotification(new NotificationViewModel() {
                Message = "Stopping render thread and cleaning up resources..."
            });

            await DispatcherUtils.WaitUntilBackgroundActivity(this.Dispatcher);

            // so that the message shows in the window
            await this.Dispatcher.Invoke(this.FrameControl.OnDisposeAsync, DispatcherPriority.Background);
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

        public async Task Render(SKPaintSurfaceEventArgs e) {
            if (Interlocked.CompareExchange(ref this.shouldRenderFrame, 0, 1) != 1) {
                return;
            }

            DateTime now = DateTime.Now;
            TimeSpan diff = now - this.lastRenderTime;
            this.lastRenderTime = now;
            this.AddDateTime(diff.TotalMilliseconds);
            double interval = this.GetAverageTime();
            this.AverageTime.Dispatcher.InvokeAsync(() => {
                this.AverageTime.Text = $"INTERVAL: {Math.Round(interval, 2).ToString().FitLength(6)}\t ({Math.Round(1000d / interval, 2).ToString().FitLength(8)} FPS)";
            });

            FrameControlViewModel frameControl = this.FrameControl ?? throw new Exception($"No {nameof(FrameControlViewModel)} available");
            SKSurface surface = e.Surface;
            SKImageInfo rawImageInfo = e.RawInfo;

            SceneModel active = frameControl.SceneDeck.PrimarySelectedItem?.Model;
            if (active == null) {
                return;
            }

            RenderContext context = new RenderContext(frameControl, surface, surface.Canvas, rawImageInfo);
            await context.RenderSceneAsync(active);
            // TODO: Maybe move this code somewhere else... maybe? dunno

            e.Surface.Flush();
            List<OutputModel> outputs = frameControl.OutputDeck.Model.Outputs.ToList();
            await Task.Run(() => {
                foreach (OutputModel output in outputs) {
                    if (output.IsEnabled && output is IVisualOutput visual) {
                        visual.OnAcceptFrame(context);
                    }
                }
            });
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

            SceneModel active = frameControl.SceneDeck.PrimarySelectedItem?.Model;
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
            foreach (OutputModel output in frameControl.OutputDeck.Model.Outputs) {
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
