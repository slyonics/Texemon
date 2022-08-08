using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TiledCS;

namespace Texemon.Scenes.MapScene
{
    public class EventTrigger : IInteractive
    {
        private MapScene mapScene;
        private TiledObject objectData;
        
        public string[] Script;

        public EventTrigger(MapScene iMapScene, TiledObject iObjectData)
        {
            mapScene = iMapScene;
            objectData = iObjectData;

            Bounds = new Rectangle((int)objectData.x, (int)objectData.y, (int)objectData.width, (int)objectData.height);
            Script = objectData.properties.FirstOrDefault(x => x.name == "Script").value.Split('\n');
        }

        public bool Activate(Hero hero)
        {
            return false;
        }

        public Rectangle Bounds { get; private set; }
        public bool Interactive { get; set; }
        public bool Terminated { get; set; }

        public string Label { get => "NPC"; }
        public Vector2 LabelPosition { get => new Vector2(Bounds.Center.X, Bounds.Center.Y - Bounds.Height); }
    }
}
