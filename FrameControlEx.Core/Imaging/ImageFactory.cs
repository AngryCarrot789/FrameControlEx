using System.Dynamic;
using System.Threading.Tasks;

namespace FrameControlEx.Core.Imaging {
    /// <summary>
    /// A class for creating and managing images in a cross-platform manner
    /// </summary>
    public abstract class ImageFactory {
        public static ImageFactory Factory { get; set; }

        public abstract Task<IImage> CreateImageAsync(string path);

        public void DrawImage() {

        }
    }
}