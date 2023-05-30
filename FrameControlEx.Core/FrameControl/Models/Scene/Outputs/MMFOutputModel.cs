using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using FrameControlEx.Core.FrameControl.Models.Scene.Outputs.Base;
using FrameControlEx.Core.RBC;
using FrameControlEx.Core.Utils;
using Microsoft.Win32.SafeHandles;
using SkiaSharp;

namespace FrameControlEx.Core.FrameControl.Models.Scene.Outputs {
    public class MMFOutputModel : AVOutputModel {
        public delegate void MMFFaultStateEventHandler(MMFOutputModel sender, bool isFaulted);

        private MemoryMappedFile file;
        private SKImageInfo lastInfo;
        private long currentLength;

        public string MapName { get; set; }

        public bool GenerateMapName { get; set; }

        private bool isAutoConnectionFault;
        public bool IsAutoConnectionFault {
            get => this.isAutoConnectionFault;
            set {
                this.isAutoConnectionFault = value;
                this.OnFault?.Invoke(this, value);
            }
        }

        public event MMFFaultStateEventHandler OnFault;

        public MMFOutputModel() {

        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.MapName = data.GetString(nameof(this.MapName), null);
            this.GenerateMapName = data.GetBool(nameof(this.GenerateMapName), false);
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetString(nameof(this.MapName), this.MapName);
            data.SetBool(nameof(this.GenerateMapName), this.GenerateMapName);
        }

        public override void OnAcceptFrame(RenderContext context) {
            base.OnAcceptFrame(context);
            SKImageInfo frameInfo = context.FrameInfo;
            MemoryMappedFile mappedFile = this.GetFile(in frameInfo);
            if (mappedFile == null) {
                return;
            }

            this.lastInfo = frameInfo;

            using (MemoryMappedViewAccessor view = mappedFile.CreateViewAccessor(0, this.currentLength)) {
                unsafe {
                    MemMapFileHeader header = new MemMapFileHeader() {
                        isValid = true,
                        width = frameInfo.Width,
                        height = frameInfo.Height,
                        bpp = (byte) frameInfo.BytesPerPixel,
                        time = Time.GetSystemTicks()
                    };

                    view.Write(0, ref header);
                    SKPixmap pixmap = new SKPixmap();
                    if (context.Surface.PeekPixels(pixmap)) {
                        SafeMemoryMappedViewHandle safe = view.SafeMemoryMappedViewHandle;
                        byte* ptr = null;
                        safe.AcquirePointer(ref ptr);
                        try {
                            Buffer.MemoryCopy((void*) pixmap.GetPixels(), ptr + sizeof(MemMapFileHeader), this.currentLength, this.currentLength);
                        }
                        finally {
                            safe.ReleasePointer();
                        }
                    }

                    view.Flush();
                }
            }
        }

        protected MemoryMappedFile GetFile(in SKImageInfo info) {
            if (this.file != null) {
                if (this.lastInfo.Equals(info)) {
                    return this.file;
                }

                try {
                    this.file.Dispose();
                }
                catch (Exception e) {
                    Debug.WriteLine("Exception disposing memory mapped file: " + this.file + "\n" + e.GetToString());
                }
                finally {
                    this.file = null;
                }
            }

            // try generate memory mapped file from file path (or generated one) and image info

            string name;
            if (string.IsNullOrEmpty(this.MapName)) {
                if (this.GenerateMapName) {
                    char[] chars = new char[16];
                    Random random = new Random();
                    for (int i = 0; i < 16; i++) {
                        chars[i] = (char) ((random.Next(2) == 0 ? 'a' : 'A') + random.Next(27));
                    }

                    name = new string(chars);
                }
                else {
                    return null;
                }
            }
            else {
                name = this.MapName;
            }

            return this.GenerateMemoryMappedFile(name, in info);
        }

        private MemoryMappedFile GenerateMemoryMappedFile(string name, in SKImageInfo info) {
            long size = info.Width * info.Height * info.BytesPerPixel;
            if (size < 1) {
                return null;
            }

            unsafe {
                size += sizeof(MemMapFileHeader);
            }

            MemoryMappedFile mmf = null;
            try {
                mmf = MemoryMappedFile.CreateOrOpen(name, size, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.DelayAllocatePages, HandleInheritability.None);
            }
            catch (Exception e) {
                if (mmf != null) {
                    try {
                        mmf.Dispose();
                    }
                    catch (Exception ex) {
                        e.AddSuppressed(ex);
                    }
                }

                Debug.WriteLine("Failed to create memory mapped file: " + e.GetToString());
                this.IsAutoConnectionFault = true;
                return null;
            }

            this.currentLength = size;
            this.file = mmf;
            return mmf;
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

        public void InvalidateFile() {
            if (this.file == null) {
                return;
            }

            try {
                this.file.Dispose();
            }
            finally {
                this.file = null;
            }
        }
    }
}