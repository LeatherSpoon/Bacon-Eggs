using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaconEggs.Data;

namespace BaconEggs
{
    /// <summary>
    /// Tileset metadata: which image to load and what tile size to use.
    /// </summary>
    public class TilesetInfo
    {
        public string ImagePath { get; set; }
        public int TileSize { get; set; }
    }

    /// <summary>
    /// Main game engine. Manages constants, the player, the camera, collision blocks,
    /// and per-frame update/render logic.
    ///
    /// Converted from index.js.
    ///
    /// Usage:
    ///   1. Implement <see cref="IRenderContext"/> and <see cref="ITextureLoader"/> for your framework.
    ///   2. Create a GameEngine with your canvas size and device pixel ratio.
    ///   3. Call InitializeAsync() once to load textures and build the background.
    ///   4. In your game loop, call Update(deltaTime) then Render().
    ///   5. Forward input events to InputManager.
    /// </summary>
    public class GameEngine
    {
        // ------------------------------------------------------------------
        // Map & viewport constants (mirrors index.js)
        // ------------------------------------------------------------------
        public const int MAP_COLS = 43;
        public const int MAP_ROWS = 38;
        public const int MAP_WIDTH  = MAP_COLS * 16;  // 688 px
        public const int MAP_HEIGHT = MAP_ROWS * 16;  // 608 px
        public const int BLOCK_SIZE = 16;

        // ------------------------------------------------------------------
        // Runtime state
        // ------------------------------------------------------------------
        public InputManager Input { get; } = new InputManager();

        private readonly int _canvasWidth;
        private readonly int _canvasHeight;
        private readonly float _sceneScale;
        private readonly float _viewportWidth;
        private readonly float _viewportHeight;
        private readonly float _sceneCenterX;
        private readonly float _sceneCenterY;
        private const float SCENE_OFFSET_X = 0f;
        private const float SCENE_OFFSET_Y = 0f;

        private readonly List<CollisionBlock> _collisionBlocks = new List<CollisionBlock>();
        private Player _player;

        private float _cameraX = SCENE_OFFSET_X;
        private float _cameraY = SCENE_OFFSET_Y;

        // Background texture built once from all tile layers
        private ITexture _backgroundTexture;

        // Dependencies injected by the host application
        private readonly IRenderContext _context;
        private readonly ITextureLoader _textureLoader;
        private readonly IOffscreenRenderer _offscreenRenderer;

        // ------------------------------------------------------------------
        // Layer definitions (mirrors layersData / tilesets in index.js)
        // ------------------------------------------------------------------
        private static readonly (string layerKey, int[][] data, string imagePath)[] LayerDefs =
        {
            ("l_Terrain",                  TilemapData.L_Terrain,                 "images/terrain.png"),
            ("l_Front_Renders",            TilemapData.L_Front_Renders,           "images/decorations.png"),
            ("l_Trees_1",                  TilemapData.L_Trees_1,                 "images/decorations.png"),
            ("l_Trees_2",                  TilemapData.L_Trees_2,                 "images/decorations.png"),
            ("l_Trees_3",                  TilemapData.L_Trees_3,                 "images/decorations.png"),
            ("l_Trees_4",                  TilemapData.L_Trees_4,                 "images/decorations.png"),
            ("l_Landscape_Decorations",    TilemapData.L_Landscape_Decorations,   "images/decorations.png"),
            ("l_Landscape_Decorations_2",  TilemapData.L_Landscape_Decorations2,  "images/decorations.png"),
            ("l_Houses",                   TilemapData.L_Houses,                  "images/decorations.png"),
            ("l_House_Decorations",        TilemapData.L_House_Decorations,       "images/decorations.png"),
            ("l_New_Layer_13",             TilemapData.L_New_Layer_13,            "images/decorations.png"),
        };

        /// <summary>
        /// Creates the game engine.
        /// </summary>
        /// <param name="canvasWidth">Logical canvas width in pixels (e.g. 1024 * dpr).</param>
        /// <param name="canvasHeight">Logical canvas height in pixels (e.g. 576 * dpr).</param>
        /// <param name="devicePixelRatio">Device pixel ratio for the display.</param>
        /// <param name="context">Render context implementation.</param>
        /// <param name="textureLoader">Texture loader implementation.</param>
        /// <param name="offscreenRenderer">Used to bake all tile layers into a single background texture.</param>
        public GameEngine(
            int canvasWidth,
            int canvasHeight,
            float devicePixelRatio,
            IRenderContext context,
            ITextureLoader textureLoader,
            IOffscreenRenderer offscreenRenderer)
        {
            _canvasWidth  = canvasWidth;
            _canvasHeight = canvasHeight;
            _context      = context;
            _textureLoader = textureLoader;
            _offscreenRenderer = offscreenRenderer;

            _sceneScale     = 2f + devicePixelRatio;
            _viewportWidth  = canvasWidth  / _sceneScale;
            _viewportHeight = canvasHeight / _sceneScale;
            _sceneCenterX   = _viewportWidth  / 2f + SCENE_OFFSET_X;
            _sceneCenterY   = _viewportHeight / 2f + SCENE_OFFSET_Y;
        }

        // ------------------------------------------------------------------
        // Initialization
        // ------------------------------------------------------------------

        /// <summary>
        /// Loads textures, builds collision blocks, renders the static background, and creates the player.
        /// Must be awaited before the first call to Update()/Render().
        /// </summary>
        public async Task InitializeAsync()
        {
            BuildCollisionBlocks();
            _backgroundTexture = await RenderStaticLayersAsync();
            _player = new Player(_sceneCenterX, _sceneCenterY, size: 15f)
            {
                Image = await _textureLoader.LoadImageAsync("images/player.png")
            };
        }

        // ------------------------------------------------------------------
        // Game loop
        // ------------------------------------------------------------------

        /// <summary>
        /// Updates player input, movement, and camera position.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since the last frame, in seconds.</param>
        public void Update(float deltaTime)
        {
            _player.HandleInput(Input);
            _player.Update(deltaTime, _collisionBlocks);
            UpdateCamera();
        }

        /// <summary>
        /// Draws the background and the player to the render context.
        /// </summary>
        public void Render()
        {
            _context.Save();
            _context.Scale(_sceneScale, _sceneScale);
            _context.Translate(-_cameraX, -_cameraY);
            _context.ClearRect(0, 0, _canvasWidth, _canvasHeight);

            if (_backgroundTexture != null)
                _context.DrawTexture(_backgroundTexture, 0, 0);

            _player?.Draw(_context);

            _context.Restore();
        }

        // ------------------------------------------------------------------
        // Private helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// Parses the collisions data array and instantiates a CollisionBlock for every cell with value 1.
        /// </summary>
        private void BuildCollisionBlocks()
        {
            int[][] collisions = TilemapData.Collisions;
            for (int row = 0; row < collisions.Length; row++)
            {
                for (int col = 0; col < collisions[row].Length; col++)
                {
                    if (collisions[row][col] == 1)
                    {
                        _collisionBlocks.Add(new CollisionBlock(
                            x: col * BLOCK_SIZE,
                            y: row * BLOCK_SIZE,
                            size: BLOCK_SIZE));
                    }
                }
            }
        }

        /// <summary>
        /// Iterates all tile layers, loads their tilesets, and composites them onto an offscreen texture.
        /// Mirrors renderStaticLayers() in index.js.
        /// </summary>
        private async Task<ITexture> RenderStaticLayersAsync()
        {
            var offscreen = _offscreenRenderer.CreateOffscreen(_canvasWidth, _canvasHeight);
            var offCtx = offscreen.Context;

            // Cache loaded textures so we don't re-load the same image multiple times
            var textureCache = new Dictionary<string, ITexture>();

            foreach (var (_, data, imagePath) in LayerDefs)
            {
                if (!textureCache.TryGetValue(imagePath, out ITexture tilesetImage))
                {
                    try
                    {
                        tilesetImage = await _textureLoader.LoadImageAsync(imagePath);
                        textureCache[imagePath] = tilesetImage;
                    }
                    catch (Exception)
                    {
                        // Skip layers whose tileset failed to load
                        continue;
                    }
                }

                RenderLayer(data, tilesetImage, tileSize: 16, offCtx);
            }

            return offscreen.GetTexture();
        }

        /// <summary>
        /// Renders a single tile layer onto the given context.
        /// Mirrors renderLayer() in index.js.
        /// </summary>
        private static void RenderLayer(int[][] tilesData, ITexture tilesetImage, int tileSize, IRenderContext context)
        {
            int tilesPerRow = (int)Math.Ceiling((double)tilesetImage.Width / tileSize);

            for (int row = 0; row < tilesData.Length; row++)
            {
                for (int col = 0; col < tilesData[row].Length; col++)
                {
                    int symbol = tilesData[row][col];
                    if (symbol == 0) continue;

                    int tileIndex = symbol - 1;
                    float srcX = (tileIndex % tilesPerRow) * tileSize;
                    float srcY = (float)Math.Floor((double)tileIndex / tilesPerRow) * tileSize;

                    context.DrawImage(
                        tilesetImage,
                        srcX, srcY, tileSize, tileSize,
                        col * 16f, row * 16f, 16f, 16f);
                }
            }
        }

        /// <summary>
        /// Recalculates camera position so it follows the player and clamps to map bounds.
        /// Mirrors the camera update inside animate() in index.js.
        /// </summary>
        private void UpdateCamera()
        {
            float playerCenterX = _player.X + _player.Width  / 2f;
            float playerCenterY = _player.Y + _player.Height / 2f;

            float maxCameraX = MAP_WIDTH  - _viewportWidth;
            float maxCameraY = MAP_HEIGHT - _viewportHeight;

            float horizontalScroll = Math.Max(0f, playerCenterX - _sceneCenterX);
            _cameraX = Math.Min(SCENE_OFFSET_X + horizontalScroll, maxCameraX);

            float verticalScroll = playerCenterY - _sceneCenterY;
            _cameraY = Math.Min(Math.Max(0f, SCENE_OFFSET_Y + verticalScroll), maxCameraY);
        }
    }
}
