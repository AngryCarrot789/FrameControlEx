namespace FrameControlEx.Core.FrameControl.Models.Scene.Sources {
    public abstract class SourceModel : BaseIOModel {
        public SourceDeckModel Deck { get; set; }

        protected SourceModel() {

        }
    }
}