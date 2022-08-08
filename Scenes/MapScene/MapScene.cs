using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.SceneObjects.Maps;

using TiledCS;

namespace Texemon.Scenes.MapScene
{
    public class MapScene : Scene
    {
        public Tilemap Tilemap { get; set; }

        private Hero player;
        public List<Npc> NPCs { get; private set; } = new List<Npc>();
        public List<EventTrigger> EventTriggers { get; private set; } = new List<EventTrigger>();

        public MapScene(string mapName)
        {
            Tilemap = AddEntity(new Tilemap(this, (GameMap)Enum.Parse(typeof(GameMap), mapName)));
            foreach (TiledProperty tiledProperty in Tilemap.MapData.Properties)
            {
                switch (tiledProperty.name)
                {
                    case "Music": Audio.PlayMusic((GameMusic)Enum.Parse(typeof(GameMusic), tiledProperty.value)); break;
                    case "Script": AddController(new EventController(this, tiledProperty.value.Split('\n'))); break;
                }
            }

            Camera = new Camera(new Rectangle(0, 0, Tilemap.Width, Tilemap.Height));

            player = new Hero(this, Tilemap, new Vector2(32, 32), "dude");
            AddEntity(player);
            PlayerController playerController = new PlayerController(this, player);
            AddController(playerController);

            foreach (Tuple<TiledLayer, TiledGroup> layer in Tilemap.ObjectData)
            {
                switch (layer.Item1.name)
                {
                    case "Triggers":
                        foreach (TiledObject tiledObject in layer.Item1.objects)
                        {
                            EventTriggers.Add(new EventTrigger(this, tiledObject));
                        }
                        break;

                    case "NPCs":
                        foreach (TiledObject tiledObject in layer.Item1.objects)
                        {
                            Npc npc = new Npc(this, Tilemap, new Vector2(tiledObject.x + tiledObject.width / 2, tiledObject.y + tiledObject.height), "gal");
                            NPCs.Add(npc);
                            AddEntity(npc);
                        }
                        break;
                }
            }
        }

        public override void Update(GameTime gameTime, PriorityLevel priorityLevel = PriorityLevel.GameLevel)
        {
            base.Update(gameTime, priorityLevel);

            Camera.Center(player.Center);

            foreach (EventTrigger eventTrigger in EventTriggers)
            {
                if (eventTrigger.Bounds.Intersects(player.Bounds))
                {
                    eventTrigger.Terminated = true;
                    AddController(new EventController(this, eventTrigger.Script));
                }
            }
            EventTriggers.RemoveAll(x => x.Terminated);
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
