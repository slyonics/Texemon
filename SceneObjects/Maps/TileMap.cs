﻿using System;
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
        private Dictionary<int, TiledTileset> tilesets = new Dictionary<int, TiledTileset>();
        private Dictionary<int, Texture2D> tilesetSpritesheets = new Dictionary<int, Texture2D>();

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
                tiledTileset.ParseXml(AssetCache.MAPS[(GameMap)Enum.Parse(typeof(GameMap), "Tilesets_" + Path.GetFileNameWithoutExtension(tiledMapTileset.source))]);
                tiledTileset.sou

                tilesets.Add(tiledMapTileset.firstgid, tiledTileset);
            }

            tiles = new Tile[Columns, Rows];
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    tiles[x, y] = new Tile(this, x, y);
                }
            }

            foreach (TiledGroup tiledGroup in mapData.Groups)
            {
                foreach (TiledLayer tiledLayer in tiledGroup.layers)
                {
                    switch (tiledLayer.type)
                    {
                        case TiledLayerType.TileLayer: LoadTileLayer(tiledLayer, tiledGroup); break;
                        case TiledLayerType.ObjectLayer: LoadObjectLayer(tiledLayer, tiledGroup); break;
                        case TiledLayerType.ImageLayer: LoadImageLayer(tiledLayer, tiledGroup); break;
                    }
                }
            }
        }

        protected virtual void LoadTileLayer(TiledLayer tiledLayer, TiledGroup tiledGroup)
        {
            if (tiledGroup.name == "Background")
            {
                int i = 0;
                for (int y = 0; y < Rows; y++)
                {
                    for (int x = 0; x < Columns; x++, i++)
                    {
                        int tileId = tiledLayer.data[i];
                        if (tileId == 0) continue;

                        TiledMapTileset tiledMapTileset = mapData.GetTiledMapTileset(tileId);
                        TiledTile tilesetTile = mapData.GetTiledTile(tiledMapTileset, tilesets[tiledMapTileset.firstgid], tileId);

                        tiles[x, y].ApplyBackgroundTile(tilesetTile, tiledMapTileset.s);
                    }
                }
            }
        }

        protected virtual void LoadObjectLayer(TiledLayer tiledLayer, TiledGroup tiledGroup)
        {

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
