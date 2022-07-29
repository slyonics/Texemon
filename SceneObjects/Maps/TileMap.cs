using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TiledCS;

namespace Texemon.SceneObjects.Maps
{
    public class TileMap : Entity
    {
        private GameMap gameMap;
        private TiledMap mapData;

        public TileMap(Scene iScene, GameMap iGameMap)
            : base(iScene, Vector2.Zero)
        {
            gameMap = iGameMap;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);


        }

        public override void DrawShader(SpriteBatch spriteBatch, Camera camera, Matrix matrix)
        {
            base.DrawShader(spriteBatch, camera, matrix);
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            base.Draw(spriteBatch, camera);
        }
    }
}
