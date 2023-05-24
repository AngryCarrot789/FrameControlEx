using System.Collections.ObjectModel;
using FrameControlEx.Core.Utils;

namespace FrameControlEx.Core.FrameControl.Scene {
    public class SourceFolderViewModel : SourceViewModel {
        private readonly ObservableCollectionEx<SourceViewModel> items;
        public ReadOnlyObservableCollection<SourceViewModel> Items { get; }

        public SourceFolderViewModel() {
            this.items = new ObservableCollectionEx<SourceViewModel>();
        }

        protected override BaseIOViewModel CreateInstanceCore() {
            return new SourceFolderViewModel();
        }

        protected override void LoadThisIntoUserCopy(BaseIOViewModel vm) {
            base.LoadThisIntoUserCopy(vm);
        }
    }
}