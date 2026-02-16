using System.Collections.Generic;

namespace BaconEggs
{
    /// <summary>
    /// Holds sprite sheet clip information for one animation state.
    /// </summary>
    public class SpriteInfo
    {
        /// <summary>X offset into the sprite sheet in pixels.</summary>
        public int SheetX { get; set; }
        /// <summary>Y offset into the sprite sheet in pixels.</summary>
        public int SheetY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        /// <summary>Total number of animation frames for this state.</summary>
        public int Frames { get; set; }
    }

    /// <summary>
    /// Player direction, used to determine the active sprite animation.
    /// </summary>
    public enum FacingDirection
    {
        Down,
        Up,
        Left,
        Right
    }

    /// <summary>
    /// The player entity: handles input, movement, collision, and sprite animation.
    /// Converted from Player.js.
    /// </summary>
    public class Player
    {
        // Movement speed constants (pixels per second) – from Player.js
        public const float X_VELOCITY = 100f;
        public const float Y_VELOCITY = 100f;

        // Position and dimensions
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        // Velocity vector
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }

        // Invincibility flag – when true the player is drawn at 50% opacity
        public bool IsInvincible { get; set; }

        // Texture loaded by the caller via ITextureLoader
        public ITexture Image { get; set; }

        // Animation state
        private float _elapsedTime;
        private int _currentFrame;
        private SpriteInfo _currentSprite;
        private FacingDirection _facing;

        // Sprite sheet clips
        public SpriteInfo SpriteIdle { get; }
        public SpriteInfo SpriteWalkDown { get; }
        public SpriteInfo SpriteWalkUp { get; }
        public SpriteInfo SpriteWalkLeft { get; }
        public SpriteInfo SpriteWalkRight { get; }

        /// <summary>
        /// Creates a new Player at the given world position.
        /// </summary>
        /// <param name="x">Initial world x (pixels).</param>
        /// <param name="y">Initial world y (pixels).</param>
        /// <param name="size">Width and height of the player hitbox (pixels).</param>
        public Player(float x, float y, float size)
        {
            X = x;
            Y = y;
            Width = size;
            Height = size;

            VelocityX = 0f;
            VelocityY = 0f;
            _elapsedTime = 0f;
            _currentFrame = 0;
            _facing = FacingDirection.Down;

            // Sprite sheet layout matches player.png (16×16 frames)
            SpriteIdle      = new SpriteInfo { SheetX =  0, SheetY = 0, Width = 16, Height = 16, Frames = 1 };
            SpriteWalkDown  = new SpriteInfo { SheetX =  0, SheetY = 0, Width = 16, Height = 16, Frames = 4 };
            SpriteWalkUp    = new SpriteInfo { SheetX = 16, SheetY = 0, Width = 16, Height = 16, Frames = 4 };
            SpriteWalkLeft  = new SpriteInfo { SheetX = 32, SheetY = 0, Width = 16, Height = 16, Frames = 4 };
            SpriteWalkRight = new SpriteInfo { SheetX = 48, SheetY = 0, Width = 16, Height = 16, Frames = 4 };

            _currentSprite = SpriteIdle;
        }

        /// <summary>
        /// Draws the player sprite. Must be called inside a Save()/Restore() pair if the caller
        /// applies its own transforms.
        /// </summary>
        public void Draw(IRenderContext context)
        {
            if (Image == null) return;

            const float cropOffset = 0.5f;

            float xScale = 1f;
            float drawX = X;

            if (_facing == FacingDirection.Left)
            {
                xScale = -1f;
                drawX = -X - Width;
            }

            context.Save();
            context.SetGlobalAlpha(IsInvincible ? 0.5f : 1f);
            context.Scale(xScale, 1f);

            float srcX = _currentSprite.SheetX;
            float srcY = _currentSprite.SheetY + _currentSprite.Height * _currentFrame + cropOffset;

            context.DrawImage(
                Image,
                srcX, srcY, _currentSprite.Width, _currentSprite.Height,
                drawX, Y, Width, Height);

            context.Restore();
        }

        /// <summary>
        /// Updates animation frame counter, position, and collision resolution.
        /// Call once per game-loop tick.
        /// </summary>
        /// <param name="deltaTime">Elapsed time since last frame in seconds.</param>
        /// <param name="collisionBlocks">All solid collision blocks in the scene.</param>
        public void Update(float deltaTime, IList<CollisionBlock> collisionBlocks)
        {
            if (deltaTime <= 0f) return;

            // Advance animation
            _elapsedTime += deltaTime;
            const float secondsInterval = 0.1f;
            if (_elapsedTime > secondsInterval)
            {
                _currentFrame = (_currentFrame + 1) % _currentSprite.Frames;
                _elapsedTime -= secondsInterval;
            }

            // Move horizontally then resolve collisions
            UpdateHorizontalPosition(deltaTime);
            CheckForHorizontalCollisions(collisionBlocks);

            // Move vertically then resolve collisions
            UpdateVerticalPosition(deltaTime);
            CheckForVerticalCollisions(collisionBlocks);

            SwitchSprites();
        }

        /// <summary>
        /// Reads the current key state and updates the player's velocity vector.
        /// Only one direction is active at a time (matches original JS priority: D > A > W > S).
        /// </summary>
        public void HandleInput(InputManager input)
        {
            VelocityX = 0f;
            VelocityY = 0f;

            if (input.IsKeyPressed(GameKey.D))
            {
                VelocityX = X_VELOCITY;
            }
            else if (input.IsKeyPressed(GameKey.A))
            {
                VelocityX = -X_VELOCITY;
            }
            else if (input.IsKeyPressed(GameKey.W))
            {
                VelocityY = -Y_VELOCITY;
            }
            else if (input.IsKeyPressed(GameKey.S))
            {
                VelocityY = Y_VELOCITY;
            }
        }

        // -----------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------

        private void UpdateHorizontalPosition(float deltaTime)
        {
            X += VelocityX * deltaTime;
        }

        private void UpdateVerticalPosition(float deltaTime)
        {
            Y += VelocityY * deltaTime;
        }

        private void CheckForHorizontalCollisions(IList<CollisionBlock> collisionBlocks)
        {
            const float buffer = 0.0001f;
            foreach (var block in collisionBlocks)
            {
                if (X     <= block.X + block.Width  &&
                    X + Width  >= block.X            &&
                    Y + Height >= block.Y            &&
                    Y          <= block.Y + block.Height)
                {
                    if (VelocityX < 0f)
                    {
                        X = block.X + block.Width + buffer;
                        break;
                    }
                    if (VelocityX > 0f)
                    {
                        X = block.X - Width - buffer;
                        break;
                    }
                }
            }
        }

        private void CheckForVerticalCollisions(IList<CollisionBlock> collisionBlocks)
        {
            const float buffer = 0.0001f;
            foreach (var block in collisionBlocks)
            {
                if (X     <= block.X + block.Width  &&
                    X + Width  >= block.X            &&
                    Y + Height >= block.Y            &&
                    Y          <= block.Y + block.Height)
                {
                    if (VelocityY < 0f)
                    {
                        VelocityY = 0f;
                        Y = block.Y + block.Height + buffer;
                        break;
                    }
                    if (VelocityY > 0f)
                    {
                        VelocityY = 0f;
                        Y = block.Y - Height - buffer;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Switches the active sprite based on the current velocity and updates the facing direction.
        /// </summary>
        private void SwitchSprites()
        {
            if (VelocityX == 0f && VelocityY == 0f)
            {
                _currentFrame = 0;
                _currentSprite.Frames = 1;
            }
            else if (VelocityX > 0f && _currentSprite != SpriteWalkRight)
            {
                _currentFrame = 0;
                _currentSprite = SpriteWalkRight;
                _currentSprite.Frames = 4;
                _facing = FacingDirection.Right;
            }
            else if (VelocityX < 0f && _currentSprite != SpriteWalkLeft)
            {
                _currentFrame = 0;
                _currentSprite = SpriteWalkLeft;
                _currentSprite.Frames = 4;
                _facing = FacingDirection.Left;
            }
            else if (VelocityY > 0f && _currentSprite != SpriteWalkDown)
            {
                _currentFrame = 0;
                _currentSprite = SpriteWalkDown;
                _currentSprite.Frames = 4;
                _facing = FacingDirection.Down;
            }
            else if (VelocityY < 0f && _currentSprite != SpriteWalkUp)
            {
                _currentFrame = 0;
                _currentSprite = SpriteWalkUp;
                _currentSprite.Frames = 4;
                _facing = FacingDirection.Up;
            }
        }
    }
}
