using System.Collections.Generic;
using FrameControlEx.Core.FrameControl.Scene;
using FrameControlEx.Core.FrameControl.Scene.Sources;
using SkiaSharp;

namespace FrameControlEx.Core.FrameControl {
    /// <summary>
    /// Contains information about a specific render phase
    /// </summary>
    public class RenderContext {
        /// <summary>
        /// The frame control instance being rendered
        /// </summary>
        public FrameControlViewModel FrameControl { get; }

        /// <summary>
        /// The target render surface
        /// </summary>
        public SKSurface Surface { get; }

        /// <summary>
        /// The surface's canvas
        /// </summary>
        public SKCanvas Canvas { get; }

        /// <summary>
        /// The image info about the surface
        /// </summary>
        public SKImageInfo FrameInfo { get; }

        public RenderContext(FrameControlViewModel frameControl, SKSurface surface, SKCanvas canvas, SKImageInfo rawFrameInfo) {
            this.FrameControl = frameControl;
            this.Surface = surface;
            this.Canvas = canvas;
            this.FrameInfo = rawFrameInfo;
        }

        public void RenderScene(SceneViewModel scene) {
            if (scene.ClearScreenOnRender) {
                this.Canvas.Clear(scene.BackgroundColour);
            }

            IEnumerable<SourceViewModel> items = scene.IsRenderOrderReversed ? scene.SourceDeck.ReverseEnumerable() : scene.SourceDeck.Items;
            foreach (SourceViewModel source in items) {
                if (!source.IsEnabled) {
                    continue;
                }

                // TODO: Maybe create separate rendering classes for each type of source
                if (source is AVSourceViewModel av) {
                    av.OnTickVisual();
                    this.Canvas.Save();
                    av.OnRender(this);
                    this.Canvas.Restore();
                }
            }
        }
    }
}