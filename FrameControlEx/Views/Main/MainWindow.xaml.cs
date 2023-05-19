using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using System.Reflection;
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

        private DateTime lastRenderTime = DateTime.Now;

        public MainWindow() {
            this.InitializeComponent();
            this.DataContext = new FrameControlViewModel(new WPFSkiaRenderSurface(this));
            this.ViewModel.SceneDeck.AddNewScene("Scene 1");
            this.buffer_time = new double[20];

            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render);
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += (sender, args) => {
                this.ViewPortElement.InvalidateVisual();
                // this.ViewPortElement.UpdateLayout();
                DateTime now = DateTime.Now;
                TimeSpan diff = now - this.lastRenderTime;
                this.lastRenderTime = now;
                this.AddDateTime(diff.TotalMilliseconds);
                double interval = this.GetAverageTime();

                this.AverageTime.Text = $"{Math.Round(interval, 2).ToString().FitLength(8)}\t ({Math.Round(1000d / interval, 2).ToString().FitLength(6)} FPS)";
            };

            timer.Start();
        }

        private static readonly FieldInfo BitmapField;

        static MainWindow() {
            BitmapField = typeof(SKElement).GetField("bitmap", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public WriteableBitmap GetViewPortBitmap() {
            return (WriteableBitmap) BitmapField.GetValue(this.ViewPortElement);
        }

        private void ViewPortElement_OnPaintSurface(object sender, SKPaintSurfaceEventArgs e) {
            FrameControlViewModel view = this.ViewModel ?? throw new Exception($"No {nameof(FrameControlViewModel)} available");
            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Black);

            SceneViewModel active = view.SceneDeck.SelectedItem;
            if (active == null || active.SourceDeck.Items.Count < 1) {
                return;
            }

            e.Surface.Flush();

            foreach (SourceViewModel source in active.SourceDeck.Items) {
                if (!source.IsEnabled) {
                    continue;
                }

                // TODO: Maybe create separate rendering classes for each type of source
                if (source is VisualSourceViewModel visual) {
                    visual.OnTickVisual();
                    Vector2 scale = visual.Scale, pos = visual.Pos;
                    if (source is ImageSourceViewModel imgSrc) {
                        if (imgSrc.Image is SkiaImageFactory.SkiaImage img) {
                            SKMatrix matrix = canvas.TotalMatrix;
                            canvas.Scale(new SKPoint(scale.X, scale.Y));
                            canvas.DrawImage(img.image, pos.X, pos.Y);
                            canvas.SetMatrix(matrix);
                        }
                    }
                    else if (source is SameInstanceInputViewModel input) {
                        if (input.TargetOutput != null && input.TargetOutput.IsEnabled & input.TargetOutput.lastFrame != null) {
                            SKMatrix matrix = canvas.TotalMatrix;
                            canvas.Scale(new SKPoint(scale.X, scale.Y));
                            canvas.DrawImage(input.TargetOutput.lastFrame, pos.X, pos.Y);
                            canvas.SetMatrix(matrix);
                        }
                    }
                }
            }

            e.Surface.Flush();

            foreach (OutputViewModel output in active.OutputDeck.Items) {
                if (!output.IsEnabled) {
                    continue;
                }

                if (output is VisualOutputViewModel visual) {
                    visual.OnAcceptFrame(e.Surface);
                }
            }
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

        private class WPFSkiaRenderSurface : IRenderSurface {
            private readonly MainWindow window;

            public WPFSkiaRenderSurface(MainWindow window) {
                this.window = window;
            }

            public void Render() {
                this.window.ViewPortElement.InvalidateVisual();
            }
        }
    }
}
