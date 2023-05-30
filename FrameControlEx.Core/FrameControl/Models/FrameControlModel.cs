using System;
using FrameControlEx.Core.FrameControl.Models.Scene;
using FrameControlEx.Core.Utils;

namespace FrameControlEx.Core.FrameControl.Models {
    public class FrameControlModel {
        public SceneDeckModel SceneDeck { get; }
        public OutputDeckModel OutputDeck { get; }

        public Action RenderCallback {
            get => this.Timer.TickCallback;
            set => this.Timer.TickCallback = value;
        }

        public bool UsePrecisionTimingMode { get; set; }

        public PrecisionTimer Timer { get; }

        public FrameControlModel() {
            this.SceneDeck = new SceneDeckModel(this);
            this.OutputDeck = new OutputDeckModel(this);
            this.Timer = new PrecisionTimer();
        }
    }
}