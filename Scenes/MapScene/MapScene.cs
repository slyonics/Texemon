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

        public Hero PartyLeader { get; private set; }
        public List<Npc> NPCs { get; private set; } = new List<Npc>();
        public List<Enemy> Enemies { get; private set; } = new List<Enemy>();
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


            PartyLeader = new Hero(this, Tilemap, new Vector2(32, 96), Models.GameProfile.PlayerProfile.Party.First().Value.Sprite.Value.ToString());
            AddEntity(PartyLeader);
            PlayerController playerController = new PlayerController(this, PartyLeader);
            AddController(playerController);

            Actor leader = PartyLeader;
            foreach (var partymember in Models.GameProfile.PlayerProfile.Party.Skip(1))
            {
                Hero follower = new Hero(this, Tilemap, new Vector2(64, 96), partymember.Value.Sprite.Value.ToString());
                AddEntity(follower);
                FollowerController followerController = new FollowerController(this, follower, leader);
                AddController(followerController);

                leader = follower;
            }            

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
                            Npc npc = new Npc(this, Tilemap, tiledObject, "Base");
                            NpcController npcController = new NpcController(this, npc);
                            NPCs.Add(npc);
                            AddEntity(npc);
                            AddController(npcController);
                        }
                        break;

                    case "Enemies":
                        foreach (TiledObject tiledObject in layer.Item1.objects)
                        {
                            Enemy enemy = new Enemy(this, Tilemap, tiledObject, "Base");
                            EnemyController enemyController = new EnemyController(this, enemy);
                            Enemies.Add(enemy);
                            AddEntity(enemy);
                            AddController(enemyController);
                        }
                        break;
                }
            }
        }

        public MapScene(string mapName, int startX, int startY, Orientation orientation)
            : this(mapName)
        {
            PartyLeader.CenterOn(Tilemap.GetTile(startX, startY).Center);
            PartyLeader.Orientation = orientation;
            PartyLeader.Idle();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Camera.Center(PartyLeader.Center);

            bool eventTriggered = false;
            foreach (EventTrigger eventTrigger in EventTriggers)
            {
                if (eventTrigger.Bounds.Intersects(PartyLeader.Bounds))
                {
                    eventTriggered = true;
                    eventTrigger.Terminated = true;
                    AddController(new EventController(this, eventTrigger.Script));
                }
            }
            EventTriggers.RemoveAll(x => x.Terminated);

            if (!eventTriggered)
            {
                foreach (Enemy enemy in Enemies)
                {
                    if (enemy.Bounds.Intersects(PartyLeader.Bounds))
                    {
                        enemy.Collides();
                    }
                }
            }

            NPCs.RemoveAll(x => x.Terminated);
            Enemies.RemoveAll(x => x.Terminated);
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
