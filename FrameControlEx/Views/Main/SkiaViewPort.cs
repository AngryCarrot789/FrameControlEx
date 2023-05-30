using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace FrameControlEx.Views.Main {
    public class SkiaViewPort : FrameworkElement {
        private readonly bool designMode;
        private WriteableBitmap bitmap;
        private IntPtr backBuffer;
        private bool ignorePixelScaling;

        private volatile int wpf_render_state;
        private volatile int async_render_state;
        private volatile int is_async_callback_render;

        private const int S_RENDER_INACTIVE = 0;
        private const int S_RENDER_ACTIVE = 1;

        public SkiaViewPort() {
            this.designMode = DesignerProperties.GetIsInDesignMode(this);
        }

        /// <summary>Gets the current canvas size.</summary>
        /// <value />
        /// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
        public SKSizeI CanvasRenderSize { get; private set; }

        public SKImageInfo FrameInfo { get; private set; }

        /// <summary>
        /// A rendering callback function that takes the rendering context and returns a task which can be awaited
        /// </summary>
        public Func<SKPaintSurfaceEventArgs, Task> OnRenderAsync { get; set; }

        /// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
        /// <summary>When overridden in a derived class, participates in rendering operations that are directed by the layout system. The rendering instructions for this element are not used directly when this method is invoked, and are instead preserved for later asynchronous use by layout and drawing.</summary>
        /// <remarks />
        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);
            if (this.designMode || this.Visibility != Visibility.Visible || PresentationSource.FromVisual(this) == null) {
                return;
            }
            SKSizeI size = this.CreateSize(out _, out double scaleX, out double scaleY);
            this.CanvasRenderSize = size;
            if (size.Width <= 0 || size.Height <= 0) {
                return;
            }
            SKImageInfo info = new SKImageInfo(size.Width, size.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            if (this.bitmap == null || info.Width != this.bitmap.PixelWidth || info.Height != this.bitmap.PixelHeight) {
                this.bitmap = new WriteableBitmap(info.Width, size.Height, 96.0 * scaleX, 96.0 * scaleY, PixelFormats.Pbgra32, null);
            }
            this.bitmap.Lock();
            using (SKSurface surface = SKSurface.Create(info, this.bitmap.BackBuffer, this.bitmap.BackBufferStride)) {
                // this.OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info.WithSize(size), info));
            }
            this.bitmap.AddDirtyRect(new Int32Rect(0, 0, info.Width, size.Height));
            this.bitmap.Unlock();
            drawingContext.DrawImage(this.bitmap, new Rect(0.0, 0.0, this.ActualWidth, this.ActualHeight));
        }

        /*

        /// <summary>
        /// When overridden in a derived class, participates in rendering operations that are directed by the layout system.
        /// The rendering instructions for this element are not used directly when this method is invoked, and are instead
        /// preserved for later asynchronous use by layout and drawing.
        /// </summary>
        /// <param name="dc">The drawing instructions for a specific element. This context is provided to the layout system.</param>
        /// <remarks />
        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            if (this.designMode || this.Visibility != Visibility.Visible || PresentationSource.FromVisual(this) == null) {
                return;
            }

            if (Interlocked.CompareExchange(ref this.is_async_callback_render, 0, 1) == 1) {
                // this is where screen tearing may happen
                this.AddDirtBitmapRect(this.CanvasRenderSize);
                dc.DrawImage(this.bitmap, new Rect(0.0, 0.0, this.ActualWidth, this.ActualHeight));
                this.async_render_state = S_RENDER_INACTIVE;
            }
            else if (Interlocked.CompareExchange(ref this.wpf_render_state, S_RENDER_ACTIVE, S_RENDER_INACTIVE) != S_RENDER_INACTIVE) {
                Debug.WriteLine("Render is already active...?");
                return;
            }

            SKSizeI scaled_size = this.CreateSize(out _, out double scaleX, out double scaleY);
            this.CanvasRenderSize = scaled_size;
            if (scaled_size.Width > 0 && scaled_size.Height > 0) {
                SKImageInfo info = new SKImageInfo(scaled_size.Width, scaled_size.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
                this.FrameInfo = info;
                if (this.bitmap == null || info.Width != this.bitmap.PixelWidth || info.Height != this.bitmap.PixelHeight) {
                    this.bitmap = new WriteableBitmap(info.Width, scaled_size.Height, 96.0 * scaleX, 96.0 * scaleY, PixelFormats.Pbgra32, null);
                    this.bitmap.Lock();
                    this.backBuffer = this.bitmap.BackBuffer;
                    this.bitmap.Unlock();
                }

                if (Interlocked.CompareExchange(ref this.async_render_state, S_RENDER_ACTIVE, S_RENDER_INACTIVE) != S_RENDER_INACTIVE) {
                    this.OnRenderInternal(info, scaled_size);
                    this.async_render_state = 0;
                }
            }

            this.wpf_render_state = S_RENDER_INACTIVE;
        }

        private void AddDirtBitmapRect(SKSizeI size) {
            this.bitmap.Lock();
            this.bitmap.AddDirtyRect(new Int32Rect(0, 0, size.Width, size.Height));
            this.bitmap.Unlock();
        }

        private async void OnRenderInternal(SKImageInfo info, SKSizeI render_size) {
            if (this.bitmap == null)
                return;
            using (SKSurface surface = SKSurface.Create(info, this.backBuffer, this.FrameInfo.Width * this.FrameInfo.BytesPerPixel)) {
                Func<SKPaintSurfaceEventArgs, Task> callback = this.OnRenderAsync;
                if (callback != null) {
                    await callback(new SKPaintSurfaceEventArgs(surface, info.WithSize(render_size), info));
                }
            }

            if (this.wpf_render_state == S_RENDER_INACTIVE && this.is_async_callback_render == 0) {
                this.is_async_callback_render = 1;
                this.Dispatcher.InvokeAsync(this.InvalidateVisual);
            }
            else {
                this.is_async_callback_render = 0;
            }
        }

        public void FireRenderAsync() {
            this.OnRenderInternal(this.FrameInfo, this.CanvasRenderSize);
        }

        */

        /// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
        /// <summary>Raises the SizeChanged event, using the specified information as part of the eventual event data.</summary>
        /// <remarks />
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);
            this.InvalidateVisual();
        }

        private SKSizeI CreateSize(out SKSizeI unscaledSize, out double scaleX, out double scaleY) {
            double actualWidth = this.ActualWidth;
            double actualHeight = this.ActualHeight;
            if (IsPositive(actualWidth) && IsPositive(actualHeight)) {
                unscaledSize = new SKSizeI((int) actualWidth, (int) actualHeight);
                PresentationSource source = PresentationSource.FromVisual(this);
                if (source?.CompositionTarget is CompositionTarget target) {
                    Matrix t2d = target.TransformToDevice;
                    scaleX = t2d.M11;
                    scaleY = t2d.M22;
                    return new SKSizeI((int) (actualWidth * scaleX), (int) (actualHeight * scaleY));
                }
            }

            unscaledSize = SKSizeI.Empty;
            scaleX = scaleY = 1d;
            return SKSizeI.Empty;
        }

        private static bool IsPositive(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value > 0.0;
    }
}