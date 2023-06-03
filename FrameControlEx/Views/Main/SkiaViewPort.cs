using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FrameControlEx.Core.Utils;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Rect = System.Windows.Rect;

namespace FrameControlEx.Views.Main {
    public class SkiaViewPort : FrameworkElement {
        public static readonly DependencyProperty ViewPortSizeProperty =
            DependencyProperty.Register(
                "ViewPortSize",
                typeof(Vec2i),
                typeof(SkiaViewPort),
                new FrameworkPropertyMetadata(
                    default(Vec2i),
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) => ((SkiaViewPort) d).OnViewPortSizeChanged((Vec2i) e.OldValue, (Vec2i) e.NewValue),
                    (d, e) => ((SkiaViewPort) d).OnViewPortSizeCoerced((Vec2i) e)));

        private readonly bool designMode;
        private readonly FrameBuffer frameA;
        private readonly FrameBuffer frameB;
        private readonly CASLock stateExchangeLock;

        private volatile bool isTargetRenderA;
        private volatile bool isTargetDrawA;

        private SKImageInfo frameInfo;

        private const int FRAME_A = 1;
        private const int FRAME_B = 2;

        private volatile int isInvalidatingVisual;

        public Vec2i ViewPortSize {
            get => (Vec2i) this.GetValue(ViewPortSizeProperty);
            set => this.SetValue(ViewPortSizeProperty, value);
        }

        /// <summary>Occurs when the the canvas needs to be redrawn.</summary>
        /// <remarks />
        [Category("Appearance")]
        public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

        public SkiaViewPort() {
            this.designMode = DesignerProperties.GetIsInDesignMode(this);
            this.stateExchangeLock = new CASLock("StateExchange");
            this.frameInfo = new SKImageInfo(0, 0, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            this.frameA = new FrameBuffer("BufferA");
            this.frameB = new FrameBuffer("BufferB");
            this.isTargetRenderA = true;
        }

        private void OnViewPortSizeChanged(Vec2i oldSize, Vec2i newSize) {
            if (oldSize.Equals(newSize)) {
                return;
            }
        }

        private object OnViewPortSizeCoerced(Vec2i size) {
            if (size.X < 0 || size.Y < 0) {
                size = new Vec2i(Math.Max(0, size.X), Math.Max(0, size.Y));
            }

            this.frameInfo = new SKImageInfo(size.X, size.Y, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            if (this.frameA.Lock(false)) {
                try {
                    this.frameA.UpdateBitmap(this.frameInfo);
                }
                finally {
                    this.frameA.Unlock();
                }
            }

            if (this.frameB.Lock(false)) {
                try {
                    this.frameB.UpdateBitmap(this.frameInfo);
                }
                finally {
                    this.frameB.Unlock();
                }
            }

            return size;
        }

        /// <param name="dc">The drawing instructions for a specific element. This context is provided to the layout system.</param>
        /// <summary>When overridden in a derived class, participates in rendering operations that are directed by the layout system. The rendering instructions for this element are not used directly when this method is invoked, and are instead preserved for later asynchronous use by layout and drawing.</summary>
        /// <remarks />
        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            this.isInvalidatingVisual = 0;
            if (this.designMode || this.Visibility != Visibility.Visible || PresentationSource.FromVisual(this) == null) {
                return;
            }

            SKImageInfo info = this.frameInfo;
            if (info.Width <= 0 || info.Height <= 0) {
                return;
            }

            FrameBuffer frame = null;
            try {
                if (this.isTargetRenderA) {
                    if (this.frameA.Lock(false)) {
                        frame = this.frameA;
                    }
                }
                else {
                    if (this.frameB.Lock(false)) {
                        frame = this.frameB;
                    }
                }
            }
            finally {
            }

            if (frame == null) {
                return;
            }

            try {
                WriteableBitmap bitmap = frame.Bitmap;
                bitmap.Lock();
                bitmap.AddDirtyRect(new Int32Rect(0, 0, info.Width, info.Height));
                bitmap.Unlock();
                dc.DrawImage(bitmap, new Rect(0.0, 0.0, this.ActualWidth, this.ActualHeight));
            }
            finally {
                frame.Unlock();
            }
        }

        public void DrawToNextBitmapAsync() {
            FrameBuffer frame = null;
            try {
                if (this.isTargetDrawA) {
                    if (this.isTargetRenderA) {
                        if (Interlocked.CompareExchange(ref this.isInvalidatingVisual, 1, 0) == 0)
                            this.Dispatcher.InvokeAsync(this.InvalidateVisual);
                        return;
                    }

                    if (this.frameA.Lock(false)) {
                        this.isTargetRenderA = false;
                        this.isTargetDrawA = false;
                        frame = this.frameA;
                    }
                }
                else {
                    if (!this.isTargetRenderA) {
                        if (Interlocked.CompareExchange(ref this.isInvalidatingVisual, 1, 0) == 0)
                            this.Dispatcher.InvokeAsync(this.InvalidateVisual);
                        return;
                    }

                    if (this.frameB.Lock(false)) {
                        this.isTargetRenderA = true;
                        this.isTargetDrawA = true;
                        frame = this.frameB;
                    }
                }
            }
            finally {
            }

            if (frame != null) {
                try {
                    if (!frame.GetBackBuffer(out IntPtr buffer)) {
                        return;
                    }

                    using (SKSurface surface = SKSurface.Create(this.frameInfo, buffer, this.frameInfo.RowBytes)) {
                        if (surface == null) {
                            return;
                        }

                        this.OnPaintSurface(new SKPaintSurfaceEventArgs(surface, this.frameInfo, this.frameInfo));
                    }
                }
                finally {
                    this.isTargetRenderA = !this.isTargetRenderA;
                    frame.Unlock();
                }
            }

            this.Dispatcher.Invoke(this.InvalidateVisual);
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