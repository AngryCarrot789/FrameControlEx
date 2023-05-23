using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FrameControlEx.Core.Notifications;
using FrameControlEx.Core.Utils;
using FrameControlEx.Core.Views.Dialogs.UserInputs;
using Microsoft.Win32.SafeHandles;
using SkiaSharp;

namespace FrameControlEx.Core.FrameControl.Scene.Outputs {
    /// <summary>
    /// A memory-mapped file based output. This writes the current frame to a memory-mapped file for other processes to read from
    /// </summary>
    public class MMFAVOutputViewModel : AVOutputViewModel {
        private MemoryMappedFile file;
        private SKImageInfo lastInfo;
        private long currentLength;

        private string mapName;
        public string MapName {
            get => this.mapName;
            private set {
                this.RaisePropertyChanged(ref this.mapName, value);
                this.IsAutoGenerationHalted = false;
            }
        }

        private bool generateMapName;
        public bool GenerateMapName {
            get => this.generateMapName;
            set {
                this.RaisePropertyChanged(ref this.generateMapName, value);
                this.IsAutoGenerationHalted = false;
            }
        }

        private bool isAutoGenerationHalted;
        public bool IsAutoGenerationHalted {
            get => this.isAutoGenerationHalted;
            set => this.RaisePropertyChanged(ref this.isAutoGenerationHalted, value);
        }

        public AsyncRelayCommand EditMappedFileNameCommand { get; }

        public MMFAVOutputViewModel() {
            this.EditMappedFileNameCommand = new AsyncRelayCommand(this.EditMappedFileNameAction);
        }

        public override void OnAcceptFrame(SKSurface surface, in SKImageInfo frameInfo) {
            base.OnAcceptFrame(surface, frameInfo);
            MemoryMappedFile mappedFile = this.GetFile(in frameInfo);
            if (mappedFile == null) {
                return;
            }

            this.lastInfo = frameInfo;

            using (MemoryMappedViewAccessor view = mappedFile.CreateViewAccessor(0, this.currentLength)) {
                unsafe {
                    MEMMAPFILE_HEADER header = new MEMMAPFILE_HEADER() {
                        isValid = true, width = frameInfo.Width, height = frameInfo.Height, bpp = (byte) frameInfo.BytesPerPixel
                    };

                    view.Write(0, ref header);
                    SKPixmap pixmap = new SKPixmap();
                    if (surface.PeekPixels(pixmap)) {
                        SafeMemoryMappedViewHandle safe = view.SafeMemoryMappedViewHandle;
                        byte* ptr = null;
                        safe.AcquirePointer(ref ptr);
                        try {
                            Buffer.MemoryCopy((void*) pixmap.GetPixels(), ptr + sizeof(MEMMAPFILE_HEADER), this.currentLength, this.currentLength);
                        }
                        finally {
                            safe.ReleasePointer();
                        }
                    }

                    view.Flush();
                }
            }

            // write frame to file
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

            string name = null;
            // try generate memory mapped file from file path (or generated one) and image info
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
                size += sizeof(MEMMAPFILE_HEADER);
            }

            MemoryMappedFile mmf = null;
            try {
                mmf = MemoryMappedFile.CreateOrOpen(name, size, MemoryMappedFileAccess.ReadWrite);
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

                IFrameControlView view = this.FrameControl?.View;
                if (view != null) {
                    view.PushNotification(new NotificationViewModel() {
                        Header = "Failed to create memory mapped file",
                        Message = e.GetToString()
                    });
                }
                else {
                    Debug.WriteLine("Failed to create memory mapped file: " + e.GetToString());
                }

                this.IsAutoGenerationHalted = true;
                return null;
            }

            this.currentLength = size;
            this.file = mmf;
            return mmf;
        }

        public async Task EditMappedFileNameAction() {
            string result = await IoC.UserInput.ShowSingleInputDialogAsync("New File Name", "Input a new file name for the memory-mapped file. This will invalidate the old one", this.MapName ?? "my_mapped_file", Validators.ForNonEmptyString("File name cannot be an empty string"));
            if (result == null) {
                return;
            }

            this.MapName = result;

            try {
                this.file?.Dispose();
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("File disposal error", "Failed to release the current file's resources", e.GetToString());
            }
            finally {
                this.file = null;
            }
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
            return new MMFAVOutputViewModel();
        }

        protected override void LoadThisIntoCopy(BaseIOViewModel vm) {
            base.LoadThisIntoCopy(vm);
            if (vm is MMFAVOutputViewModel output) {
                output.generateMapName = this.generateMapName;
                output.mapName = this.mapName;
            }
        }
    }
}