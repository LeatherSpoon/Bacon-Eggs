namespace BaconEggs
{
    /// <summary>
    /// Represents an offscreen render target that can be drawn to and then converted to a texture.
    /// Mirrors the offscreen canvas created in renderStaticLayers() in index.js.
    /// </summary>
    public interface IOffscreenSurface
    {
        /// <summary>The render context for drawing onto this surface.</summary>
        IRenderContext Context { get; }

        /// <summary>Flattens the surface into an <see cref="ITexture"/> that can be drawn to the main context.</summary>
        ITexture GetTexture();
    }

    /// <summary>
    /// Factory for creating offscreen render surfaces.
    /// Implement this in your target framework (e.g. wrap a MonoGame RenderTarget2D).
    /// </summary>
    public interface IOffscreenRenderer
    {
        /// <summary>Creates a new offscreen surface with the given dimensions.</summary>
        IOffscreenSurface CreateOffscreen(int width, int height);
    }
}
