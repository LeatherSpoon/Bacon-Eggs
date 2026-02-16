namespace BaconEggs
{
    /// <summary>
    /// Abstracts canvas/graphics drawing operations.
    /// Implement this interface with your target framework (MonoGame, SkiaSharp, etc.).
    /// Mirrors the HTML5 CanvasRenderingContext2D API used in the original JavaScript.
    /// </summary>
    public interface IRenderContext
    {
        /// <summary>Saves the current drawing state onto a stack.</summary>
        void Save();

        /// <summary>Restores the most recently saved drawing state.</summary>
        void Restore();

        /// <summary>Sets the global alpha (transparency) for subsequent drawing operations. Range: 0.0 to 1.0.</summary>
        void SetGlobalAlpha(float alpha);

        /// <summary>Applies a scaling transformation to the canvas (use negative x to flip horizontally).</summary>
        void Scale(float x, float y);

        /// <summary>Applies a translation transformation to the canvas.</summary>
        void Translate(float x, float y);

        /// <summary>Clears the specified rectangle area.</summary>
        void ClearRect(float x, float y, float width, float height);

        /// <summary>Fills a rectangle with the given RGBA color string (e.g. "rgba(255,0,0,0.5)").</summary>
        void FillRect(float x, float y, float width, float height, string color);

        /// <summary>
        /// Draws a portion of a texture onto the canvas.
        /// Mirrors CanvasRenderingContext2D.drawImage(image, sx, sy, sw, sh, dx, dy, dw, dh).
        /// </summary>
        void DrawImage(ITexture texture, float srcX, float srcY, float srcWidth, float srcHeight,
                       float dstX, float dstY, float dstWidth, float dstHeight);

        /// <summary>Draws a pre-rendered texture (e.g. an offscreen canvas) at the given position.</summary>
        void DrawTexture(ITexture texture, float x, float y);
    }
}
