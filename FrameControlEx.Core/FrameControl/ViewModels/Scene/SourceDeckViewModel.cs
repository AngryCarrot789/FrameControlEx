using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Models;
using FrameControlEx.Core.FrameControl.Models.Scene;
using FrameControlEx.Core.FrameControl.Models.Scene.Sources;
using FrameControlEx.Core.FrameControl.Models.Scene.Sources.Base;
using FrameControlEx.Core.FrameControl.ViewModels.Scene.Sources;
using FrameControlEx.Core.Utils;
using FrameControlEx.Core.Views.Dialogs.Message;

namespace FrameControlEx.Core.FrameControl.ViewModels.Scene {
    /// <summary>
    /// A view model for storing all sources for a specific scene
    /// </summary>
    public class SourceDeckViewModel : BaseListDeckViewModel<SourceViewModel> {
        public static readonly MessageDialog ConfirmRemoveDialog;
        private readonly bool isModelLoaded;

        public SourceDeckModel Model { get; }

        public SceneViewModel Scene { get; }

        public int CountOfEnabled => this.Items.Count(x => x.IsEnabled);

        public int CountOfDisabled => this.Items.Count(x => !x.IsEnabled);

        public RelayCommand AddImageCommand { get; }
        public RelayCommand AddMemMapFileCommand { get; }
        public RelayCommand AddLoopbackInputCommand { get; }
        public RelayCommand AddSceneSourceCommand { get; }

        public RelayCommand EnableAllCommand { get; }
        public RelayCommand DisableAllCommand { get; }
        public RelayCommand ToggleEnabledAllCommand { get; }

        public event AVDeckInvalidatedEventHandler OnVisualInvalidated;

        public SourceDeckViewModel(SceneViewModel scene, SourceDeckModel model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model), "Model cannot be null");
            this.Scene = scene;
            this.AddImageCommand = new RelayCommand(() => {
                ImageSourceViewModel source = new ImageSourceViewModel(new ImageSourceModel()) {
                    ReadableName = $"Image {this.Items.Count(x => x is ImageSourceViewModel) + 1}"
                };

                this.Add(source);
                this.InvalidateVisual();
            });
            this.AddMemMapFileCommand = new RelayCommand(() => {
                MMFAVSourceViewModel source = new MMFAVSourceViewModel(new MMFSourceModel()) {
                    ReadableName = $"MMF Source {this.Items.Count(x => x is MMFAVSourceViewModel) + 1}"
                };

                this.Add(source);
                this.InvalidateVisual();
            });
            this.AddLoopbackInputCommand = new RelayCommand(() => {
                LoopbackSourceViewModel source = new LoopbackSourceViewModel(new LoopbackSourceModel()) {
                    ReadableName = $"SIInput {this.Items.Count(x => x is LoopbackSourceViewModel) + 1}"
                };

                this.Add(source);
                this.InvalidateVisual();
            });
            this.AddSceneSourceCommand = new RelayCommand(() => {
                SceneSourceViewModel source = new SceneSourceViewModel(new SceneSourceModel()) {
                    ReadableName = $"Scene Source {this.Items.Count(x => x is SceneSourceViewModel) + 1}"
                };

                this.Add(source);
                this.InvalidateVisual();
            });
            this.EnableAllCommand = new RelayCommand(() => this.Items.ForEach(x => x.IsEnabled = true), () => this.Items.Any(x => !x.IsEnabled));
            this.DisableAllCommand = new RelayCommand(() => this.Items.ForEach(x => x.IsEnabled = false), () => this.Items.Any(x => x.IsEnabled));
            this.ToggleEnabledAllCommand = new RelayCommand(() => this.Items.ForEach(x => x.IsEnabled = !x.IsEnabled));

            foreach (SourceModel source in model.Sources) {
                this.Add((SourceViewModel) IORegistry.CreateViewModelFromModel(source));
            }

            this.isModelLoaded = true;
        }

        static SourceDeckViewModel() {
            ConfirmRemoveDialog = Dialogs.YesCancelDialog.Clone();
            ConfirmRemoveDialog.ShowAlwaysUseNextResultOption = true;
        }

        public void OnIsEnabledChanged(SourceViewModel source) {
            this.RaisePropertyChanged(nameof(this.CountOfEnabled));
            this.RaisePropertyChanged(nameof(this.CountOfDisabled));
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

        public void InvalidateVisual() {
            this.OnVisualInvalidated?.Invoke();
        }

        protected override void OnItemAdded(SourceViewModel item) {
            if (this.isModelLoaded)
                this.Model.Sources.Add(item.Model);
            item.Deck = this;
        }

        protected override void OnRemovingItem(SourceViewModel item) {
            this.Model.Sources.Remove(item.Model);
            item.Deck = null;
        }

        protected override void OnClearing() {
            foreach (SourceViewModel source in this.Items)
                source.Deck = null;
            this.Model.Sources.Clear();
        }

        protected override void OnItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            base.OnItemCollectionChanged(sender, e);
            this.RaisePropertyChanged(nameof(this.CountOfEnabled));
            this.RaisePropertyChanged(nameof(this.CountOfDisabled));
        }
    }
}