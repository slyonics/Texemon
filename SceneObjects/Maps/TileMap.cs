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
    public class TileMap : Entity
    {
        private GameMap gameMap;
        private TiledMap mapData;
        private Dictionary<string, TiledTileset> tilesets = new Dictionary<string, TiledTileset>();

        private Tile[,] tiles;


        public TileMap(Scene iScene, GameMap iGameMap)
            : base(iScene, Vector2.Zero)
        {
            gameMap = iGameMap;

            mapData = new TiledMap();
            mapData.ParseXml(AssetCache.MAPS[gameMap]);
            foreach (TiledMapTileset tiledMapTileset in mapData.Tilesets)
            {
                TiledTileset tiledTileset = new TiledTileset();
                tiledTileset.ParseXml(AssetCache.MAPS[(GameMap)Enum.Parse(typeof(GameMap), Path.GetFileNameWithoutExtension(tiledMapTileset.source))]);
                tilesets.Add(tiledMapTileset.source, tiledTileset);
            }

            tiles = new Tile[Columns, Rows];
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    tiles[x, y] = new Tile(this, x, y);
                }
            }

            foreach (TiledLayer tiledLayer in mapData.Layers)
            {
                if (tiledLayer.type == "tilelayer")
                {

                }
            }
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
            int startTileX = Math.Max((int)(camera.View.Left / mapData.TileWidth) - 1, 0);
            int startTileY = Math.Max((int)(camera.View.Top / mapData.TileHeight) - 1, 0);
            int endTileX = Math.Min((int)(camera.View.Right / mapData.TileWidth) - 1, Columns - 1);
            int endTileY = Math.Min((int)(camera.View.Bottom / mapData.TileHeight) - 1, Rows - 1);

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
            
        }

        public override void DrawShader(SpriteBatch spriteBatch, Camera camera, Matrix matrix)
        {
            
        }

        public Tile GetTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Columns || y >= Rows) return null;
            return tiles[x, y];
        }

        public int Width { get => mapData.Width * mapData.TileWidth; }
        public int Height { get => mapData.Height * mapData.TileHeight; }
        public int Columns { get => mapData.Width; }
        public int Rows { get => mapData.Height; }
    }
}
