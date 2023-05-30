using System.Numerics;
using FrameControlEx.Core.FrameControl.Models.Scene.Sources.Base;

namespace FrameControlEx.Core.FrameControl.Models.Scene.Sources {
    public class SceneSourceModel : AVSourceModel {
        public SceneModel TargetScene { get; set; }

        public override Vector2 GetSize() {
            return new Vector2();
        }

        public override void OnRender(RenderContext context) {
            base.OnRender(context);
            // This barely works... scenes need to be rendered on a separate
            // surface so that it can be property scaled
            // if (this.TargetScene != null) {
            //     context.Canvas.Save();
            //     Rect rect = this.GetFullRectangle();
            //     context.Canvas.ClipRect(SKRect.Create(rect.X1, rect.Y1, rect.Width, rect.Height));
            //     Vector2 s = this.Scale, o = this.ScaleOrigin;
            //     context.Canvas.Scale(s.X, s.Y, rect.Width * o.X, rect.Height * o.Y);
            //     context.Canvas.Translate(rect.X1, rect.Y1);
            //     context.RenderScene(this.TargetScene);
            //     context.Canvas.Restore();
            // }
        }
    }
}