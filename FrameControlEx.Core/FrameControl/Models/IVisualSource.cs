using System.Numerics;

namespace FrameControlEx.Core.FrameControl.Models {
    public interface IVisualSource {
        Vector2 Pos { get; }
        Vector2 Scale { get; }
        Vector2 ScaleOrigin { get; }

        void OnTickVisual();

        void OnRender(RenderContext context);
    }
}