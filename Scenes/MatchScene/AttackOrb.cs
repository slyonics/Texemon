using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texemon.Scenes;
using Texemon.Scenes.MatchScene;

namespace Texemon.Scenes.MatchScene
{
    public class AttackOrb : Particle
    {
        private const int ORB_SIZE = 20;

        private static readonly Dictionary<TileColor, GameSprite> spritelookup = new Dictionary<TileColor, GameSprite>()
        {
            { TileColor.Blue, GameSprite.Particles_attackOrb_blue },
            { TileColor.Cyan, GameSprite.Particles_attackOrb_cyan },
            { TileColor.Green, GameSprite.Particles_attackOrb_green },
            { TileColor.Red, GameSprite.Particles_attackOrb_red },
            { TileColor.Yellow, GameSprite.Particles_attackOrb_yellow }
        };

        private Dictionary<string, Animation> ORB_ANIMS = new Dictionary<string, Animation>()
        {
            { "Idle", new Animation(0, 0, ORB_SIZE, ORB_SIZE, 7, 100) }
        };

        private float moveTimer;
        //private float distance = Rng.RandomInt(30, 100);
        private float rotation; // = (float)Rng.RandomDouble(0, Math.PI * 2);
        float Rx;
        float Ry;
        private bool direction;

        private int fade;

        private float rotationSpeed = 300;

        Vector2? attackPosition = null;

        public AttackOrb(Scene iScene, Vector2 iPosition, TileColor tileColor, bool iDir)
            : base(iScene, iPosition, false)
        {
            animatedSprite = new AnimatedSprite(AssetCache.SPRITES[spritelookup[tileColor]], ORB_ANIMS);
            animatedSprite.SpriteColor = new Color(255, 255, 255, 0);
            animatedSprite.Scale = new Vector2(CrossPlatformGame.Scale);
            animatedSprite.PlayAnimation("Idle");
            direction = iDir;

            int r = Rng.RandomInt(30, 50) * CrossPlatformGame.Scale;
            Rx = r;
            Ry = 150 * CrossPlatformGame.Scale - r;
            rotation = direction ? (float)Math.PI / 4 : (float)Math.PI * 2 / 3;
        }

        public override void Update(GameTime gameTime)
        {
            moveTimer += gameTime.ElapsedGameTime.Milliseconds / rotationSpeed;

            if (fade < 2000)
            {
                fade += gameTime.ElapsedGameTime.Milliseconds;
                if (fade > 2000) fade = 2000;

                animatedSprite.SpriteColor = Color.Lerp(new Color(255, 255, 255, 0), new Color(255, 255, 255, 180), (float)fade / 2000);
            }

            float distanceToTarget = 0;
            if (attackPosition.HasValue) distanceToTarget = Vector2.Distance(this.position, attackPosition.Value);

            UpdatePosition(gameTime);

            if (attackPosition.HasValue)
            {
                float newDistanceToTarget = Vector2.Distance(this.position, attackPosition.Value);
                if (newDistanceToTarget > distanceToTarget) Terminate();
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (attackPosition.HasValue) animatedSprite.Draw(spriteBatch, position, camera, 0.20f);
            else
            {
                float x = Rx * (float)Math.Cos(moveTimer) * (float)Math.Cos(rotation) - Ry * (float)Math.Sin(moveTimer) * (float)Math.Sin(rotation);
                float y = Rx * (float)Math.Cos(moveTimer) * (float)Math.Sin(rotation) + Ry * (float)Math.Sin(moveTimer) * (float)Math.Cos(rotation);
                Vector2 offset = new Vector2(x, y);
                animatedSprite.Draw(spriteBatch, position + offset, camera, 0.20f);
            }
        }

        public void SpeedUp(int speed)
        {
            rotationSpeed = 300 - speed * 10;

            if (rotationSpeed < 50) rotationSpeed = 50;
        }

        public void Attack(Vector2 aPosition)
        {
            if (attackPosition.HasValue) return;

            attackPosition = aPosition;

            float x = Rx * (float)Math.Cos(moveTimer) * (float)Math.Cos(rotation) - Ry * (float)Math.Sin(moveTimer) * (float)Math.Sin(rotation);
            float y = Rx * (float)Math.Cos(moveTimer) * (float)Math.Sin(rotation) + Ry * (float)Math.Sin(moveTimer) * (float)Math.Cos(rotation);
            Vector2 offset = new Vector2(x, y);
            position = position + offset;

            Vector2 moveVector = this.position - attackPosition.Value;
            moveVector.Normalize();
            moveVector *= 1000.0f;

            velocity = -moveVector;
        }
    }
}
