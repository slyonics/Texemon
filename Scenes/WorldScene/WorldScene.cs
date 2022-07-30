using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.SceneObjects.Maps;

namespace Texemon.Scenes.WorldScene
{
    public class WorldScene : Scene
    {
        public TileMap TileMap { get; set; }

        public WorldScene(string mapName)
        {
            TileMap = AddEntity(new TileMap(this, (GameMap)Enum.Parse(typeof(GameMap), mapName)));
            Camera = new Camera(new Rectangle(0, 0, TileMap.Width, TileMap.Height));
        }

        public override void Update(GameTime gameTime, PriorityLevel priorityLevel = PriorityLevel.GameLevel)
        {
            base.Update(gameTime, priorityLevel);
        }

        public override void DrawBackground(SpriteBatch spriteBatch)
        {
            TileMap.DrawBackground(spriteBatch, Camera);
        }

        public override void DrawGame(SpriteBatch spriteBatch, Effect shader, Matrix matrix)
        {
            base.DrawGame(spriteBatch, shader, matrix);
        }
    }
}
