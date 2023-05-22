using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FrameControlEx.Core.MainView;
using FrameControlEx.Core.MainView.Scene;
using FrameControlEx.Core.MainView.Scene.Outputs;
using FrameControlEx.Core.MainView.Scene.Sources;
using FrameControlEx.Core.Utils;
using FrameControlEx.Imaging;
using FrameControlEx.Utils;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace FrameControlEx.Views.Main {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowEx {
        public FrameControlViewModel ViewModel { get => (FrameControlViewModel) this.DataContext; }

        private int buffer_index;
        private readonly double[] buffer_time;
        private DateTime lastRenderTime = DateTime.Now;

        public MainWindow() {
            this.InitializeComponent();
            FrameControlViewModel frameControl = new FrameControlViewModel();
            frameControl.SceneDeck.AddNewScene("Scene 1");
            frameControl.RenderCallback = () => {
                this.Dispatcher.Invoke(this.ViewPortElement.InvalidateVisual, DispatcherPriority.Render);
            };

            this.DataContext = frameControl;
            this.buffer_time = new double[20];

            // DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render);
            // timer.Interval = TimeSpan.FromMilliseconds(1);
            // timer.Tick += (sender, args) => {
            //     this.ViewPortElement.InvalidateVisual();
            //     // this.ViewPortElement.UpdateLayout();
            //     DateTime now = DateTime.Now;
            //     TimeSpan diff = now - this.lastRenderTime;
            //     this.lastRenderTime = now;
            //     this.AddDateTime(diff.TotalMilliseconds);
            //     double interval = this.GetAverageTime();
            //     this.AverageTime.Text = $"{Math.Round(interval, 2).ToString().FitLength(8)}\t ({Math.Round(1000d / interval, 2).ToString().FitLength(6)} FPS)";
            // };
            // timer.Start();
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            if (this.DataContext is FrameControlViewModel vm) {
                vm.OnDispose();
            }
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
            DateTime now = DateTime.Now;
            TimeSpan diff = now - this.lastRenderTime;
            this.lastRenderTime = now;
            this.AddDateTime(diff.TotalMilliseconds);
            double interval = this.GetAverageTime();
            this.AverageTime.Text = $"{Math.Round(interval, 2).ToString().FitLength(8)}\t ({Math.Round(1000d / interval, 2).ToString().FitLength(6)} FPS)";

            FrameControlViewModel frameControl = this.ViewModel ?? throw new Exception($"No {nameof(FrameControlViewModel)} available");
            SKCanvas canvas = e.Surface.Canvas;

            SceneViewModel active = frameControl.SceneDeck.SelectedItem;
            if (active == null) {
                canvas.Clear(SKColors.Black);
                return;
            }

            if (active.ClearScreenOnRender) {
                canvas.Clear(active.BackgroundColour);
            }

            foreach (SourceViewModel source in active.SourceDeck.Items) {
                if (!source.IsEnabled) {
                    continue;
                }

                // TODO: Maybe create separate rendering classes for each type of source
                if (source is VisualSourceViewModel visual) {
                    visual.OnTickVisual();
                    Vector2 scale = visual.Scale, pos = visual.Pos, origin = visual.ScaleOrigin;
                    if (source is ImageSourceViewModel imgSrc) {
                        if (imgSrc.Image is SkiaImageFactory.SkiaImage img) {
                            SKMatrix matrix = canvas.TotalMatrix;
                            canvas.Translate(pos.X, pos.Y);
                            canvas.Scale(scale.X, scale.Y, (float) img.image.Width * origin.X, (float) img.image.Height * origin.Y);
                            canvas.DrawImage(img.image, 0, 0);
                            canvas.SetMatrix(matrix);
                        }
                    }
                    else if (source is LoopbackSourceViewModel input) {
                        if (input.TargetOutput != null && input.TargetOutput.IsEnabled & input.TargetOutput.lastFrame != null) {
                            SKMatrix matrix = canvas.TotalMatrix;
                            canvas.Translate(pos.X, pos.Y);
                            SKImage frame = input.TargetOutput.lastFrame;
                            canvas.Scale(scale.X, scale.Y, (float) frame.Width * origin.X, (float) frame.Height * origin.Y);
                            canvas.DrawImage(frame, 0, 0);
                            canvas.SetMatrix(matrix);
                        }
                    }
                }
            }

            // TODO: Maybe move this code somewhere else... maybe? dunno

            e.Surface.Flush();
            foreach (OutputViewModel output in frameControl.OutputDeck.Items) {
                if (!output.IsEnabled) {
                    continue;
                }

                if (output is VisualOutputViewModel visual) {
                    visual.OnAcceptFrame(e.Surface);
                }
            }
        }
    }
}
