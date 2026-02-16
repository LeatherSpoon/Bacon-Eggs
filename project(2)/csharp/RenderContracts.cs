namespace BaconEggs;

public interface IRenderContext
{
    float GlobalAlpha { get; set; }
    void Save();
    void Restore();
    void Scale(float xScale, float yScale);
    void DrawImage(string imagePath, float srcX, float srcY, float srcWidth, float srcHeight, float destX, float destY, float destWidth, float destHeight);
    void FillRect(float x, float y, float width, float height, string fillStyle);
    void ClearRect(float x, float y, float width, float height);
    void Translate(float x, float y);
}

public sealed record Sprite(float X, float Y, float Width, float Height, int Frames);

public sealed class SpriteSet
{
    public Sprite Idle { get; init; } = new(0, 0, 16, 16, 1);
    public Sprite WalkDown { get; init; } = new(0, 0, 16, 16, 4);
    public Sprite WalkUp { get; init; } = new(16, 0, 16, 16, 4);
    public Sprite WalkLeft { get; init; } = new(32, 0, 16, 16, 4);
    public Sprite WalkRight { get; init; } = new(48, 0, 16, 16, 4);
}

public sealed class KeyState
{
    public bool Pressed { get; set; }
}

public sealed class InputState
{
    public KeyState W { get; } = new();
    public KeyState A { get; } = new();
    public KeyState S { get; } = new();
    public KeyState D { get; } = new();
}
