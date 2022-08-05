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
            public int height;
        }

        private Tilemap parentMap;
        private Vector2 position;
        private List<TileSprite> backgroundSprites = new List<TileSprite>();
        private Dictionary<int, List<TileSprite>> entitySprites = new Dictionary<int, List<TileSprite>>();

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
                spriteBatch.Draw(backgroundSprite.atlas, position - camera.Position, backgroundSprite.source, backgroundSprite.color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth);
                depth -= 0.001f;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            foreach (KeyValuePair<int, List<TileSprite>> tileSprites in entitySprites)
            {
                float depth = camera.GetDepth(position.Y + tileSprites.Key * parentMap.TileHeight);
                foreach (TileSprite tileSprite in tileSprites.Value)
                {
                    spriteBatch.Draw(tileSprite.atlas, position, tileSprite.source, tileSprite.color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth);
                    depth -= 0.0001f;
                }
            }
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
                foreach (TiledObject tiledObject in tiledTile.objects) ColliderList.Add(new Rectangle((int)(tiledObject.x + position.X), (int)(tiledObject.y + position.Y), (int)tiledObject.width, (int)tiledObject.height));
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

        public void ApplyEntityTile(TiledTile tiledTile, Rectangle source, Texture2D atlas, int height)
        {
            List<TileSprite> tileSprites;
            if (!entitySprites.TryGetValue(height, out tileSprites))
            {
                tileSprites = new List<TileSprite>();
                entitySprites.Add(height, tileSprites);
            }

            TileSprite tileSprite = new TileSprite()
            {
                source = source,
                atlas = atlas,
                height = height
            };

            if (tiledTile != null)
            {
                foreach (TiledObject tiledObject in tiledTile.objects) ColliderList.Add(new Rectangle((int)(tiledObject.x + position.X), (int)(tiledObject.y + position.Y), (int)tiledObject.width, (int)tiledObject.height));
                foreach (TiledProperty tiledProperty in tiledTile.properties)
                {
                    switch (tiledProperty.name)
                    {
                        case "Color": var color = System.Drawing.ColorTranslator.FromHtml(tiledProperty.value); tileSprite.color = new Color(color.R, color.G, color.B, color.A); break;
                    }
                }
            }

            tileSprites.Add(tileSprite);
        }

        public int TileX { get; private set; }
        public int TileY { get; private set; }
        public List<Rectangle> ColliderList { get; private set; } = new List<Rectangle>();
    }
}
