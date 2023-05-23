using System.Numerics;

namespace FrameControlEx.Core.FrameControl.Scene.Templates {
    public class SourceTemplateViewModel : BaseViewModel {
        private Vector2 pos;
        public Vector2 Pos {
            get => this.pos;
            set => this.RaisePropertyChanged(ref this.pos, value);
        }
    }
}