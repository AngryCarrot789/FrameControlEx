using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Scene.Sources;
using FrameControlEx.Core.Utils;
using FrameControlEx.Core.Views.Dialogs.Message;

namespace FrameControlEx.Core.FrameControl.Scene {
    /// <summary>
    /// A view model for storing all sources for a specific scene
    /// </summary>
    public class SourceDeckViewModel : BaseListDeckViewModel<SourceViewModel> {
        public static readonly MessageDialog ConfirmRemoveDialog;

        public SceneViewModel Scene { get; }

        public int CountEnabled => this.Items.Count(x => x.IsEnabled);

        public int CountDisabled => this.Items.Count(x => !x.IsEnabled);

        public RelayCommand AddImageCommand { get; }
        public RelayCommand AddMMFCommand { get; }
        public RelayCommand AddLoopbackInput { get; }

        public RelayCommand EnableAllCommand { get; }
        public RelayCommand DisableAllCommand { get; }
        public RelayCommand ToggleEnabledAllCommand { get; }

        public event VisualDeckInvalidatedEventHandler OnVisualInvalidated;

        public SourceDeckViewModel(SceneViewModel scene) {
            this.Scene = scene;
            this.AddImageCommand = new RelayCommand(() => {
                ImageSourceViewModel source = new ImageSourceViewModel {
                    ReadableName = $"Image {this.Items.Count(x => x is ImageSourceViewModel) + 1}"
                };

                this.Add(source);
                this.InvalidateVisual();
            });
            this.AddMMFCommand = new RelayCommand(() => {
                MMFAVSourceViewModel source = new MMFAVSourceViewModel {
                    ReadableName = $"MMF Source {this.Items.Count(x => x is MMFAVSourceViewModel) + 1}"
                };

                this.Add(source);
                this.InvalidateVisual();
            });
            this.AddLoopbackInput = new RelayCommand(() => {
                LoopbackSourceViewModel source = new LoopbackSourceViewModel {
                    ReadableName = $"SIInput {this.Items.Count(x => x is LoopbackSourceViewModel) + 1}"
                };

                this.Add(source);
                this.InvalidateVisual();
            });
            this.EnableAllCommand = new RelayCommand(() => this.Items.ForEach(x => x.IsEnabled = true), () => this.Items.Any(x => !x.IsEnabled));
            this.DisableAllCommand = new RelayCommand(() => this.Items.ForEach(x => x.IsEnabled = false), () => this.Items.Any(x => x.IsEnabled));
            this.ToggleEnabledAllCommand = new RelayCommand(() => this.Items.ForEach(x => x.IsEnabled = !x.IsEnabled));
        }

        static SourceDeckViewModel() {
            ConfirmRemoveDialog = Dialogs.YesCancelDialog.Clone();
            ConfirmRemoveDialog.ShowAlwaysUseNextResultOption = true;
        }

        public void OnIsEnabledChanged(SourceViewModel source) {
            this.RaisePropertyChanged(nameof(this.CountEnabled));
            this.RaisePropertyChanged(nameof(this.CountDisabled));
            this.EnableAllCommand.RaiseCanExecuteChanged();
            this.DisableAllCommand.RaiseCanExecuteChanged();
        }

        public override async Task AddActionAsync() {
            await IoC.MessageDialogs.ShowMessageAsync("Wot", "Right click the '+' button and select an item to add");
        }

        public override async Task RemoveItemsAction(IList<SourceViewModel> list) {
            string result = await ConfirmRemoveDialog.ShowAsync("Remove inputs/sources?", $"Are you sure you want to remove {(list.Count == 1 && !string.IsNullOrEmpty(list[0].ReadableName) ? list[0].ReadableName : list.Count.ToString())}?");
            if (result == "yes") {
                await base.RemoveItemsAction(list);
                this.InvalidateVisual();
            }
        }

        // was originally going to make rendering event based, but that would just add extra overhead
        // because there may be multiple sources whose frames aren't necessarily event base-able (e.g.
        // capturing desktop; you do that event 60th of a sec or whatever the refresh rate is)

        // so instead, the main window just has a dispatcher callback running as quickly as possible
        // constantly rendering the output frame. It does use a bit of extra GPU... but that's probably alright

        public void InvalidateVisual() {
            this.OnVisualInvalidated?.Invoke();
        }

        protected override void EnsureItem(SourceViewModel item, bool valid) {
            base.EnsureItem(item, valid);
            item.Deck = valid ? this : null;
        }

        protected override void OnItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            base.OnItemCollectionChanged(sender, e);
            this.RaisePropertyChanged(nameof(this.CountEnabled));
            this.RaisePropertyChanged(nameof(this.CountDisabled));
        }
    }
}