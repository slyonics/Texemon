using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.SceneObjects.Maps;

namespace Texemon.Scenes.MapScene
{
    public class MapScene : Scene
    {
        public Tilemap Tilemap { get; set; }

        public MapScene(string mapName)
        {
            Tilemap = AddEntity(new Tilemap(this, (GameMap)Enum.Parse(typeof(GameMap), mapName)));
            Camera = new Camera(new Rectangle(0, 0, Tilemap.Width, Tilemap.Height));
        }

        public override void Update(GameTime gameTime, PriorityLevel priorityLevel = PriorityLevel.GameLevel)
        {
            base.Update(gameTime, priorityLevel);
        }

        public override void DrawBackground(SpriteBatch spriteBatch)
        {
            Tilemap.DrawBackground(spriteBatch, Camera);
        }

        public override void DrawGame(SpriteBatch spriteBatch, Effect shader, Matrix matrix)
        {
            base.DrawGame(spriteBatch, shader, matrix);
        }
    }
}
