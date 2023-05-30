using System;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Numerics;
using FrameControlEx.Core.FrameControl.Models.Scene.Sources.Base;
using FrameControlEx.Core.Utils;
using Microsoft.Win32.SafeHandles;
using SkiaSharp;

namespace FrameControlEx.Core.FrameControl.Models.Scene.Sources {
    /// <summary>
    /// A memory-mapped frame source. This reads the current frame from a memory-mapped file. Memory mapped files are
    /// typically accessible across processes, and this class does not care as long as you provide a name
    /// </summary>
    public class MMFSourceModel : AVSourceModel {
        public delegate void MMFFaultStateEventHandler(MMFSourceModel source, bool isFaulted);

        private MemoryMappedFile file;
        private Vector2 lastFrameSize;

        public string MapName { get; set; }

        private bool isAutoConnectionFault;
        public bool IsAutoConnectionFault {
            get => this.isAutoConnectionFault;
            set {
                this.isAutoConnectionFault = value;
                this.OnFault?.Invoke(this, value);
            }
        }

        public event MMFFaultStateEventHandler OnFault;

        public MMFSourceModel() {

        }

        public override Vector2 GetSize() {
            return this.lastFrameSize;
        }

        private MemoryMappedFile TryOpenFile(string name) {
            MemoryMappedFile mmf;
            try {
                mmf = MemoryMappedFile.OpenExisting(name, MemoryMappedFileRights.Read);
            }
            catch (Exception e) {
                Debug.WriteLine("Failed to create memory mapped file: " + e.GetToString());
                return null;
            }

            return this.file = mmf;
        }

        public void InvalidateFile() {
            try {
                this.file?.Dispose();
            }
            catch (Exception e) {
                Debug.WriteLine("Failed to release the current file's resources: " + e.GetToString());
            }
            finally {
                this.file = null;
            }

            this.IsAutoConnectionFault = false;
        }

        protected override void DisposeCore(ExceptionStack e) {
            base.DisposeCore(e);
            if (this.file != null) {
                try {
                    this.file.Dispose();
                }
                catch (Exception ex) {
                    e.Push(ex);
                }
            }
        }

        public override void OnRender(RenderContext context) {
            base.OnRender(context);
            if (string.IsNullOrWhiteSpace(this.MapName) || this.IsAutoConnectionFault) {
                return;
            }

            MemoryMappedFile mmf = this.TryOpenFile(this.MapName);
            if (mmf == null) {
                this.IsAutoConnectionFault = true;
                return;
            }

            unsafe {
                MemMapFileHeader header;
                using (MemoryMappedViewAccessor thing = mmf.CreateViewAccessor(0, sizeof(MemMapFileHeader), MemoryMappedFileAccess.Read)) {
                    thing.Read(0, out header);
                }

                if (!header.isValid) {
                    return;
                }

                long bytes = header.width * header.height * header.bpp;
                using (MemoryMappedViewAccessor thing = mmf.CreateViewAccessor(sizeof(MemMapFileHeader), bytes, MemoryMappedFileAccess.Read)) {
                    SafeMemoryMappedViewHandle safe = thing.SafeMemoryMappedViewHandle;

                    byte* src = null;
                    safe.AcquirePointer(ref src);
                    try {
                        // src + sizeof(MEMMAPFILE_HEADER) | this is required otherwise weird stuff happens
                        // remove sizeof(MEMMAPFILE_HEADER) to have a weird stuff!
                        SKImage image = SKImage.FromPixels(context.FrameInfo, (IntPtr) src + sizeof(MemMapFileHeader));
                        this.lastFrameSize = new Vector2(image.Width, image.Height);

                        Vector2 scale = this.Scale, pos = this.Pos, origin = this.ScaleOrigin;
                        SKMatrix matrix = context.Canvas.TotalMatrix;
                        context.Canvas.Translate(pos.X, pos.Y);
                        context.Canvas.Scale(scale.X, scale.Y, image.Width * origin.X, image.Height * origin.Y);
                        context.Canvas.DrawImage(image, 0, 0);
                        context.Canvas.SetMatrix(matrix);
                        context.Canvas.Flush();
                        image.Dispose();
                    }
                    finally {
                        safe.ReleasePointer();
                    }
                }
            }
        }
    }
}