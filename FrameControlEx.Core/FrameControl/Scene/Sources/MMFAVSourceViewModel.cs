using System;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Threading.Tasks;
using FrameControlEx.Core.Notifications;
using FrameControlEx.Core.Utils;
using FrameControlEx.Core.Views.Dialogs.UserInputs;
using Microsoft.Win32.SafeHandles;
using SkiaSharp;

namespace FrameControlEx.Core.FrameControl.Scene.Sources {
    /// <summary>
    /// A memory-mapped frame source view model. This reads the current frame from a memory-mapped file. Memory mapped files are
    /// typically accessible across processes, and this class does not care as long as you provide a name
    /// </summary>
    public class MMFAVSourceViewModel : AVSourceViewModel {
        private MemoryMappedFile file;
        private byte[] pixels;

        private string mapName;
        public string MapName {
            get => this.mapName;
            private set {
                this.RaisePropertyChanged(ref this.mapName, value);
                this.InvalidateFile();
            }
        }

        private bool isAutoConnectFaulted;
        public bool IsAutoConnectFaulted {
            get => this.isAutoConnectFaulted;
            set => this.RaisePropertyChanged(ref this.isAutoConnectFaulted, value);
        }

        public AsyncRelayCommand EditMappedFileNameCommand { get; }

        public MMFAVSourceViewModel() {
            this.EditMappedFileNameCommand = new AsyncRelayCommand(this.EditMappedFileNameAction);
        }

        // public override void OnAcceptFrame(SKSurface surface, in SKImageInfo frameInfo) {
        //     base.OnAcceptFrame(surface, frameInfo);
        //     MemoryMappedFile mappedFile = this.GetFile(in frameInfo);
        //     if (mappedFile == null) {
        //         return;
        //     }
//
        //     this.lastInfo = frameInfo;
//
        //     using (MemoryMappedViewAccessor view = mappedFile.CreateViewAccessor(0, this.currentLength)) {
        //         unsafe {
        //             MEMMAPFILE_HEADER header = new MEMMAPFILE_HEADER() {
        //                 width = frameInfo.Width, height = frameInfo.Height, bpp = (byte) frameInfo.BytesPerPixel
        //             };
//
        //             view.Write(0, ref header);
        //             SKPixmap pixmap = new SKPixmap();
        //             if (surface.PeekPixels(pixmap)) {
        //                 SafeMemoryMappedViewHandle safe = view.SafeMemoryMappedViewHandle;
        //                 byte* ptr = null;
        //                 safe.AcquirePointer(ref ptr);
        //                 try {
        //                     Buffer.MemoryCopy((void*) pixmap.GetPixels(), ptr + sizeof(MEMMAPFILE_HEADER), this.currentLength, this.currentLength);
        //                 }
        //                 finally {
        //                     safe.ReleasePointer();
        //                 }
        //             }
//
        //             view.Flush();
        //         }
        //     }
//
        //     // write frame to file
        // }

        public override void OnRender(RenderContext context) {
            base.OnRender(context);
            if (string.IsNullOrWhiteSpace(this.MapName) || this.IsAutoConnectFaulted) {
                return;
            }

            MemoryMappedFile mmf = this.AttemptOpenMMF(this.MapName);
            if (mmf == null) {
                this.IsAutoConnectFaulted = true;
                return;
            }

            unsafe {
                MEMMAPFILE_HEADER header;
                using (MemoryMappedViewAccessor thing = mmf.CreateViewAccessor(0, sizeof(MEMMAPFILE_HEADER), MemoryMappedFileAccess.Read)) {
                    thing.Read(0, out header);
                }

                if (!header.isValid) {
                    return;
                }

                long bytes = header.width * header.height * header.bpp;
                using (MemoryMappedViewAccessor thing = mmf.CreateViewAccessor(sizeof(MEMMAPFILE_HEADER), bytes, MemoryMappedFileAccess.Read)) {
                    if (this.pixels == null || this.pixels.Length != bytes) {
                        this.pixels = new byte[bytes];
                    }

                    fixed (byte* dest = this.pixels) {
                        SafeMemoryMappedViewHandle safe = thing.SafeMemoryMappedViewHandle;

                        byte* src = null;
                        safe.AcquirePointer(ref src);
                        try {
                            // src + sizeof(MEMMAPFILE_HEADER) | this is required otherwise weird stuff happens
                            // remove sizeof(MEMMAPFILE_HEADER) to have a weird stuff!
                            SKImage image = SKImage.FromPixels(context.FrameInfo, (IntPtr) src + sizeof(MEMMAPFILE_HEADER));
                            System.Numerics.Vector2 scale = this.Scale, pos = this.Pos, origin = this.ScaleOrigin;
                            SKMatrix matrix = context.Canvas.TotalMatrix;
                            context.Canvas.Translate(pos.X, pos.Y);
                            context.Canvas.Scale(scale.X, scale.Y, image.Width * origin.X, image.Height * origin.Y);
                            context.Canvas.DrawImage(image, 0, 0);
                            context.Canvas.SetMatrix(matrix);
                            context.Canvas.Flush();
                            image.Dispose();
                            // Buffer.MemoryCopy(src + sizeof(MEMMAPFILE_HEADER), dest, bytes, bytes);
                        }
                        finally {
                            safe.ReleasePointer();
                        }

                    }
                    // SafeMemoryMappedViewHandle safe = thing.SafeMemoryMappedViewHandle;
                }
            }
        }

        private MemoryMappedFile AttemptOpenMMF(string name) {
            MemoryMappedFile mmf = null;
            try {
                mmf = MemoryMappedFile.OpenExisting(name, MemoryMappedFileRights.Read);
            }
            catch (Exception e) {
                IFrameControlView view = this.FrameControl?.View;
                if (view != null) {
                    view.PushNotification(new NotificationViewModel() {
                        Header = "Failed to open memory mapped file",
                        Message = e.GetToString()
                    });
                }
                else {
                    Debug.WriteLine("Failed to create memory mapped file: " + e.GetToString());
                }

                return null;
            }

            this.file = mmf;
            return mmf;
        }

        public async Task EditMappedFileNameAction() {
            string result = await IoC.UserInput.ShowSingleInputDialogAsync("New File Name", "Input a new file name for the memory-mapped file. This will invalidate the old one", this.MapName ?? "my_mapped_file", Validators.ForNonEmptyString("File name cannot be an empty string"));
            if (result == null) {
                return;
            }

            this.MapName = result;
            this.InvalidateFile();
        }

        private void InvalidateFile() {
            try {
                this.file?.Dispose();
            }
            catch (Exception e) {
                IFrameControlView view = this.FrameControl?.View;
                if (view != null) {
                    view.PushNotification(new NotificationViewModel() {
                        Header = "File disposal error",
                        Message = "Failed to release the current file's resources: " + e.GetToString()
                    });
                }
                else {
                    Debug.WriteLine("Failed to release the current file's resources: " + e.GetToString());
                }
            }
            finally {
                this.file = null;
            }

            this.IsAutoConnectFaulted = false;
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

        protected override BaseIOViewModel CreateInstanceCore() {
            return new MMFAVSourceViewModel();
        }

        protected override void LoadThisIntoUserCopy(BaseIOViewModel vm) {
            base.LoadThisIntoUserCopy(vm);
            if (vm is MMFAVSourceViewModel output) {
                output.mapName = this.mapName;
            }
        }
    }
}