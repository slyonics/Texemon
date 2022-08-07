using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using TiledCS;

namespace Texemon.SceneObjects.Maps
{
    public class Tilemap : Entity
    {
        private class Tileset
        {
            public Tileset(TiledTileset iTiledTileset)
            {
                TiledTileset = iTiledTileset;
                SpriteAtlas = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Tiles_" + Path.GetFileNameWithoutExtension(TiledTileset.Image.source))];
            }

            public TiledTileset TiledTileset { get; private set; }
            public Texture2D SpriteAtlas { get; private set; }
        }

        private GameMap gameMap;
        private TiledMap tiledMap;
        private Dictionary<int, Tileset> tilesets = new Dictionary<int, Tileset>();

        private Tile[,] tiles;

        public Tilemap(Scene iScene, GameMap iGameMap)
            : base(iScene, Vector2.Zero)
        {
            gameMap = iGameMap;

            tiledMap = new TiledMap();
            tiledMap.ParseXml(AssetCache.MAPS[gameMap]);
            foreach (TiledMapTileset tiledMapTileset in tiledMap.Tilesets)
            {
                TiledTileset tiledTileset = new TiledTileset();
                tiledTileset.ParseXml(AssetCache.MAPS[(GameMap)Enum.Parse(typeof(GameMap), "Tilesets_" + Path.GetFileNameWithoutExtension(tiledMapTileset.source))]);
                tilesets.Add(tiledMapTileset.firstgid, new Tileset(tiledTileset));
            }

            tiles = new Tile[Columns, Rows];
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    tiles[x, y] = new Tile(this, x, y);
                }
            }

            foreach (TiledGroup tiledGroup in tiledMap.Groups) LoadLayers(tiledGroup.layers, tiledGroup);
            LoadLayers(tiledMap.Layers, null);
        }

        protected virtual void LoadLayers(TiledLayer[] tiledLayers, TiledGroup tiledGroup)
        {
            foreach (TiledLayer tiledLayer in tiledLayers)
            {
                switch (tiledLayer.type)
                {
                    case TiledLayerType.TileLayer: LoadTileLayer(tiledLayer, tiledGroup); break;
                    case TiledLayerType.ObjectLayer: LoadObjectLayer(tiledLayer, tiledGroup); break;
                    case TiledLayerType.ImageLayer: LoadImageLayer(tiledLayer, tiledGroup); break;
                }
            }
        }

        protected virtual void LoadTileLayer(TiledLayer tiledLayer, TiledGroup tiledGroup)
        {
            if (tiledGroup == null || tiledGroup.name == "Background")
            {
                int i = 0;
                for (int y = 0; y < Rows; y++)
                {
                    for (int x = 0; x < Columns; x++, i++)
                    {
                        int tileId = tiledLayer.data[i];
                        if (tileId == 0) continue;

                        TiledMapTileset tiledMapTileset = tiledMap.GetTiledMapTileset(tileId);
                        Tileset tileset = tilesets[tiledMapTileset.firstgid];
                        TiledTileset tiledTileset = tileset.TiledTileset;
                        TiledTile tilesetTile = tiledMap.GetTiledTile(tiledMapTileset, tiledTileset, tileId);
                        TiledSourceRect spriteSource = tiledMap.GetSourceRect(tiledMapTileset, tiledTileset, tileId);

                        tiles[x, y].ApplyBackgroundTile(tilesetTile, new Rectangle(spriteSource.x, spriteSource.y, spriteSource.width, spriteSource.height), tileset.SpriteAtlas);
                    }
                }
            }
            else if (tiledGroup.name.Contains("Height"))
            {
                int height = int.Parse(tiledGroup.name.Remove(0, 6));

                int i = 0;
                for (int y = 0; y < Rows; y++)
                {
                    for (int x = 0; x < Columns; x++, i++)
                    {
                        int tileId = tiledLayer.data[i];
                        if (tileId == 0) continue;

                        TiledMapTileset tiledMapTileset = tiledMap.GetTiledMapTileset(tileId);
                        Tileset tileset = tilesets[tiledMapTileset.firstgid];
                        TiledTileset tiledTileset = tileset.TiledTileset;
                        TiledTile tilesetTile = tiledMap.GetTiledTile(tiledMapTileset, tiledTileset, tileId);
                        TiledSourceRect spriteSource = tiledMap.GetSourceRect(tiledMapTileset, tiledTileset, tileId);

                        tiles[x, y].ApplyEntityTile(tilesetTile, new Rectangle(spriteSource.x, spriteSource.y, spriteSource.width, spriteSource.height), tileset.SpriteAtlas, height);
                    }
                }
            }
        }

        protected virtual void LoadObjectLayer(TiledLayer tiledLayer, TiledGroup tiledGroup)
        {
            ObjectData.Add(new Tuple<TiledLayer, TiledGroup>(tiledLayer, tiledGroup));
        }

        protected virtual void LoadImageLayer(TiledLayer tiledLayer, TiledGroup tiledGroup)
        {

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    tiles[x, y].Update(gameTime);
                }
            }
        }

        public void DrawBackground(SpriteBatch spriteBatch, Camera camera)
        {
            int startTileX = Math.Max((int)(camera.View.Left / tiledMap.TileWidth) - 1, 0);
            int startTileY = Math.Max((int)(camera.View.Top / tiledMap.TileHeight) - 1, 0);
            int endTileX = Math.Min((int)(camera.View.Right / tiledMap.TileWidth), Columns - 1);
            int endTileY = Math.Min((int)(camera.View.Bottom / tiledMap.TileHeight), Rows - 1);

            for (int y = startTileY; y <= endTileY; y++)
            {
                for (int x = startTileX; x <= endTileX; x++)
                {
                    tiles[x, y].DrawBackground(spriteBatch, camera);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            int startTileX = Math.Max((int)(camera.View.Left / tiledMap.TileWidth) - 1, 0);
            int startTileY = Math.Max((int)(camera.View.Top / tiledMap.TileHeight) - 1, 0);
            int endTileX = Math.Min((int)(camera.View.Right / tiledMap.TileWidth), Columns - 1);
            int endTileY = Math.Min((int)(camera.View.Bottom / tiledMap.TileHeight), Rows - 1);

            for (int y = startTileY; y <= endTileY; y++)
            {
                for (int x = startTileX; x <= endTileX; x++)
                {
                    tiles[x, y].Draw(spriteBatch, camera);
                }
            }
        }

        public override void DrawShader(SpriteBatch spriteBatch, Camera camera, Matrix matrix)
        {
            
        }

        public Tile GetTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Columns || y >= Rows) return null;
            return tiles[x, y];
        }

        public TiledMap MapData { get => tiledMap; }
        public List<Tuple<TiledLayer, TiledGroup>> ObjectData { get; } = new List<Tuple<TiledLayer, TiledGroup>>();

        public int TileWidth { get => tiledMap.TileWidth; }
        public int TileHeight { get => tiledMap.TileHeight; }
        public int Width { get => tiledMap.Width * TileWidth; }
        public int Height { get => tiledMap.Height * TileHeight; }
        public int Columns { get => tiledMap.Width; }
        public int Rows { get => tiledMap.Height; }        
    }
}
