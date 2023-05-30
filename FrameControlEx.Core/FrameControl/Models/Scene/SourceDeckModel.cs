using System.Collections.Generic;
using FrameControlEx.Core.FrameControl.Scene.Sources;

namespace FrameControlEx.Core.FrameControl.Models.Scene.Sources {
    public class SourceDeckModel {
        public SceneModel Scene { get; }

        public List<SourceModel> Sources { get; }

        public event AVDeckInvalidatedEventHandler OnVisualInvalidated;

        public SourceDeckModel(SceneModel scene) {
            this.Scene = scene;
            this.Sources = new List<SourceModel>();
        }

        public void InvalidateVisual() {
            this.OnVisualInvalidated?.Invoke();
        }
    }
}