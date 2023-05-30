using System.Numerics;
using FrameControlEx.Core.FrameControl.Models.Scene.Outputs;
using FrameControlEx.Core.FrameControl.Models.Scene.Sources.Base;
using SkiaSharp;

namespace FrameControlEx.Core.FrameControl.Models.Scene.Sources {
    /// <summary>
    /// A source that pulls its video data from a <see cref="BufferedOutputModel"/>
    /// </summary>
    public class LoopbackSourceModel : AVSourceModel {
        public BufferedOutputModel TargetOutput { get; set; }

        public LoopbackSourceModel() {

        }

        public override Vector2 GetSize() {
            SKImage frame = this.TargetOutput?.lastFrame;
            if (frame == null) {
                return Vector2.Zero;
            }

            return new Vector2(frame.Width, frame.Height);
        }

        public override void OnRender(RenderContext context) {
            base.OnRender(context);
            if (this.TargetOutput != null && this.TargetOutput.IsEnabled & this.TargetOutput.lastFrame != null) {
                Vector2 scale = this.Scale, pos = this.Pos, origin = this.ScaleOrigin;
                context.Canvas.Translate(pos.X, pos.Y);
                SKImage frame = this.TargetOutput.lastFrame;
                context.Canvas.Scale(scale.X, scale.Y, frame.Width * origin.X, frame.Height * origin.Y);
                context.Canvas.DrawImage(frame, 0, 0);
            }
        }
    }
}