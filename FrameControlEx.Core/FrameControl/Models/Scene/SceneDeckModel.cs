using System;
using System.Collections.Generic;
using FrameControlEx.Core.RBC;

namespace FrameControlEx.Core.FrameControl.Models.Scene {
    public class SceneDeckModel {
        public List<SceneModel> Scenes { get; }

        public FrameControlModel FrameControl { get; }

        public SceneDeckModel(FrameControlModel frameControl) {
            this.FrameControl = frameControl;
            this.Scenes = new List<SceneModel>();
        }

        public SceneModel CreateScene(string name) {
            SceneModel scene = new SceneModel(this) {
                DisplayName = name
            };

            this.Scenes.Add(scene);
            return scene;
        }

        public bool RemoveScene(SceneModel scene) {
            return this.Scenes.Remove(scene);
        }

        public void LoadLayout(RBEDictionary data) {
            List<RBEBase> list = data.GetList(nameof(this.Scenes));
            List<SceneModel> scenes = new List<SceneModel>();
            for (int i = 0; i < list.Count; i++) {
                if (!(list[i] is RBEDictionary dictionary))
                    throw new Exception($"Expected scene list to contain RBE dictionary; got {list[i]?.GetType()}");
                SceneModel scene = new SceneModel(this);
                try {
                    scene.ReadFromRBE(dictionary);
                }
                catch (Exception e) {
                    throw new Exception($"Failed to read scene {i + 1}/{list.Count}", e);
                }

                scenes.Add(scene);
            }

            this.Scenes.Clear();
            this.Scenes.AddRange(scenes);
        }

        public void SaveLayout(RBEDictionary data) {
            RBEList list = data.GetOrCreateListElement(nameof(this.Scenes));
            for (int i = 0; i < this.Scenes.Count; i++) {
                RBEDictionary dictionary = new RBEDictionary();
                try {
                    this.Scenes[i].WriteToRBE(dictionary);
                }
                catch (Exception e) {
                    throw new Exception($"Failed to write scene {i + 1}/{this.Scenes.Count}", e);
                }

                list.Add(dictionary);
            }
        }
    }
}