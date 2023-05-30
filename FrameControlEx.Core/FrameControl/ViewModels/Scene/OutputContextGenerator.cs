using System.Collections.Generic;
using FrameControlEx.Core.Actions.Contexts;
using FrameControlEx.Core.AdvancedContextService;

namespace FrameControlEx.Core.FrameControl.ViewModels.Scene {
    public class OutputContextGenerator : IContextGenerator {
        public static OutputContextGenerator Instance { get; } = new OutputContextGenerator();

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (context.TryGetContext(out OutputViewModel output)) {
                list.Add(new CommandContextEntry("Rename", output.RenameCommand));
                list.Add(SeparatorEntry.Instance);
                if (output.IsEnabled) {
                    list.Add(new CommandContextEntry("Disable", output.DisableCommand));
                }
                else {
                    list.Add(new CommandContextEntry("Enable", output.EnableCommand));
                }

                if (output.Deck != null) {
                    list.Add(SeparatorEntry.Instance);
                    list.Add(new CommandContextEntry("Remove", output.RemoveCommand));
                }

                if (context.TryGetContext(out OutputDeckViewModel deck)) {
                    list.Add(SeparatorEntry.Instance);
                    this.AddNewItemsContext(list, deck);
                }
                else if (output.Deck != null) {
                    list.Add(SeparatorEntry.Instance);
                    this.AddNewItemsContext(list, output.Deck);
                }
            }
            else if (context.TryGetContext(out OutputDeckViewModel deck)) {
                this.AddNewItemsContext(list, deck);
            }
        }

        public void AddNewItemsContext(List<IContextEntry> list, OutputDeckViewModel deck) {
            List<IContextEntry> children = new List<IContextEntry>();
            GroupContextEntry group = new GroupContextEntry("Add", "Shows all of the possible items to add", children);
            list.Add(group);
            list = children;

            // list.Add(new CommandContextEntry("FFmpeg/Recorder Output", deck.AddFFmpegOutputCommand));
            list.Add(new CommandContextEntry("MemMapFile Output", deck.AddMMFCommand));
            list.Add(SeparatorEntry.Instance);
            list.Add(new CommandContextEntry("Buffered Output (for loopback)", deck.AddBufferedOutputCommand));
        }
    }
}