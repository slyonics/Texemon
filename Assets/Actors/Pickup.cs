using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Panderling.GameObjects.Maps;
using Panderling.Main;
using Panderling.Procedural;
using Panderling.Scenes;

namespace Panderling.GameObjects.Actors
{
    public enum PickupType
    {
        Coin,
        Heart,
        GreenMoss,
        BlueMoss
    }

    public class Pickup : Actor
    {
        private const int PICKUP_SIZE = 8;
        private const int SPAWN_SPEED = 100;
        private const float SPAWN_DEACCELERATION = 200.0f;

        private static Texture2D PICKUP_SPRITE;
        private static Dictionary<PickupType, Animation> PICKUP_ANIMATIONS = new Dictionary<PickupType, Animation>()
        {
            { PickupType.Coin, new Animation(1, 0, PICKUP_SIZE, PICKUP_SIZE, 1, 9999) },
            { PickupType.Heart, new Animation(0, 0, PICKUP_SIZE, PICKUP_SIZE, 1, 9999) },
            { PickupType.GreenMoss, new Animation(2, 0, PICKUP_SIZE, PICKUP_SIZE, 1, 9999) },
            { PickupType.BlueMoss, new Animation(3, 0, PICKUP_SIZE, PICKUP_SIZE, 1, 9999) }
        };

        private static Dictionary<PickupType, Texture2D> PICKUP_SHADOWS = new Dictionary<PickupType, Texture2D>();
        private static Dictionary<PickupType, Rectangle> PICKUP_BOUNDING_BOXES = new Dictionary<PickupType, Rectangle>()
        {
            { PickupType.Coin, new Rectangle(-PICKUP_SIZE / 2, -PICKUP_SIZE, PICKUP_SIZE, PICKUP_SIZE) },
            { PickupType.Heart, new Rectangle(-PICKUP_SIZE / 2, -PICKUP_SIZE, PICKUP_SIZE, PICKUP_SIZE) },
            { PickupType.GreenMoss, new Rectangle(-PICKUP_SIZE / 2, -PICKUP_SIZE, PICKUP_SIZE, PICKUP_SIZE) },
            { PickupType.BlueMoss, new Rectangle(-PICKUP_SIZE / 2, -PICKUP_SIZE, PICKUP_SIZE, PICKUP_SIZE) }
        };

        private PickupType pickupType;
        private bool spawnedInObstacle;
        private int decayTimeLeft;
        private bool decayBlink;

        public Pickup(MapScene iMapScene, Vector2 iPosition, PickupType iPickupType, float iVelocityZ, int iDecayTime = 6000)
            : base(iMapScene, iPosition, PICKUP_SPRITE, new Dictionary<string, Animation>() { { "IdleDown", PICKUP_ANIMATIONS[iPickupType] } }, PICKUP_BOUNDING_BOXES[iPickupType])
        {
            pickupType = iPickupType;
            positionZ = 1;
            velocityZ = iVelocityZ;
            decayTimeLeft = iDecayTime;

            foreach (Rectangle collider in NearbyColliders)
            {
                if (collider.Intersects(currentBounds))
                {
                    spawnedInObstacle = true;
                    break;
                }
            }

            if (spawnedInObstacle)
            {
                Vector2 spawnDestination = mapScene.GameMap.GetClearTileNear(position).Center;
                Vector2 spawnVector = spawnDestination - position;
                velocity = desiredVelocity = spawnVector * 2;

                ignoreObstacles = true;
                landingFollowup = Landing;
            }
            else
            {
                double direction = Rng.RandomDouble(0, Math.PI * 2);
                int spawnSpeed = Rng.RandomInt(SPAWN_SPEED / 2, SPAWN_SPEED);
                velocity = desiredVelocity = new Vector2((float)Math.Cos(direction) * spawnSpeed, (float)Math.Sin(direction) * spawnSpeed);
            }

            shadow = PICKUP_SHADOWS[pickupType];
        }

        public static new void LoadContent(ContentManager contentManager)
        {
            PICKUP_SPRITE = contentManager.Load<Texture2D>("Graphics//Pickups");

            foreach (PickupType pickupId in Enum.GetValues(typeof(PickupType)))
            {
                PICKUP_SHADOWS.Add(pickupId, BuildShadow(PICKUP_BOUNDING_BOXES[pickupId]));
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (spawnedInObstacle)
            {

            }
            else
            {
                if (velocity.Length() > 0.001f)
                {
                    if (velocity.X > 0.0f)
                    {
                        velocity.X -= SPAWN_DEACCELERATION * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                        if (velocity.X <= 0.0f) velocity.X = 0.0f;
                    }
                    else if (velocity.X < 0.0f)
                    {
                        velocity.X += SPAWN_DEACCELERATION * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                        if (velocity.X >= 0.0f) velocity.X = 0.0f;
                    }

                    if (velocity.Y > 0.0f)
                    {
                        velocity.Y -= SPAWN_DEACCELERATION * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                        if (velocity.Y <= 0.0f) velocity.Y = 0.0f;
                    }
                    else if (velocity.Y < 0.0f)
                    {
                        velocity.Y += SPAWN_DEACCELERATION * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                        if (velocity.Y >= 0.0f) velocity.Y = 0.0f;
                    }
                }
                else velocity = Vector2.Zero;
            }

            desiredVelocity = velocity;

            if (Collectable && decayTimeLeft > 0)
            {
                decayTimeLeft -= gameTime.ElapsedGameTime.Milliseconds;
                if (decayTimeLeft <= 0) terminated = true;
                else
                {
                    if (decayTimeLeft < 2000) decayBlink = gameTime.TotalGameTime.Milliseconds / 250 % 2 == 0;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (!decayBlink) base.Draw(spriteBatch, camera);
            else DrawShadow(spriteBatch, camera);
        }

        public void Collect(Player collector)
        {
            if (!Collectable) return;

            if (pickupType == PickupType.Coin) Audio.PlaySound(GameSound.Coin, 1.0f);
            else Audio.PlaySound(GameSound.Pickup, 1.0f);
            
            switch (pickupType)
            {
                case PickupType.Heart:
                    if (collector.Heal(2)) Audio.PlaySound(GameSound.Heart, 1.0f);
                    else return;
                    break;
            }

            terminated = true;
        }

        public void Landing()
        {
            desiredVelocity = velocity = Vector2.Zero;
            ignoreObstacles = false;
        }

        public PickupType Type { get => pickupType; }
        public bool Collectable { get => positionZ <= 0.0f && velocityZ <= 0.0f; }
    }
}
