using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTrainer.SceneObjects.Maps;

namespace MonsterTrainer.Scenes.MapScene
{
    public class Bullet : SceneObjects.Maps.Actor
    {
        private MapScene mapScene;

        public const int BULLET_WIDTH = 8;
        public const int BULLET_HEIGHT = 8;

        public static readonly Rectangle BULLET_BOUNDS = new Rectangle(-8, -4, 8, 8);

        private static readonly Dictionary<string, Animation> BULLET_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { "Idle", new Animation(0, 0, BULLET_WIDTH, BULLET_HEIGHT, 1, 1000) },
            { "Flicker", new Animation(0, 0, BULLET_WIDTH, BULLET_HEIGHT, 4, 50) }
        };

        public Bullet(MapScene iMapScene, Vector2 iPosition, GameSprite gameSprite, Orientation iOrientation) 
            : base(iMapScene, iMapScene.Tilemap, iPosition, AssetCache.SPRITES[gameSprite], BULLET_ANIMATIONS, BULLET_BOUNDS, iOrientation)
        {
            mapScene = iMapScene;

            ignoreObstacles = true;

            switch (orientation)
            {
                case Orientation.Up: desiredVelocity = new Vector2(0, -100); break;
                case Orientation.Right: desiredVelocity = new Vector2(100, 0); break;
                case Orientation.Down: desiredVelocity = new Vector2(0, 100); break;
                case Orientation.Left: desiredVelocity = new Vector2(-100, 0); break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!Visible || NearbyColliders.Any(x => x.Intersects(currentBounds))) terminated = true;
        }
    }
}
