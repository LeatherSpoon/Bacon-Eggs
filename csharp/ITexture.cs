namespace BaconEggs
{
    /// <summary>
    /// Represents a loaded image/texture.
    /// Implement this interface to wrap textures from your target framework.
    /// </summary>
    public interface ITexture
    {
        int Width { get; }
        int Height { get; }
    }
}
