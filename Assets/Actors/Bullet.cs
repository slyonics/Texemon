using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Panderling.Scenes;
using Panderling.Main;

using GameData;

namespace Panderling.GameObjects.Actors
{
    public class Bullet : Actor
    {
        public enum BulletType
        {
            Invisible,
            Missile,
            Slash
        }

        private static readonly Dictionary<int, Texture2D> BULLET_SHADOWS = new Dictionary<int, Texture2D>();
        private static readonly Dictionary<int, Rectangle> BULLET_BOUNDING_BOXES = new Dictionary<int, Rectangle>()
        {
            { (int)BulletType.Invisible, new Rectangle(0, 0, 0, 0) },
            { (int)BulletType.Missile, new Rectangle(-5, -10, 10, 10) },
            { (int)BulletType.Slash, new Rectangle(-16, -23, 32, 23) }
        };

        private static readonly Dictionary<string, Animation> BULLET_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { BulletType.Invisible.ToString(), new Animation(0, 0, 1, 1, 1, 9999 ) },
            { BulletType.Missile.ToString(), new Animation(0, 0, 10, 10, 2, 50 ) },
            { BulletType.Slash.ToString(), new Animation(0, 0, 32, 23, 3, new int[] { 100, 200, 0 } ) }
        };

        private static Texture2D[] BULLET_SPRITES = new Texture2D[Enum.GetNames(typeof(BulletType)).Length];

        private BulletData bulletData;

        private bool hurtPlayers;
        private bool hurtEnemies;

        private float rotation;

        public Bullet(MapScene iMapScene, Vector2 iPosition, BulletData iBulletData, int bulletId)
            : base(iMapScene, iPosition, BULLET_SPRITES[bulletId], BULLET_ANIMATIONS, BULLET_BOUNDING_BOXES[bulletId])
        {
            bulletData = iBulletData;

            hurtPlayers = bulletData.hurtPlayers;
            hurtEnemies = bulletData.hurtEnemies;

            if (bulletData.drawShadow) shadow = BULLET_SHADOWS[bulletId];
            ignoreObstacles = bulletData.ignoreObstacles;

            if (bulletData.expireAfterAnimation) animatedSprite.PlayAnimation(bulletData.type, AnimationFinished);
            else animatedSprite.PlayAnimation(bulletData.type);
        }

        public static new void LoadContent(ContentManager contentManager)
        {
            foreach (int spriteId in Enum.GetValues(typeof(BulletType)))
            {
                BULLET_SPRITES[spriteId] = contentManager.Load<Texture2D>("Graphics/Bullets/" + Enum.GetName(typeof(BulletType), spriteId));
                BULLET_SHADOWS.Add(spriteId, BuildShadow(BULLET_BOUNDING_BOXES[spriteId]));
            }
        }

        public void Impact(Actor target)
        {
            if (bulletData.expireOnImpact) Kill();

            target.Hurt(this);
        }

        private void AnimationFinished()
        {
            terminated = true;
        }

        public void ProjectFrom(Actor parent)
        {
            position = parent.ShootPosition;            

            switch (bulletData.orientations)
            {
                case 360: break;

                case 4:
                    orientation = parent.Orientation;
                    currentBounds = UpdateBounds(position, ORIENTATION_ROTATIONS[(int)orientation]);
                    animatedSprite.Rotation = ORIENTATION_ROTATIONS[(int)orientation];
                    break;

                case 1:
                    orientation = parent.Orientation;
                    currentBounds = UpdateBounds(position);
                    break;
            }
        }

        public void SetBoundingBox(Rectangle newBounds)
        {
            boundingBox = newBounds;
            currentBounds = UpdateBounds(position);
        }

        public Vector2 Knockback(Actor target)
        {
                Vector2 knockbackDirection;
                if (velocity.Length() < 0.001f)
                {
                    if (bulletData.orientations == 360) knockbackDirection = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
                    else if (bulletData.orientations == 1) knockbackDirection = target.Center - this.Center;
                    else knockbackDirection = ORIENTATION_UNIT_VECTORS[(int)orientation];
                }
                else knockbackDirection = velocity;

                knockbackDirection.Normalize();

                return knockbackDirection * bulletData.knockback;
        }

        public bool HurtPlayers { get => hurtPlayers; }
        public bool HurtEnemies { get => hurtEnemies; }
        public BulletData BulletData { get => bulletData; }
    }
}
