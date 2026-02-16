using BaconEggs.Data;

namespace BaconEggs;

public sealed class Game
{
    private const int MapCols = 43;
    private const int MapRows = 38;
    private const int TileSize = 16;

    private readonly Dictionary<string, int[][]> _layersData = new()
    {
        ["l_Terrain"] = LTerrainData.l_Terrain,
        ["l_Front_Renders"] = LFrontRendersData.l_Front_Renders,
        ["l_Trees_1"] = LTrees1Data.l_Trees_1,
        ["l_Trees_2"] = LTrees2Data.l_Trees_2,
        ["l_Trees_3"] = LTrees3Data.l_Trees_3,
        ["l_Trees_4"] = LTrees4Data.l_Trees_4,
        ["l_Landscape_Decorations"] = LLandscapeDecorationsData.l_Landscape_Decorations,
        ["l_Landscape_Decorations_2"] = LLandscapeDecorations2Data.l_Landscape_Decorations_2,
        ["l_Houses"] = LHousesData.l_Houses,
        ["l_House_Decorations"] = LHouseDecorationsData.l_House_Decorations,
        ["l_New_Layer_13"] = LNewLayer13Data.l_New_Layer_13,
    };

    private readonly Dictionary<string, (string ImageUrl, int TileSize)> _tilesets = new()
    {
        ["l_Terrain"] = ("./images/terrain.png", 16),
        ["l_Front_Renders"] = ("./images/decorations.png", 16),
        ["l_Trees_1"] = ("./images/decorations.png", 16),
        ["l_Trees_2"] = ("./images/decorations.png", 16),
        ["l_Trees_3"] = ("./images/decorations.png", 16),
        ["l_Trees_4"] = ("./images/decorations.png", 16),
        ["l_Landscape_Decorations"] = ("./images/decorations.png", 16),
        ["l_Landscape_Decorations_2"] = ("./images/decorations.png", 16),
        ["l_Houses"] = ("./images/decorations.png", 16),
        ["l_House_Decorations"] = ("./images/decorations.png", 16),
        ["l_New_Layer_13"] = ("./images/decorations.png", 16),
    };

    private readonly List<CollisionBlock> _collisionBlocks = new();
    private readonly InputState _keys = new();
    private readonly Player _player;

    public Game(float canvasWidth, float canvasHeight, float dpr)
    {
        var mapHeight = MapRows * TileSize;
        var mapWidth = MapCols * TileSize;
        var sceneScale = 2 + dpr;
        var sceneCenterX = canvasWidth / 2 / sceneScale;
        var sceneCenterY = canvasHeight / 2 / sceneScale;

        _player = new Player(sceneCenterX, sceneCenterY, 15);

        for (var y = 0; y < CollisionsData.collisions.Length; y++)
        {
            var row = CollisionsData.collisions[y];
            for (var x = 0; x < row.Length; x++)
            {
                if (row[x] == 1)
                {
                    _collisionBlocks.Add(new CollisionBlock(x * TileSize, y * TileSize, TileSize));
                }
            }
        }

        MapHeight = mapHeight;
        MapWidth = mapWidth;
        SceneScale = sceneScale;
    }

    public float MapHeight { get; }
    public float MapWidth { get; }
    public float SceneScale { get; }
    public InputState Keys => _keys;

    public async Task RenderStaticLayersAsync()
    {
        foreach (var (layerName, _) in _layersData)
        {
            if (_tilesets.TryGetValue(layerName, out var tilesetInfo))
            {
                await ImageLoader.LoadImageAsync(tilesetInfo.ImageUrl);
            }
        }
    }

    public void Animate(float deltaTime)
    {
        _player.HandleInput(_keys);
        _player.Update(deltaTime, _collisionBlocks);
    }
}
