using System.Collections.Generic;

namespace FrameControlEx.Core.FrameControl.ViewModels.Scene {
    public class SceneDeckModel {
        public List<SceneModel> Scenes { get; }

        public SceneDeckModel() {
            this.Scenes = new List<SceneModel>();
        }

        public SceneModel CreateScene(string name) {
            SceneModel scene = new SceneModel(this) {
                ReadableName = name
            };

            this.Scenes.Add(scene);
            return scene;
        }

        public bool RemoveScene(SceneModel scene) {
            return this.Scenes.Remove(scene);
        }
    }
}