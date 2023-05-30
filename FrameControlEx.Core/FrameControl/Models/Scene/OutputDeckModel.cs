using System.Collections.Generic;

namespace FrameControlEx.Core.FrameControl.Models.Scene.Outputs {
    public class OutputDeckModel {
        public OutputModel Scene { get; }

        public List<OutputModel> Outputs { get; }

        public OutputDeckModel(OutputModel scene) {
            this.Scene = scene;
            this.Outputs = new List<OutputModel>();
        }
    }
}