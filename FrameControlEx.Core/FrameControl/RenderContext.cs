using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameControlEx.Core.FrameControl.Models;
using FrameControlEx.Core.FrameControl.Models.Scene;
using FrameControlEx.Core.FrameControl.Models.Scene.Sources.Base;
using FrameControlEx.Core.FrameControl.ViewModels;
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

        public void RenderScene(SceneModel scene) {
            if (scene.ClearScreenOnRender) {
                this.Canvas.Clear(scene.BackgroundColour);
            }

            IEnumerable<SourceModel> items = scene.SourceDeck.Sources;
            if (scene.IsRenderOrderReversed) {
                items = items.Reverse();
            }

            foreach (SourceModel source in items) {
                if (!source.IsEnabled) {
                    continue;
                }

                // TODO: Maybe create separate rendering classes for each type of source
                if (source is IVisualSource visual) {
                    visual.OnTickVisual();
                    this.Canvas.Save();
                    visual.OnRender(this);
                    this.Canvas.Restore();
                }
            }
        }

        public async Task RenderSceneAsync(SceneModel scene) {
            if (scene.ClearScreenOnRender) {
                this.Canvas.Clear(scene.BackgroundColour);
            }

            IEnumerable<SourceModel> items = scene.SourceDeck.Sources;
            if (scene.IsRenderOrderReversed) {
                items = items.Reverse();
            }

            List<SourceModel> list = items.ToList();
            await Task.Run(() => {
                foreach (SourceModel source in list) {
                    if (!source.IsEnabled) {
                        continue;
                    }

                    // TODO: Maybe create separate rendering classes for each type of source
                    if (source is IVisualSource visual) {
                        visual.OnTickVisual();
                        this.Canvas.Save();
                        visual.OnRender(this);
                        this.Canvas.Restore();
                    }
                }
            });
        }
    }
}