using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FrameControlEx.Core.Utils;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Rect = System.Windows.Rect;

namespace FrameControlEx.Views.Main {
    public class SkiaViewPort_OLD : FrameworkElement {
        public static readonly DependencyProperty ViewPortSizeProperty =
            DependencyProperty.Register(
                "ViewPortSize",
                typeof(Vec2i),
                typeof(SkiaViewPort_OLD),
                new FrameworkPropertyMetadata(default(Vec2i), FrameworkPropertyMetadataOptions.AffectsRender));

        private const double BitmapDpi = 96.0;
        private readonly bool designMode;
        private WriteableBitmap bitmapA;
        private WriteableBitmap bitmapB;
        private IntPtr bitmapBufferA;
        private IntPtr bitmapBufferB;
        private volatile bool canDrawIntoA;
        private volatile bool canDrawIntoB;
        private readonly CASLock bmpLockA = new CASLock();
        private readonly CASLock bmpLockB = new CASLock();
        private SKImageInfo frameInfo;

        private volatile int currFreshFrame;
        private volatile int lastFreshFrame;

        private const int FRAME_A = 1;
        private const int FRAME_B = 2;

        public Vec2i ViewPortSize {
            get => (Vec2i) this.GetValue(ViewPortSizeProperty);
            set => this.SetValue(ViewPortSizeProperty, value);
        }

        /// <summary>Occurs when the the canvas needs to be redrawn.</summary>
        /// <remarks />
        [Category("Appearance")]
        public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

        public SkiaViewPort_OLD() {
            this.designMode = DesignerProperties.GetIsInDesignMode(this);
        }

        /// <param name="dc">The drawing instructions for a specific element. This context is provided to the layout system.</param>
        /// <summary>When overridden in a derived class, participates in rendering operations that are directed by the layout system. The rendering instructions for this element are not used directly when this method is invoked, and are instead preserved for later asynchronous use by layout and drawing.</summary>
        /// <remarks />
        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            if (this.designMode || this.Visibility != Visibility.Visible || PresentationSource.FromVisual(this) == null)
                return;
            Vec2i size = this.ViewPortSize;
            if (size.X <= 0 || size.Y <= 0) {
                return;
            }

            if (this.currFreshFrame == FRAME_A && this.bmpLockA.Lock(false)) {
                this.OnRenderBitmapA(dc, in size);
                this.bmpLockA.Unlock();
            }
            else if (this.currFreshFrame == FRAME_B && this.bmpLockB.Lock(false)) {
                this.OnRenderBitmapB(dc, in size);
                this.bmpLockB.Unlock();
            }
            else if (this.lastFreshFrame == FRAME_B && this.bmpLockA.Lock(false)) {
                this.OnRenderBitmapA(dc, in size);
                this.bmpLockA.Unlock();
            }
            else if (this.lastFreshFrame == FRAME_A && this.bmpLockB.Lock(false)) {
                this.OnRenderBitmapB(dc, in size);
                this.bmpLockB.Unlock();
            }
            else if (this.bmpLockA.Lock(false)) {
                this.OnRenderBitmapA(dc, in size);
                this.bmpLockA.Unlock();
            }
            else if (this.bmpLockB.Lock(false)) {
                this.OnRenderBitmapB(dc, in size);
                this.bmpLockB.Unlock();
            }
        }

        protected void OnRenderBitmapA(DrawingContext dc, in Vec2i size) {
            this.canDrawIntoA = false;
            this.RenderBitmapToContext(dc, ref this.bitmapA, ref this.bitmapBufferA, in size);
            this.canDrawIntoA = this.bitmapA != null;
        }

        protected void OnRenderBitmapB(DrawingContext dc, in Vec2i size) {
            this.canDrawIntoB = false;
            this.RenderBitmapToContext(dc, ref this.bitmapB, ref this.bitmapBufferB, in size);
            this.canDrawIntoB = this.bitmapB != null;
        }

        protected void RenderBitmapToContext(DrawingContext dc, ref WriteableBitmap bitmap, ref IntPtr backBuffer, in Vec2i size) {
            WriteableBitmap bmp = bitmap;
            SKImageInfo frame = this.frameInfo;
            if (frame.Width < 1 || frame.Height < 1) {
                this.frameInfo = frame = new SKImageInfo(size.X, size.Y, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            }

            if (bmp == null || frame.Width != bmp.PixelWidth || frame.Height != bmp.PixelHeight) {
                bmp = bitmap = new WriteableBitmap(size.X, size.Y, 96d, 96d, PixelFormats.Pbgra32, null);
                bmp.Lock();
                backBuffer = bmp.BackBuffer;
                using (SKSurface surface = SKSurface.Create(frame, backBuffer, bmp.BackBufferStride)) {
                    this.OnPaintSurface(new SKPaintSurfaceEventArgs(surface, frame, frame));
                }

                bmp.AddDirtyRect(new Int32Rect(0, 0, size.X, size.Y));
                bmp.Unlock();
            }
            else {
                bmp.Lock();
                bmp.AddDirtyRect(new Int32Rect(0, 0, size.X, size.Y));
                bmp.Unlock();
                dc.DrawImage(bmp, new Rect(0.0, 0.0, this.ActualWidth, this.ActualHeight));
            }
        }

        public void DrawToNextBitmapAsync() {
            if (this.currFreshFrame == FRAME_A) {
                if (this.bmpLockB.Lock(false)) {
                    this.DrawToBitmapAsync(ref this.bitmapBufferB, in this.frameInfo);
                    this.bmpLockB.Unlock();
                    this.lastFreshFrame = Interlocked.Exchange(ref this.currFreshFrame, FRAME_B);
                }
                else if (this.bmpLockA.Lock(true)) {
                    this.DrawToBitmapAsync(ref this.bitmapBufferA, in this.frameInfo);
                    this.bmpLockA.Unlock();
                }
                else {
                    return;
                }
            }
            else if (this.currFreshFrame == FRAME_B) {
                if (this.bmpLockA.Lock(false)) {
                    this.DrawToBitmapAsync(ref this.bitmapBufferA, in this.frameInfo);
                    this.bmpLockA.Unlock();
                    this.lastFreshFrame = Interlocked.Exchange(ref this.currFreshFrame, FRAME_A);
                }
                else if (this.bmpLockB.Lock(true)) {
                    this.DrawToBitmapAsync(ref this.bitmapBufferB, in this.frameInfo);
                    this.bmpLockB.Unlock();
                }
                else {
                    return;
                }
            }
            else if (this.bmpLockA.Lock(false)) {
                this.DrawToBitmapAsync(ref this.bitmapBufferA, in this.frameInfo);
                this.lastFreshFrame = this.currFreshFrame;
                this.currFreshFrame = FRAME_B;
                this.bmpLockA.Unlock();
            }
            else if (this.bmpLockB.Lock(true)) {
                this.DrawToBitmapAsync(ref this.bitmapBufferB, in this.frameInfo);
                this.lastFreshFrame = this.currFreshFrame;
                this.currFreshFrame = FRAME_A;
                this.bmpLockB.Unlock();
            }

            this.Dispatcher.Invoke(this.InvalidateVisual);
        }

        public void DrawToBitmapAsync(ref IntPtr bitmap, in SKImageInfo frame) {
            using (SKSurface surface = SKSurface.Create(frame, bitmap, frame.Width * frame.BytesPerPixel)) {
                if (surface == null) {
                    return;
                }

                this.OnPaintSurface(new SKPaintSurfaceEventArgs(surface, frame, frame));
            }
        }

        /// <param name="e">The event arguments that contain the drawing surface and information.</param>
        /// <summary>Implement this to draw on the canvas.</summary>
        /// <remarks />
        protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e) {
            this.PaintSurface?.Invoke(this, e);
        }

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
                if (PresentationSource.FromVisual(this)?.CompositionTarget is CompositionTarget target) {
                    unscaledSize = new SKSizeI((int) actualWidth, (int) actualHeight);
                    Matrix t2d = target.TransformToDevice;
                    scaleX = t2d.M11;
                    scaleY = t2d.M22;
                    return new SKSizeI((int) (actualWidth * scaleX), (int) (actualHeight * scaleY));
                }
            }

            scaleX = scaleY = 1;
            return unscaledSize = SKSizeI.Empty;
        }

        private static bool IsPositive(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value > 0.0;
    }
}