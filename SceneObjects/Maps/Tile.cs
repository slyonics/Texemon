using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using TiledCS;

namespace Texemon.SceneObjects.Maps
{
    public class Tile
    {
        private class TileSprite
        {
            public Rectangle source;
            public Texture2D atlas;
            public Color color = Color.White;
        }

        private Tilemap parentMap;
        private Vector2 position;
        private List<TileSprite> backgroundSprites = new List<TileSprite>();        

        public Tile(Tilemap iTileMap, int iTileX, int iTileY)
        {
            parentMap = iTileMap;
            TileX = iTileX;
            TileY = iTileY;

            position = new Vector2(TileX * parentMap.TileWidth, TileY * parentMap.TileHeight);
        }

        public void Update(GameTime gameTime)
        {

        }

        public void DrawBackground(SpriteBatch spriteBatch, Camera camera)
        {
            float depth = 0.9f;
            foreach (TileSprite backgroundSprite in backgroundSprites)
            {
                spriteBatch.Draw(backgroundSprite.atlas, position, backgroundSprite.source, backgroundSprite.color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth);
                depth -= 0.001f;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {

        }

        public void ApplyBackgroundTile(TiledTile tiledTile, Rectangle source, Texture2D atlas)
        {
            TileSprite tileSprite = new TileSprite()
            {
                source = source,
                atlas = atlas
            };

            if (tiledTile != null)
            {
                foreach (TiledProperty tiledProperty in tiledTile.properties)
                {
                    switch (tiledProperty.name)
                    {
                        case "Color": var color = System.Drawing.ColorTranslator.FromHtml(tiledProperty.value); tileSprite.color = new Color(color.R, color.G, color.B, color.A); break;
                    }
                }
            }

            backgroundSprites.Add(tileSprite);
        }

        public int TileX { get; private set; }
        public int TileY { get; private set; }
    }
}
