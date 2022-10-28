using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texemon.Models;
using Texemon.SceneObjects.Maps;
using Texemon.Scenes.StatusScene;
using TiledCS;

namespace Texemon.Scenes.MapScene
{
    public class MapScene : Scene
    {
        public Tilemap Tilemap { get; set; }

        public List<Hero> Party { get; private set; } = new List<Hero>();
        public Hero PartyLeader { get => Party.FirstOrDefault(); }

        public List<Npc> NPCs { get; private set; } = new List<Npc>();
        public List<Enemy> Enemies { get; private set; } = new List<Enemy>();
        public List<EventTrigger> EventTriggers { get; private set; } = new List<EventTrigger>();

        private ParallaxBackdrop parallaxBackdrop;

        public MapScene(string mapName)
        {
            Tilemap = AddEntity(new Tilemap(this, (GameMap)Enum.Parse(typeof(GameMap), mapName)));
            foreach (TiledProperty tiledProperty in Tilemap.MapData.Properties)
            {
                switch (tiledProperty.name)
                {
                    case "Music": Audio.PlayMusic((GameMusic)Enum.Parse(typeof(GameMusic), tiledProperty.value)); break;
                    case "Script": AddController(new EventController(this, tiledProperty.value.Split('\n'))); break;

                    case "ColorFilter": SceneShader = new SceneObjects.Shaders.ColorFade(Graphics.ParseHexcode("#" + tiledProperty.value.Substring(3)), 0.75f); break;
                    case "DayNight": SceneShader = new SceneObjects.Shaders.DayNight(Graphics.ParseHexcode("#" + tiledProperty.value.Substring(3)), 1.2f); break;
                    case "HeatDistortion": SceneShader = new SceneObjects.Shaders.HeatDistortion(); break;

                    case "Background": BuildParallaxBackground(tiledProperty.value); break;
                }
            }

            Camera = new Camera(new Rectangle(0, 0, Tilemap.Width, Tilemap.Height));

            var leaderHero = new Hero(this, Tilemap, new Vector2(32, 96), Models.GameProfile.PlayerProfile.Party.First().Value);
            Party.Add(leaderHero);
            AddEntity(leaderHero);
            PlayerController playerController = new PlayerController(this, leaderHero);
            AddController(playerController);

            Actor leader = leaderHero;
            foreach (var partymember in Models.GameProfile.PlayerProfile.Party.Skip(1))
            {
                Hero follower = new Hero(this, Tilemap, new Vector2(64, 96), partymember.Value);
                Party.Add(follower);
                AddEntity(follower);
                FollowerController followerController = new FollowerController(this, follower, leader);
                AddController(followerController);

                leader = follower;
            }            

            foreach (Tuple<TiledLayer, TiledGroup> layer in Tilemap.ObjectData)
            {
                foreach (TiledObject tiledObject in layer.Item1.objects)
                {
                    var prop = tiledObject.properties.FirstOrDefault(x => x.name == "EnableIf");
                    if (prop != null && !Models.GameProfile.GetSaveData<bool>(prop.value)) continue;

                    prop = tiledObject.properties.FirstOrDefault(x => x.name == "DisableIf");
                    if (prop != null && Models.GameProfile.GetSaveData<bool>(prop.value)) continue;

                    switch (layer.Item1.name)
                    {
                        case "Triggers":
                            {
                                EventTriggers.Add(new EventTrigger(this, tiledObject));
                            }
                            break;

                        case "NPCs":
                            {
                                Npc npc = new Npc(this, Tilemap, tiledObject, "Base");
                                NpcController npcController = new NpcController(this, npc);
                                NPCs.Add(npc);
                                AddEntity(npc);
                                AddController(npcController);
                            }
                            break;

                        case "Chests":
                            {
                                Chest chest = new Chest(this, Tilemap, tiledObject, tiledObject.properties.First(x => x.name == "Sprite").value);
                                NPCs.Add(chest);
                                AddEntity(chest);
                            }
                            break;

                        case "Enemies":
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
        }

        public MapScene(string mapName, int startX, int startY, Orientation orientation)
            : this(mapName)
        {
            PartyLeader.CenterOn(Tilemap.GetTile(startX, startY).Bottom);
            PartyLeader.Orientation = orientation;
            PartyLeader.Idle();

            PartyLeader.UpdateBounds();
            Camera.Center(PartyLeader.Center);

            int i = 1;
            foreach (Hero hero in Party.Skip(1))
            {
                hero.CenterOn(new Vector2(PartyLeader.SpriteBounds.Left + i * 6, PartyLeader.SpriteBounds.Bottom - 12 + (i % 2) * 6));
                hero.Orientation = orientation;
                hero.Idle();

                i++;
            }

            SaveMapPosition();
        }

        public MapScene(string mapName, Vector2 leaderPosition)
            : this(mapName)
        {
            PartyLeader.Position = leaderPosition;
            PartyLeader.Orientation = Orientation.Down;
            PartyLeader.Idle();

            PartyLeader.UpdateBounds();
            Camera.Center(PartyLeader.Center);

            int i = 1;
            foreach (Hero hero in Party.Skip(1))
            {
                hero.CenterOn(new Vector2(PartyLeader.SpriteBounds.Left + i * 6, PartyLeader.SpriteBounds.Bottom - 12 + (i % 2) * 6));
                hero.Orientation = Orientation.Down;
                hero.Idle();

                i++;
            }
        }

        public MapScene(string mapName, string sourceMapName)
            : this(mapName)
        {
            var spawnZone = EventTriggers.First(x => x.Name == sourceMapName);

            Orientation orientation = (Orientation)Enum.Parse(typeof(Orientation), spawnZone.GetProperty("Direction"));

            Vector2 spawnPosition = Vector2.Zero;
            switch (orientation)
            {
                case Orientation.Up: spawnPosition = new Vector2(spawnZone.Bounds.Center.X, spawnZone.Bounds.Top); break;
                case Orientation.Right: spawnPosition = new Vector2(); break;
                case Orientation.Down: spawnPosition = new Vector2(spawnZone.Bounds.Center.X, spawnZone.Bounds.Bottom + PartyLeader.Bounds.Height); break;
                case Orientation.Left: spawnPosition = new Vector2(); break;
            }
            PartyLeader.CenterOn(spawnPosition);
            PartyLeader.Orientation = orientation;
            PartyLeader.Idle();

            PartyLeader.UpdateBounds();
            Camera.Center(PartyLeader.Center);

            int i = 1;
            foreach (Hero hero in Party.Skip(1))
            {
                hero.CenterOn(new Vector2(PartyLeader.SpriteBounds.Left + i * 6, PartyLeader.SpriteBounds.Bottom - 12 + (i % 2) * 6));
                hero.Orientation = orientation;
                hero.Idle();

                i++;
            }

            SaveMapPosition();
        }

        public void SaveMapPosition()
        {
            GameProfile.SetSaveData<string>("LastMapName", Tilemap.Name);
            GameProfile.SetSaveData<int>("LastPositionX", (int)PartyLeader.Position.X);
            GameProfile.SetSaveData<int>("LastPositionY", (int)PartyLeader.Position.Y);
            GameProfile.SetSaveData<string>("PlayerLocation", Tilemap.MapData.Properties.First(x => x.name == "LocationName").value);
        }

        public void AddPartyMember(HeroModel heroModel)
        {
            Hero follower = new Hero(this, Tilemap, new Vector2(64, 96), heroModel);            
            AddEntity(follower);
            FollowerController followerController = new FollowerController(this, follower, Party.Last());
            AddController(followerController);
            Party.Add(follower);
            follower.CenterOn(new Vector2(PartyLeader.SpriteBounds.Center.X, PartyLeader.SpriteBounds.Bottom + 2));
            follower.Orientation = Orientation.Down;
            follower.Idle();
            AddParticle(new SceneObjects.Particles.AnimationParticle(this, follower.Center, SceneObjects.Particles.AnimationType.Smoke));
        }

        public void AddPartyMember(HeroModel heroModel, Actor oldActor)
        {
            Hero follower = new Hero(this, Tilemap, new Vector2(64, 96), heroModel);
            AddEntity(follower);
            FollowerController followerController = new FollowerController(this, follower, Party.Last());
            AddController(followerController);
            Party.Add(follower);
            follower.Position = oldActor.Position;
            follower.Orientation = oldActor.Orientation;
            follower.UpdateBounds();
            follower.Idle();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Camera.Center(PartyLeader.Center);

            bool eventTriggered = false;
            foreach (EventTrigger eventTrigger in EventTriggers)
            {
                if (eventTrigger.Bounds.Intersects(PartyLeader.Bounds) && !eventTrigger.Interactive)
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

            parallaxBackdrop?.Update(gameTime, Camera);
        }

        public override void DrawBackground(SpriteBatch spriteBatch)
        {
            parallaxBackdrop?.Draw(spriteBatch);

            Tilemap.DrawBackground(spriteBatch, Camera);
        }

        private void BuildParallaxBackground(string background)
        {
            string[] tokens = background.Split(' ');

            parallaxBackdrop = new ParallaxBackdrop(tokens[0], tokens.Skip(1).Select(x => float.Parse(x)).ToArray());
        }

        public override void DrawGame(SpriteBatch spriteBatch, Effect shader, Matrix matrix)
        {
            base.DrawGame(spriteBatch, shader, matrix);
        }

        public void HandleOffscreen()
        {
            var travelZone = EventTriggers.Where(x => x.TravelZone && x.DefaultTravelZone).OrderBy(x => Vector2.Distance(new Vector2(x.Bounds.Center.X, x.Bounds.Center.Y), PartyLeader.Position)).First();
            travelZone.Activate(PartyLeader);
        }
    }
}
