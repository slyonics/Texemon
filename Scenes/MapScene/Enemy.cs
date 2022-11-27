﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

using TiledCS;

using MonsterTrainer.SceneObjects.Maps;

namespace MonsterTrainer.Scenes.MapScene
{
    public class Enemy : Actor
    {
        protected enum NpcAnimation
        {
            IdleDown,
            IdleLeft,
            IdleRight,
            IdleUp,
            WalkDown,
            WalkLeft,
            WalkRight,
            WalkUp
        }

        public const int ENEMY_WIDTH = 32;
        public const int ENEMY_HEIGHT = 32;

        public static readonly Rectangle NPC_BOUNDS = new Rectangle(-12, -16, 24, 18);

        private static readonly Dictionary<string, Animation> NPC_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { NpcAnimation.IdleDown.ToString(), new Animation(0, 0, ENEMY_WIDTH, ENEMY_HEIGHT, 2, 240) },
            { NpcAnimation.IdleLeft.ToString(), new Animation(0, 0, ENEMY_WIDTH, ENEMY_HEIGHT, 2, 240) },
            { NpcAnimation.IdleRight.ToString(), new Animation(0, 0, ENEMY_WIDTH, ENEMY_HEIGHT, 2, 240) },
            { NpcAnimation.IdleUp.ToString(), new Animation(0, 0, ENEMY_WIDTH, ENEMY_HEIGHT, 2, 240) },
            { NpcAnimation.WalkDown.ToString(), new Animation(0, 0, ENEMY_WIDTH, ENEMY_HEIGHT, 2, 240) },
            { NpcAnimation.WalkLeft.ToString(), new Animation(0, 0, ENEMY_WIDTH, ENEMY_HEIGHT, 2, 240) },
            { NpcAnimation.WalkRight.ToString(), new Animation(0, 0, ENEMY_WIDTH, ENEMY_HEIGHT, 2, 240) },
            { NpcAnimation.WalkUp.ToString(), new Animation(0, 0, ENEMY_WIDTH, ENEMY_HEIGHT, 2, 240) }
        };

        private MapScene mapScene;

        private int shotCooldown = MAX_SHOT;
        private const int MAX_SHOT = 1000;

        public Enemy(MapScene iMapScene, Tilemap iTilemap, TiledObject tiledObject, string spriteName, Orientation iOrientation = Orientation.Down)
            : base(iMapScene, iTilemap, new Vector2(), NPC_BOUNDS, iOrientation)
        {
            mapScene = iMapScene;

            foreach (TiledProperty tiledProperty in tiledObject.properties)
            {
                switch (tiledProperty.name)
                {
                    case "Idle": IdleScript = tiledProperty.value.Split('\n'); break;
                    case "Collide": CollideScript = tiledProperty.value.Split('\n'); break;
                    case "Sprite":
                        animatedSprite = new AnimatedSprite(AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Actors_" + tiledProperty.value)], NPC_ANIMATIONS);
                        break;
                    case "SpriteHeight":
                        SetFlight(int.Parse(tiledProperty.value), AssetCache.SPRITES[GameSprite.Actors_DroneShadow]);
                        break;
                }
            }

            CenterOn(iTilemap.GetTile(new Vector2(tiledObject.x + tiledObject.width / 2, tiledObject.y + tiledObject.height)).Center);
        }

        public void Collides(Bullet bullet)
        {
            // Terminate();

            //EventController eventController = new EventController(mapScene, CollideScript);
            //mapScene.AddController(eventController);

            bullet.Terminate();

            Health--;
            if (Health <= 0) Terminate();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            shotCooldown -= gameTime.ElapsedGameTime.Milliseconds;
            if (shotCooldown < 0)
            {
                Bullet bullet = new Bullet(mapScene, Position + new Vector2(0, 0), GameSprite.Actors_Bullet, Orientation.Down);
                mapScene.AddEnemyBullet(bullet);

                Bullet bullet1 = new Bullet(mapScene, Position + new Vector2(0, 0), GameSprite.Actors_Bullet, Orientation.Down);
                bullet1.desiredVelocity = new Vector2(-50, 50);
                mapScene.AddEnemyBullet(bullet1);

                Bullet bullet2 = new Bullet(mapScene, Position + new Vector2(0, 0), GameSprite.Actors_Bullet, Orientation.Down);
                bullet2.desiredVelocity = new Vector2(50, 50);
                mapScene.AddEnemyBullet(bullet2);

                shotCooldown = MAX_SHOT;
            }
        }

        public int Health { get; private set; } = 10;

        public string[] IdleScript { get; private set; } = null;
        public string[] CollideScript { get; private set; } = null;
    }
}
