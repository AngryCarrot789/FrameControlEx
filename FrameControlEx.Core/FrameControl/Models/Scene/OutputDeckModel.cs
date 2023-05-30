using System.Collections.Generic;
using FrameControlEx.Core.FrameControl.Models.Scene.Outputs.Base;

namespace FrameControlEx.Core.FrameControl.Models.Scene {
    public class OutputDeckModel {
        public List<OutputModel> Outputs { get; }

        public FrameControlModel FrameControl { get; }

        public OutputDeckModel(FrameControlModel frameControl) {
            this.FrameControl = frameControl;
            this.Outputs = new List<OutputModel>();
        }
    }
}