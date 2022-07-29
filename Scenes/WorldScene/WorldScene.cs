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
            TileMap = new TileMap(this, (GameMap)Enum.Parse(typeof(GameMap), mapName));
        }
    }
}
