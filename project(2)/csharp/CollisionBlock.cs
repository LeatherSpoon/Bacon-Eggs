namespace BaconEggs;

public sealed class CollisionBlock
{
    public CollisionBlock(float x, float y, float size)
    {
        X = x;
        Y = y;
        Width = size;
        Height = size;
    }

    public float X { get; }
    public float Y { get; }
    public float Width { get; }
    public float Height { get; }

    public void Draw(IRenderContext context)
    {
        context.FillRect(X, Y, Width, Height, "rgba(255, 0, 0, 0.5)");
    }
}
