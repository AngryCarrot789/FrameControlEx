using System.Collections.Generic;
using FrameControlEx.Core.Actions.Contexts;
using FrameControlEx.Core.AdvancedContextService;

namespace FrameControlEx.Core.FrameControl.Scene {
    public class SceneContextMenuGenerator : IContextGenerator {
        public static SceneContextMenuGenerator Instance { get; } = new SceneContextMenuGenerator();

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (context.TryGetContext(out SceneViewModel scene)) {
                list.Add(new CommandContextEntry("Rename Scene", scene.RenameCommand));
                if (scene.Deck != null) {
                    list.Add(SeparatorEntry.Instance);
                    list.Add(new CommandContextEntry("Add New Scene", scene.Deck.AddCommand));
                    list.Add(new CommandContextEntry("Remove Scene", scene.RemoveCommand));
                }
            }
            else if (context.TryGetContext(out SceneDeckViewModel deck)) {
                list.Add(new CommandContextEntry("Add Scene", deck.AddCommand));
            }
        }
    }
}