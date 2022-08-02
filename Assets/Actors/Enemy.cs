using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Panderling.GameObjects.Controllers;
using Panderling.GameObjects.Particles;
using Panderling.Main;
using Panderling.Procedural;
using Panderling.Scenes;

using GameData;

namespace Panderling.GameObjects.Actors
{
    public enum EnemyType
    {
        BlueSlime,
        PinkSlime,
        AquaFuzz,
        BlueFuzz,
        GreenFuzz,
        PinkFuzz,
        PurpleFuzz,
        YellowFuzz,
        GreyFuzz
    }

    public class Enemy : Actor
    {
        private const int SLIME_WIDTH = 15;
        private const int SLIME_HEIGHT = 14;

        private const int FUZZ_WIDTH = 27;
        private const int FUZZ_HEIGHT = 30;

        private static Dictionary<string, Texture2D> enemySprites = new Dictionary<string, Texture2D>();

        protected static Dictionary<string, Texture2D> ENEMY_SHADOWS = new Dictionary<string, Texture2D>();
        protected static readonly Dictionary<string, Rectangle> ENEMY_BOUNDING_BOXES = new Dictionary<string, Rectangle>()
        {
            { "Slime", new Rectangle(-6, -10, 13, 9) },
            { "Fuzz", new Rectangle(-13, -20, 26, 22) }
        };

        protected static readonly Dictionary<string, Dictionary<string, Animation>> ENEMY_ANIMATIONS = new Dictionary<string, Dictionary<string, Animation>>()
        {
            {
                "Slime", new Dictionary<string, Animation>()
                {
                    { "IdleDown", new Animation(0, 0, SLIME_WIDTH, SLIME_HEIGHT, 4, 180 ) },
                    { "WalkDown", new Animation(0, 0, SLIME_WIDTH, SLIME_HEIGHT, 4, 180 ) },
                    { "Alerted", new Animation(0, 0, SLIME_WIDTH, SLIME_HEIGHT, 4, 70 ) }
                }
            },

            {
                "Fuzz", new Dictionary<string, Animation>()
                {
                    { "IdleDown", new Animation(0, 0, FUZZ_WIDTH, FUZZ_HEIGHT, 6, 180 ) },
                    { "WalkDown", new Animation(0, 0, FUZZ_WIDTH, FUZZ_HEIGHT, 6, 180 ) },
                    { "Alerted", new Animation(0, 0, FUZZ_WIDTH, FUZZ_HEIGHT, 6, 70 ) }
                }
            }
        };

        protected EnemyData data;

        protected Rectangle spawnZone;

        protected string deathScript;

        public Enemy(MapScene iMapScene, Vector2 iPosition, EnemyData iEnemyData, string iScript = null)
            : base(iMapScene, iPosition, enemySprites[iEnemyData.spriteName], ENEMY_ANIMATIONS[iEnemyData.animationName], ENEMY_BOUNDING_BOXES[iEnemyData.boundsName])
        {
            data = iEnemyData;
            deathScript = iScript;
            health = data.maxHealth;

            orientation = (data.orientations == 2) ? Orientation.Right : Orientation.Down;

            ignoreObstacles = data.ignoreObstacles;

            shadow = ENEMY_SHADOWS[data.boundsName];
        }

        public Enemy(MapScene iMapScene, Rectangle iSpawnZone, EnemyData iEnemyData, string iScript = null)
            : this(iMapScene, Rng.RandomPosition(iSpawnZone, ENEMY_BOUNDING_BOXES[iEnemyData.boundsName]), iEnemyData, iScript)
        {
            spawnZone = iSpawnZone;
        }

        public static new void LoadContent(ContentManager contentManager)
        {
            foreach (int enemyId in Enum.GetValues(typeof(EnemyType)))
            {
                string enemyName = Enum.GetName(typeof(EnemyType), enemyId);
                enemySprites.Add(enemyName, contentManager.Load<Texture2D>("Graphics/Enemies/" + enemyName));                
            }

            foreach (KeyValuePair<string, Rectangle> pair in ENEMY_BOUNDING_BOXES)
            {
                ENEMY_SHADOWS.Add(pair.Key, BuildShadow(pair.Value));
            }
        }

        public override void Reorient(Vector2 movement)
        {
            switch (data.orientations)
            {
                case 4: base.Reorient(movement); break;
                case 1: orientation = Orientation.Down; break;

                case 2:
                    if (movement.X < -1) orientation = Orientation.Left;
                    else if (movement.X > 1) orientation = Orientation.Right;
                    break;
            }
        }

        public override bool Hurt(Bullet bullet)
        {
            bool hurt = base.Hurt(bullet);
            if (hurt && health > 0)
            {
                Audio.PlaySound(GameSound.EnemyHit);

                EnemyController enemyController = controllerList.FirstOrDefault(x => x is EnemyController) as EnemyController;
                if (enemyController != null) enemyController.ParseTokens(new string[] { "RunScript", "Hurt" });
            }

            return hurt;
        }

        public override void Kill()
        {
            Audio.PlaySound(GameSound.EnemyDeath, 1.0f);

            parentScene.AddParticle(new AnimationParticle(parentScene, position, AnimationType.Vanquish));

            foreach (DropData itemDrop in data.itemDrops)
            {
                if (Rng.RandomInt(0, 99) < itemDrop.dropRate)
                {
                    float velocityZ = (float)Math.Sqrt(currentBounds.Height) * 40;
                    Pickup pickup = new Pickup(mapScene, position, (PickupType)Enum.Parse(typeof(PickupType), itemDrop.pickupName), velocityZ);
                    mapScene.Add(pickup);
                }
            }

            EnemyController enemyController = controllerList.FirstOrDefault(x => x is EnemyController) as EnemyController;
            if (enemyController != null) enemyController.ParseTokens(new string[] { "RunScript", "Dead" });

            base.Kill();
        }

        public bool Unaware
        {
            get
            {
                EnemyController enemyController = (EnemyController)controllerList.FirstOrDefault(x => x is EnemyController);
                if (enemyController == null) return false;

                return enemyController.Unaware;
            }
        }

        public bool Onscreen { get => mapScene.Camera.View.Intersects(currentBounds); }
        public Rectangle SpawnZone { get => spawnZone; }
        public EnemyData EnemyData { get => data; }
    }
}
