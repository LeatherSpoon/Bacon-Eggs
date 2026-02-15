namespace BaconEggs;

public sealed class Player
{
    private const float XVelocity = 100f;
    private const float YVelocity = 100f;

    private float _elapsedTime;
    private int _currentFrame;
    private Sprite _currentSprite;

    public Player(float x, float y, float size)
    {
        X = x;
        Y = y;
        Width = size;
        Height = size;
        VelocityX = 0;
        VelocityY = 0;
        Sprites = new SpriteSet();
        _currentSprite = Sprites.Idle;
        Facing = "down";
        ImagePath = "./images/player.png";
        IsImageLoaded = true;
    }

    public float X { get; private set; }
    public float Y { get; private set; }
    public float Width { get; }
    public float Height { get; }
    public float VelocityX { get; private set; }
    public float VelocityY { get; private set; }
    public bool IsInvincible { get; set; }
    public bool IsImageLoaded { get; }
    public string ImagePath { get; }
    public string Facing { get; private set; }
    public SpriteSet Sprites { get; }

    public (float X, float Y) Center => (X + Width / 2f, Y + Height / 2f);

    public void Draw(IRenderContext context)
    {
        if (!IsImageLoaded)
        {
            return;
        }

        float xScale = 1;
        float drawX = X;
        const float cropOffset = 0.5f;

        if (Facing == "left")
        {
            xScale = -1;
            drawX = -X - Width;
        }

        context.Save();
        context.GlobalAlpha = IsInvincible ? 0.5f : 1f;
        context.Scale(xScale, 1);
        context.DrawImage(
            ImagePath,
            _currentSprite.X,
            _currentSprite.Y + _currentSprite.Height * _currentFrame + cropOffset,
            _currentSprite.Width,
            _currentSprite.Height,
            drawX,
            Y,
            Width,
            Height);
        context.Restore();
    }

    public void Update(float deltaTime, IReadOnlyList<CollisionBlock> collisionBlocks)
    {
        if (deltaTime <= 0)
        {
            return;
        }

        _elapsedTime += deltaTime;
        const float secondsInterval = 0.1f;
        if (_elapsedTime > secondsInterval)
        {
            _currentFrame = (_currentFrame + 1) % _currentSprite.Frames;
            _elapsedTime -= secondsInterval;
        }

        X += VelocityX * deltaTime;
        CheckForHorizontalCollisions(collisionBlocks);

        Y += VelocityY * deltaTime;
        CheckForVerticalCollisions(collisionBlocks);

        SwitchSprites();
    }

    public void HandleInput(InputState keys)
    {
        VelocityX = 0;
        VelocityY = 0;

        if (keys.D.Pressed)
        {
            VelocityX = XVelocity;
            Facing = "right";
        }
        else if (keys.A.Pressed)
        {
            VelocityX = -XVelocity;
            Facing = "left";
        }
        else if (keys.W.Pressed)
        {
            VelocityY = -YVelocity;
            Facing = "up";
        }
        else if (keys.S.Pressed)
        {
            VelocityY = YVelocity;
            Facing = "down";
        }
    }

    private void SwitchSprites()
    {
        if (VelocityX == 0 && VelocityY == 0)
        {
            _currentFrame = 0;
            _currentSprite = Sprites.Idle;
        }
        else if (VelocityX > 0 && _currentSprite != Sprites.WalkRight)
        {
            _currentFrame = 0;
            _currentSprite = Sprites.WalkRight;
        }
        else if (VelocityX < 0 && _currentSprite != Sprites.WalkLeft)
        {
            _currentFrame = 0;
            _currentSprite = Sprites.WalkLeft;
        }
        else if (VelocityY > 0 && _currentSprite != Sprites.WalkDown)
        {
            _currentFrame = 0;
            _currentSprite = Sprites.WalkDown;
        }
        else if (VelocityY < 0 && _currentSprite != Sprites.WalkUp)
        {
            _currentFrame = 0;
            _currentSprite = Sprites.WalkUp;
        }
    }

    private void CheckForHorizontalCollisions(IReadOnlyList<CollisionBlock> collisionBlocks)
    {
        const float buffer = 0.0001f;
        foreach (var block in collisionBlocks)
        {
            if (!Intersects(block))
            {
                continue;
            }

            if (VelocityX < 0)
            {
                X = block.X + block.Width + buffer;
                break;
            }

            if (VelocityX > 0)
            {
                X = block.X - Width - buffer;
                break;
            }
        }
    }

    private void CheckForVerticalCollisions(IReadOnlyList<CollisionBlock> collisionBlocks)
    {
        const float buffer = 0.0001f;
        foreach (var block in collisionBlocks)
        {
            if (!Intersects(block))
            {
                continue;
            }

            if (VelocityY < 0)
            {
                VelocityY = 0;
                Y = block.Y + block.Height + buffer;
                break;
            }

            if (VelocityY > 0)
            {
                VelocityY = 0;
                Y = block.Y - Height - buffer;
                break;
            }
        }
    }

    private bool Intersects(CollisionBlock block) =>
        X <= block.X + block.Width &&
        X + Width >= block.X &&
        Y + Height >= block.Y &&
        Y <= block.Y + block.Height;
}
