using System;
using System.Collections.Generic;
using FrameControlEx.Core.FrameControl.Models.Scene.Sources.Base;
using FrameControlEx.Core.RBC;

namespace FrameControlEx.Core.FrameControl.Models.Scene {
    public class SourceDeckModel : IRBESerialisable {
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

        public void WriteToRBE(RBEDictionary data) {
            RBEList list = data.GetOrCreateListElement(nameof(this.Sources));
            foreach (SourceModel source in this.Sources) {
                RBEDictionary dictionary = new RBEDictionary();
                list.Add(dictionary);
                source.WriteToRBE(dictionary);
            }
        }

        public void ReadFromRBE(RBEDictionary data) {
            List<RBEBase> list = data.GetList(nameof(this.Sources));
            foreach (RBEBase element in list) {
                if (!(element is RBEDictionary dictionary))
                    throw new Exception($"Expected dictionary for source element, got {element?.Type}");

            }
        }
    }
}