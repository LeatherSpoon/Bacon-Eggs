const canvas = document.querySelector('canvas')
const c = canvas.getContext('2d')
const dpr = window.devicePixelRatio || 1

canvas.width = 1024 * dpr
canvas.height = 576 * dpr

const MAP_COLS = 43
const MAP_ROWS = 38
const MAP_HEIGHT = MAP_ROWS * 16
const MAP_WIDTH = MAP_COLS * 16
const SCENE_SCALE = 2 + dpr // Edit this to zoom in/out
const SCENE_OFFSET_X = 0 // Change to init player at different location
const SCENE_OFFSET_Y = 0 // Change to init player at different location
const SCENE_CENTER_X = canvas.width / 2 / SCENE_SCALE + SCENE_OFFSET_X
const SCENE_CENTER_Y = canvas.height / 2 / SCENE_SCALE + SCENE_OFFSET_Y
const VIEWPORT_WIDTH = canvas.width / SCENE_SCALE
const VIEWPORT_HEIGHT = canvas.height / SCENE_SCALE

const layersData = {
   l_Terrain: l_Terrain,
   l_Front_Renders: l_Front_Renders,
   l_Trees_1: l_Trees_1,
   l_Trees_2: l_Trees_2,
   l_Trees_3: l_Trees_3,
   l_Trees_4: l_Trees_4,
   l_Landscape_Decorations: l_Landscape_Decorations,
   l_Landscape_Decorations_2: l_Landscape_Decorations_2,
   l_Houses: l_Houses,
   l_House_Decorations: l_House_Decorations,
   l_New_Layer_13: l_New_Layer_13,
};

const tilesets = {
  l_Terrain: { imageUrl: './images/terrain.png', tileSize: 16 },
  l_Front_Renders: { imageUrl: './images/decorations.png', tileSize: 16 },
  l_Trees_1: { imageUrl: './images/decorations.png', tileSize: 16 },
  l_Trees_2: { imageUrl: './images/decorations.png', tileSize: 16 },
  l_Trees_3: { imageUrl: './images/decorations.png', tileSize: 16 },
  l_Trees_4: { imageUrl: './images/decorations.png', tileSize: 16 },
  l_Landscape_Decorations: { imageUrl: './images/decorations.png', tileSize: 16 },
  l_Landscape_Decorations_2: { imageUrl: './images/decorations.png', tileSize: 16 },
  l_Houses: { imageUrl: './images/decorations.png', tileSize: 16 },
  l_House_Decorations: { imageUrl: './images/decorations.png', tileSize: 16 },
  l_New_Layer_13: { imageUrl: './images/decorations.png', tileSize: 16 },
};


// Tile setup
const collisionBlocks = []
const blockSize = 16 // Assuming each tile is 16x16 pixels

collisions.forEach((row, y) => {
  row.forEach((symbol, x) => {
    if (symbol === 1) {
      collisionBlocks.push(
        new CollisionBlock({
          x: x * blockSize,
          y: y * blockSize,
          size: blockSize,
        }),
      )
    }
  })
})

const renderLayer = (tilesData, tilesetImage, tileSize, context) => {
  // Calculate the number of tiles per row in the tileset
  // We use Math.ceil to ensure we get a whole number of tiles
  const tilesPerRow = Math.ceil(tilesetImage.width / tileSize)

  tilesData.forEach((row, y) => {
    row.forEach((symbol, x) => {
      if (symbol !== 0) {
        // Adjust index to be 0-based for calculations
        const tileIndex = symbol - 1

        // Calculate source coordinates
        const srcX = (tileIndex % tilesPerRow) * tileSize
        const srcY = Math.floor(tileIndex / tilesPerRow) * tileSize

        context.drawImage(
          tilesetImage, // source image
          srcX,
          srcY, // source x, y
          tileSize,
          tileSize, // source width, height
          x * 16,
          y * 16, // destination x, y
          16,
          16, // destination width, height
        )
      }
    })
  })
}

const renderStaticLayers = async () => {
  const offscreenCanvas = document.createElement('canvas')
  offscreenCanvas.width = canvas.width
  offscreenCanvas.height = canvas.height
  const offscreenContext = offscreenCanvas.getContext('2d')

  for (const [layerName, tilesData] of Object.entries(layersData)) {
    const tilesetInfo = tilesets[layerName]
    if (tilesetInfo) {
      try {
        const tilesetImage = await loadImage(tilesetInfo.imageUrl)
        renderLayer(
          tilesData,
          tilesetImage,
          tilesetInfo.tileSize,
          offscreenContext,
        )
      } catch (error) {
        console.error(`Failed to load image for layer ${layerName}:`, error)
      }
    }
  }

  // Optionally draw collision blocks and platforms for debugging
  // collisionBlocks.forEach(block => block.draw(offscreenContext));

  return offscreenCanvas
}
// END - Tile setup

// Change xy coordinates to move player's default position
const player = new Player({
  x: SCENE_CENTER_X,
  y: SCENE_CENTER_Y,
  size: 15,
})

const keys = {
  w: {
    pressed: false,
  },
  a: {
    pressed: false,
  },
  s: {
    pressed: false,
  },
  d: {
    pressed: false,
  },
}

const camera = {
  x: SCENE_OFFSET_X,
  y: SCENE_OFFSET_Y,
}

let lastTime = performance.now()
function animate(backgroundCanvas) {
  // Calculate delta time
  const currentTime = performance.now()
  const deltaTime = (currentTime - lastTime) / 1000
  lastTime = currentTime

  player.handleInput(keys)
  player.update(deltaTime, collisionBlocks)

  // Update camera position
  player.center = {
    x: player.x + player.width / 2,
    y: player.y + player.height / 2,
  }

  const maxCameraX = MAP_WIDTH - VIEWPORT_WIDTH
  const maxCameraY = MAP_HEIGHT - VIEWPORT_HEIGHT

  const horizontalScrollDistance = Math.max(0, player.center.x - SCENE_CENTER_X)
  camera.x = Math.min(SCENE_OFFSET_X + horizontalScrollDistance, maxCameraX)

  const verticalScrollDistance = player.center.y - SCENE_CENTER_Y
  camera.y = Math.min(
    Math.max(0, SCENE_OFFSET_Y + verticalScrollDistance),
    maxCameraY,
  )

  // Render scene
  c.save()
  c.scale(SCENE_SCALE, SCENE_SCALE)
  c.translate(-camera.x, -camera.y)
  c.clearRect(0, 0, canvas.width, canvas.height)
  c.drawImage(backgroundCanvas, 0, 0)
  player.draw(c)

  c.restore()

  requestAnimationFrame(() => animate(backgroundCanvas))
}

const startRendering = async () => {
  try {
    const backgroundCanvas = await renderStaticLayers()
    if (!backgroundCanvas) {
      console.error('Failed to create the background canvas')
      return
    }

    animate(backgroundCanvas)
  } catch (error) {
    console.error('Error during rendering:', error)
  }
}

startRendering()

