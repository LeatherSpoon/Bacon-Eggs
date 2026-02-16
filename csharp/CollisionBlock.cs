namespace BaconEggs
{
    /// <summary>
    /// Represents a solid collision block in the tile map.
    /// Converted from CollisionBlock.js.
    /// </summary>
    public class CollisionBlock
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        /// <param name="x">World x position in pixels.</param>
        /// <param name="y">World y position in pixels.</param>
        /// <param name="size">Width and height of the block in pixels.</param>
        public CollisionBlock(float x, float y, float size)
        {
            X = x;
            Y = y;
            Width = size;
            Height = size;
        }

        /// <summary>
        /// Optional: draws this collision block for debugging purposes (semi-transparent red rectangle).
        /// </summary>
        public void Draw(IRenderContext context)
        {
            context.FillRect(X, Y, Width, Height, "rgba(255, 0, 0, 0.5)");
        }
    }
}
