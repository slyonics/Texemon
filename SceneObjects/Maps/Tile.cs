using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Texemon.SceneObjects.Maps
{
    public class Tile
    {
        private TileMap parentMap;

        public Tile(TileMap iTileMap, int iTileX, int iTileY)
        {
            parentMap = iTileMap;
            TileX = iTileX;
            TileY = iTileY;
        }

        public void Update(GameTime gameTime)
        {

        }

        public void DrawBackground(SpriteBatch spriteBatch, Camera camera)
        {

        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {

        }

        public void ApplyLayer()
        {

        }

        public int TileX { get; private set; }
        public int TileY { get; private set; }
    }
}
