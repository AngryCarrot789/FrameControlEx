using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Scene.Outputs;
using FrameControlEx.Core.Views.Dialogs.Message;

namespace FrameControlEx.Core.FrameControl.Scene {
    /// <summary>
    /// A view model for storing all outputs for a specific scene
    /// </summary>
    public class OutputDeckViewModel : BaseListDeckViewModel<OutputViewModel> {
        public static readonly MessageDialog ConfirmRemoveDialog;

        public FrameControlViewModel FrameControl { get; }

        // not sure how else to bypass dialogs :/
        public bool InternalBypassDialog { get; set; }

        public RelayCommand AddBufferedOutputCommand { get; }
        public RelayCommand AddMMFCommand { get; }
        public RelayCommand AddFFmpegOutputCommand { get; }

        public OutputDeckViewModel(FrameControlViewModel frameControl) {
            this.FrameControl = frameControl;
            this.AddBufferedOutputCommand = new RelayCommand(() => {
                BasicBufferOutputViewModel source = new BasicBufferOutputViewModel() {
                    ReadableName = $"Buffer Output {this.Items.Count(x => x is BasicBufferOutputViewModel) + 1}"
                };

                this.Add(source);
            });
            this.AddMMFCommand = new RelayCommand(() => {
                MMFAVOutputViewModel source = new MMFAVOutputViewModel() {
                    ReadableName = $"MMF Output {this.Items.Count(x => x is MMFAVOutputViewModel) + 1}"
                };

                this.Add(source);
            });
            this.AddFFmpegOutputCommand = new RelayCommand(() => {
                FFmpegOutputViewModel source = new FFmpegOutputViewModel() {
                    ReadableName = $"FFmpeg Output {this.Items.Count(x => x is FFmpegOutputViewModel) + 1}"
                };

                this.Add(source);
            });
        }

        static OutputDeckViewModel() {
            ConfirmRemoveDialog = Dialogs.YesCancelDialog.Clone();
            ConfirmRemoveDialog.ShowAlwaysUseNextResultOption = true;
        }

        public override async Task AddActionAsync() {
            await IoC.MessageDialogs.ShowMessageAsync("Coming soon", "This feature is coming soon!");
        }

        public override async Task RemoveItemsAction(IList<OutputViewModel> list) {
            if (!this.InternalBypassDialog) {
                string result = await ConfirmRemoveDialog.ShowAsync("Remove inputs/sources?", $"Are you sure you want to remove {(list.Count == 1 && !string.IsNullOrEmpty(list[0].ReadableName) ? list[0].ReadableName : list.Count.ToString())}?");
                if (result != "yes") {
                    return;
                }
            }

            await base.RemoveItemsAction(list);
        }

        protected override void EnsureItem(OutputViewModel item, bool valid) {
            base.EnsureItem(item, valid);
            item.Deck = valid ? this : null;
        }
    }
}