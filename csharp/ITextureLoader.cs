using System.Threading.Tasks;

namespace BaconEggs
{
    /// <summary>
    /// Abstracts asynchronous texture/image loading.
    /// Mirrors the loadImage() utility from utils.js.
    /// </summary>
    public interface ITextureLoader
    {
        /// <summary>
        /// Loads a texture from the given path and returns it asynchronously.
        /// </summary>
        /// <param name="path">Path to the image file (e.g. "images/terrain.png").</param>
        Task<ITexture> LoadImageAsync(string path);
    }
}
