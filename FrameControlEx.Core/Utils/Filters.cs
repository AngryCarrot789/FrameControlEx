using FrameControlEx.Core.Views.Dialogs.FilePicking;

namespace FrameControlEx.Core.Utils {
    public static class Filters {
        public static readonly string ImageTypesAndAll =
            Filter.Of().
                   Add("PNG File", "png").
                   Add("JPEG", "jpg", "jpeg").
                   Add("Bitmap", "bmp").
                   AddAllFiles().
                   ToString();
    }
}