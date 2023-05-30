using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Models.Scene;
using FrameControlEx.Core.FrameControl.Models.Scene.Outputs;
using FrameControlEx.Core.FrameControl.Models.Scene.Outputs.Base;
using FrameControlEx.Core.FrameControl.Scene.Outputs;
using FrameControlEx.Core.Views.Dialogs.Message;

namespace FrameControlEx.Core.FrameControl.Scene {
    /// <summary>
    /// A view model for storing all outputs for a specific scene
    /// </summary>
    public class OutputDeckViewModel : BaseListDeckViewModel<OutputViewModel> {
        public static readonly MessageDialog ConfirmRemoveDialog;
        private readonly bool isModelLoaded;

        public FrameControlViewModel FrameControl { get; }

        // not sure how else to bypass dialogs :/
        public bool InternalBypassDialog { get; set; }

        public RelayCommand AddBufferedOutputCommand { get; }
        public RelayCommand AddMMFCommand { get; }
        // public RelayCommand AddFFmpegOutputCommand { get; }

        public OutputDeckModel Model { get; }

        public OutputDeckViewModel(FrameControlViewModel frameControl) {
            this.FrameControl = frameControl;
            this.Model = frameControl.Model.OutputDeck;
            this.AddBufferedOutputCommand = new RelayCommand(() => {
                BufferedOutputViewModel source = new BufferedOutputViewModel(new BufferedOutputModel()) {
                    ReadableName = $"Buffer Output {this.Items.Count(x => x is BufferedOutputViewModel) + 1}"
                };

                this.Add(source);
            });
            this.AddMMFCommand = new RelayCommand(() => {
                MMFAVOutputViewModel source = new MMFAVOutputViewModel(new MMFOutputModel()) {
                    ReadableName = $"MMF Output {this.Items.Count(x => x is MMFAVOutputViewModel) + 1}"
                };

                this.Add(source);
            });
            // this.AddFFmpegOutputCommand = new RelayCommand(() => {
            //     FFmpegOutputViewModel source = new FFmpegOutputViewModel() {
            //         ReadableName = $"FFmpeg Output {this.Items.Count(x => x is FFmpegOutputViewModel) + 1}"
            //     };
            //     this.Add(source);
            // });

            foreach (OutputModel output in this.Model.Outputs) {
                this.Add((OutputViewModel) IORegistry.CreateViewModelFromModel(output));
            }

            this.isModelLoaded = true;
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

        protected override void OnItemAdded(OutputViewModel item) {
            if (this.isModelLoaded)
                this.Model.Outputs.Add(item.Model);
            item.Deck = this;
        }

        protected override void OnRemovingItem(OutputViewModel item) {
            this.Model.Outputs.Remove(item.Model);
            item.Deck = null;
        }

        protected override void OnClearing() {
            foreach (OutputViewModel output in this.Items)
                output.Deck = null;
            this.Model.Outputs.Clear();
        }
    }
}