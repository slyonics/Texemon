﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Panderling.GameObjects.Maps;
using Panderling.Main;
using Panderling.Scenes;
using Panderling.UserInterface.Controllers;

using GameData;

namespace Panderling.GameObjects.Actors
{
    public enum Orientation
    {
        Up,
        Right,
        Down,
        Left,

        None = -1,
    }

    public abstract class Actor : Entity
    {
        public const int ORIENTATION_COUNT = 4;

        public static Vector2[] ORIENTATION_UNIT_VECTORS = new Vector2[4] { new Vector2(0, -1), new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0) };
        protected static float[] ORIENTATION_ROTATIONS = new float[5] { 0.0f, (float)Math.PI / 2.0f, (float)Math.PI, (float)Math.PI * 3.0f / 2.0f, (float)Math.PI };

        protected const float SHADOW_DEPTH = Camera.MAXIMUM_ENTITY_DEPTH + 0.001f;
        protected const float START_SHADOW = 0.6f;
        protected const float END_SHADOW = 0.8f;
        protected static readonly Color SHADOW_COLOR = new Color(0.0f, 0.0f, 0.0f, 0.5f);

        private const int DAMAGE_FLASH_LENGTH = 500;

        protected MapScene mapScene;

        protected Rectangle boundingBox;
        protected Rectangle currentBounds;
        protected bool ignoreObstacles = false;
        protected bool hitTerrain;

        protected Orientation orientation;
        private bool[] movementArcs = new bool[4];

        private Vector2 displacement;
        protected Vector2 blockedDisplacement;
        protected Vector2 desiredVelocity;
        protected Vector2 knockbackVelocity;

        protected int flinchTimeLeft;
        protected int flinchLength;
        protected int invincibleTimeLeft;
        private List<Bullet> ignoredBullets = new List<Bullet>();
        protected int poise;
        protected int flinch;

        protected int health;

        protected Texture2D shadow = null;
        protected int damageFlashTimeLeft;

        protected List<Controller> controllerList = new List<Controller>();

        public Actor(MapScene iMapScene, Vector2 iPosition, Texture2D iSprite, Dictionary<string, Animation> iAnimationList, Rectangle iBounds, Orientation iOrientation = Orientation.None)
            : base(iMapScene, iPosition, iSprite, iAnimationList)
        {
            mapScene = iMapScene;

            boundingBox = iBounds;
            currentBounds = UpdateBounds(position);

            orientation = iOrientation;
        }

        public static new void LoadContent(ContentManager contentManager)
        {
            Player.LoadContent(contentManager);
            Follower.LoadContent(contentManager);
            Villager.LoadContent(contentManager);
            Enemy.LoadContent(contentManager);
            Bullet.LoadContent(contentManager);
            Pickup.LoadContent(contentManager);
        }

        protected static Texture2D BuildShadow(Rectangle bounds)
        {
            int shadowWidth = (int)Math.Max(1, bounds.Width * 1.25f);
            int shadowHeight = (int)Math.Max(1, bounds.Height * 1.25f);
            float ovalFactorX = ((float)shadowHeight / (shadowWidth + shadowHeight));
            float ovalFactorY = ((float)shadowWidth / (shadowWidth + shadowHeight));
            float maxDistance = (float)Math.Sqrt(Math.Pow(shadowWidth / 2 * ovalFactorX, 2) + Math.Pow(shadowHeight / 2 * ovalFactorY, 2));

            Texture2D result = new Texture2D(CrossPlatformGame.GameInstance.GraphicsDevice, shadowWidth, shadowHeight);
            Color[] colorData = new Color[shadowWidth * shadowHeight];
            for (int y = 0; y < shadowHeight; y++)
            {
                for (int x = 0; x < shadowWidth; x++)
                {
                    float distance = (float)Math.Sqrt(Math.Pow(Math.Abs(x - shadowWidth / 2) * ovalFactorX, 2) + Math.Pow(Math.Abs(y - shadowHeight / 2) * ovalFactorY, 2));
                    float shadowInterval = distance / maxDistance;

                    if (shadowInterval < START_SHADOW) colorData[y * shadowWidth + x] = new Color(0.0f, 0.0f, 0.0f, 1.0f);
                    else if (shadowInterval > END_SHADOW) colorData[y * shadowWidth + x] = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                    else colorData[y * shadowWidth + x] = new Color(0.0f, 0.0f, 0.0f, 1.0f - (shadowInterval - START_SHADOW) / (END_SHADOW - START_SHADOW));
                }
            }
            result.SetData<Color>(colorData);

            return result;
        }

        public override void Update(GameTime gameTime)
        {
            controllerList.RemoveAll(x => x.Terminated);

            if (flinchTimeLeft > 0) flinchTimeLeft -= gameTime.ElapsedGameTime.Milliseconds;
            if (invincibleTimeLeft > 0) invincibleTimeLeft -= gameTime.ElapsedGameTime.Milliseconds;
            if (damageFlashTimeLeft > 0) damageFlashTimeLeft -= gameTime.ElapsedGameTime.Milliseconds;
            else flinch = 0;

            Vector2 startingPosition = position;
            if (flinchTimeLeft > 0) velocity = Vector2.SmoothStep(desiredVelocity, knockbackVelocity, (float)flinchTimeLeft / flinchLength);
            else velocity = desiredVelocity;

            base.Update(gameTime);

            currentBounds = UpdateBounds(position);
            displacement = position - startingPosition;
        }

        public override void Move(GameTime gameTime)
        {
            hitTerrain = false;
            blockedDisplacement = Vector2.Zero;
            Displace(gameTime, mapScene.GameMap);
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            DrawShadow(spriteBatch, camera);
            if (damageFlashTimeLeft > 0) animatedSprite.SpriteColor = ((damageFlashTimeLeft / 150) % 2 == 0) ? Color.White : Color.OrangeRed;
            else animatedSprite.SpriteColor = Color.White;

            base.Draw(spriteBatch, camera);

            if (Settings.GetProgramSetting<bool>("DebugMode")) Debug.DrawBox(spriteBatch, currentBounds);
        }

        protected virtual void DrawShadow(SpriteBatch spriteBatch, Camera camera)
        {
            if (shadow == null) return;

            Color shadowColor = Color.Lerp(SHADOW_COLOR, Color.TransparentBlack, Math.Min(1.0f, positionZ / (boundingBox.Width + boundingBox.Height) / 2));
            spriteBatch.Draw(shadow, new Vector2((int)(Bounds.Center.X - shadow.Width / 2), (int)(Bounds.Center.Y - shadow.Height / 2) + 1), null, shadowColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, SHADOW_DEPTH);
        }

        private void Displace(GameTime gameTime, Map tileMap)
        {
            if (ignoreObstacles)
            {
                position.X += velocity.X * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                position.Y += velocity.Y * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            }
            else
            {
                Vector2 desiredDisplacement = velocity * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                Vector2 expectedPosition = position + desiredDisplacement;
                Vector2 initialExpectedPosition = expectedPosition;
                position = ConstrainedPosition(tileMap, desiredDisplacement);

                float slideDistance = (expectedPosition - position).Length() / 2.0f;
                if (slideDistance > 0.001f && Math.Abs(velocity.X) > 0.001f && Math.Abs(velocity.Y) > 0.001f)
                {
                    currentBounds = UpdateBounds(position);
                    currentBounds = UpdateBounds(position);
                    if (Math.Abs(velocity.Y) > 0.001f)
                    {
                        desiredDisplacement = new Vector2(0.0f, slideDistance * velocity.Y / Math.Abs(velocity.Y));
                        expectedPosition = position + desiredDisplacement;
                        position = ConstrainedPosition(tileMap, desiredDisplacement);
                    }

                    if (slideDistance > 0.001f)
                    {
                        if (Math.Abs(velocity.X) > 0.001f)
                        {
                            desiredDisplacement = new Vector2(slideDistance * velocity.X / Math.Abs(velocity.X), 0.0f);
                            expectedPosition = position + desiredDisplacement;
                            position = ConstrainedPosition(tileMap, desiredDisplacement);
                            slideDistance = (expectedPosition - position).Length() / 2.0f;
                        }
                    }
                }

                blockedDisplacement = initialExpectedPosition - position;
            }
        }

        private Vector2 ConstrainedPosition(Map tileMap, Vector2 displacement)
        {
            Vector2 endPosition = position + displacement;
            Rectangle endBounds = UpdateBounds(endPosition);
            Rectangle displacementBounds = Rectangle.Union(currentBounds, endBounds);

            int startTileX = displacementBounds.Left / Tile.TILE_SIZE;
            int startTileY = displacementBounds.Top / Tile.TILE_SIZE;
            int endTileX = displacementBounds.Right / Tile.TILE_SIZE;
            int endTileY = displacementBounds.Bottom / Tile.TILE_SIZE;
            List<Rectangle> colliderList = new List<Rectangle>();
            List<Rectangle> entityColliders = ActorColliders;

            if (entityColliders != null) colliderList.AddRange(entityColliders);
            for (int y = startTileY; y <= endTileY; y++)
            {
                for (int x = startTileX; x <= endTileX; x++)
                {
                    Tile tile = tileMap.GetTile(x, y);
                    if (tile != null) colliderList.AddRange(tile.ColliderList);
                }
            }

            Vector2 result = position + displacement;
            if (colliderList.Count == 0) return result;
            else
            {
                bool movingRight = (displacement.X > 0.0f);
                bool movingLeft = (displacement.X < 0.0f);
                bool movingDown = (displacement.Y > 0.0f);
                bool movingUp = (displacement.Y < 0.0f);

                float right = position.X + boundingBox.Right;
                float left = position.X + boundingBox.Left;
                float bottom = position.Y + boundingBox.Bottom;
                float top = position.Y + boundingBox.Top;

                float leadingStartX = -999;
                int constraintX = -999;
                if (movingRight) { leadingStartX = position.X + boundingBox.Right; constraintX = int.MaxValue; right += displacement.X; }
                if (movingLeft) { leadingStartX = position.X + boundingBox.Left; constraintX = int.MinValue; left += displacement.X; }
                float leadingEndX = leadingStartX + displacement.X;

                float leadingStartY = -999;
                int constraintY = -999;
                if (movingUp) { leadingStartY = position.Y + boundingBox.Top; constraintY = int.MinValue; top += displacement.Y; }
                if (movingDown) { leadingStartY = position.Y + boundingBox.Bottom; constraintY = int.MaxValue; bottom += displacement.Y; }
                float leadingEndY = leadingStartY + displacement.Y;

                bool constrainedX = false;
                bool constrainedY = false;

                foreach (Rectangle collider in colliderList)
                {
                    if (left > collider.Right) continue;
                    if (right < collider.Left) continue;
                    if (top > collider.Bottom) continue;
                    if (bottom < collider.Top) continue;

                    int blockingX = -999;
                    if (movingRight) blockingX = collider.Left;
                    if (movingLeft) blockingX = collider.Right;

                    int blockingY = -999;
                    if (movingUp) blockingY = collider.Bottom;
                    if (movingDown) blockingY = collider.Top;

                    if (movingRight && leadingStartX < blockingX && leadingEndX > blockingX && blockingX < constraintX) { constraintX = blockingX; constrainedX = true; }
                    if (movingLeft && leadingStartX > blockingX && leadingEndX < blockingX && blockingX > constraintX) { constraintX = blockingX; constrainedX = true; }
                    if (movingUp && leadingStartY > blockingY && leadingEndY < blockingY && blockingY > constraintY) { constraintY = blockingY; constrainedY = true; }
                    if (movingDown && leadingStartY < blockingY && leadingEndY > blockingY && blockingY < constraintY) { constraintY = blockingY; constrainedY = true; }
                }

                if (constrainedX)
                {
                    hitTerrain = true;

                    if (movingRight) result = new Vector2(constraintX - boundingBox.Right - 0.001f, result.Y);
                    if (movingLeft) result = new Vector2(constraintX - boundingBox.Left + 0.001f, result.Y);
                }

                if (constrainedY)
                {
                    hitTerrain = true;

                    if (movingUp) result = new Vector2(result.X, constraintY - boundingBox.Top + 0.001f);
                    if (movingDown) result = new Vector2(result.X, constraintY - boundingBox.Bottom - 0.001f);
                }

                return result;
            }
        }

        protected Rectangle UpdateBounds(Vector2 boxPosition)
        {
            return new Rectangle((int)boxPosition.X + boundingBox.X, (int)boxPosition.Y + boundingBox.Y, boundingBox.Width, boundingBox.Height);
        }

        protected Rectangle UpdateBounds(Vector2 boxPosition, float rotation)
        {
            Matrix rotationMatrix = Matrix.CreateRotationZ(rotation);

            Vector2 leftTop = new Vector2(boundingBox.Left, boundingBox.Top);
            Vector2 rightTop = new Vector2(boundingBox.Right, boundingBox.Top);
            Vector2 leftBottom = new Vector2(boundingBox.Left, boundingBox.Bottom);
            Vector2 rightBottom = new Vector2(boundingBox.Right, boundingBox.Bottom);

            Vector2.Transform(ref leftTop, ref rotationMatrix, out leftTop);
            Vector2.Transform(ref rightTop, ref rotationMatrix, out rightTop);
            Vector2.Transform(ref leftBottom, ref rotationMatrix, out leftBottom);
            Vector2.Transform(ref rightBottom, ref rotationMatrix, out rightBottom);

            Vector2 min = Vector2.Min(Vector2.Min(leftTop, rightTop), Vector2.Min(leftBottom, rightBottom));
            Vector2 max = Vector2.Max(Vector2.Max(leftTop, rightTop), Vector2.Max(leftBottom, rightBottom));

            Rectangle rotatedBoundingBox = new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));

            return new Rectangle((int)boxPosition.X + rotatedBoundingBox.X, (int)boxPosition.Y + rotatedBoundingBox.Y, rotatedBoundingBox.Width, rotatedBoundingBox.Height);
        }

        public void UpdateBounds()
        {
            currentBounds = UpdateBounds(position);
        }

        public virtual void Reorient(Vector2 movement)
        {
            if (movement.Length() < 0.0001f) return;

            float rotation = (float)Math.Atan2(movement.Y, movement.X) + (float)Math.PI / 2.0f;
            for (int i = 0; i < movementArcs.Length; i++)
            {
                float deltaRotation = Math.Min(Math.Abs(rotation - ORIENTATION_ROTATIONS[i]), (float)Math.PI * 2.0f - Math.Abs(rotation - ORIENTATION_ROTATIONS[i]));
                bool newMovementArc = deltaRotation < Math.PI / 2.0f * 0.9f;
                if (newMovementArc && !movementArcs[i]) orientation = (Orientation)i;

                movementArcs[i] = newMovementArc;
            }

            if (Math.Abs(movement.X) > Math.Abs(movement.Y) * 1.05f)
            {
                if (movement.X > 0.0f) orientation = Orientation.Right;
                else orientation = Orientation.Left;
            }
            else if (Math.Abs(movement.Y) > Math.Abs(movement.X) * 1.05f)
            {
                if (movement.Y > 0.0f) orientation = Orientation.Down;
                else orientation = Orientation.Up;
            }
        }

        public virtual void Reorient(Orientation newOrientation)
        {
            orientation = newOrientation;
            for (int i = 0; i < movementArcs.Length; i++) movementArcs[i] = false;
        }

        public float Distance(Rectangle rectangle)
        {
            if (currentBounds.Intersects(rectangle)) return 0.0f;

            float x = 0;
            if (currentBounds.Left - rectangle.Right > 0) x = currentBounds.Left - rectangle.Right;
            else if (rectangle.Left - currentBounds.Right > 0) x = rectangle.Left - currentBounds.Right;

            float y = 0;
            if (currentBounds.Top - rectangle.Bottom > 0) y = currentBounds.Top - rectangle.Bottom;
            else if (rectangle.Top - currentBounds.Bottom > 0) y = rectangle.Top - currentBounds.Bottom;

            return (float)Math.Sqrt(x * x + y * y);
        }

        public float Distance(Actor actor)
        {
            return Distance(actor.Bounds);
        }

        public virtual void Idle()
        {
            desiredVelocity = Vector2.Zero;

            animatedSprite.PlayAnimation("Idle" + orientation.ToString());
        }

        public virtual void Walk(Vector2 movement, float walkSpeed)
        {
            desiredVelocity = movement * walkSpeed;

            Reorient(movement);

            animatedSprite.PlayAnimation("Walk" + orientation.ToString());
        }

        public virtual void Teleport(Vector2 destination)
        {
            position = new Vector2(destination.X + boundingBox.Left + boundingBox.Width / 2, destination.Y + boundingBox.Bottom);
        }

        public virtual bool Hurt(Bullet bullet)
        {
            if (Invincible || Terminated || ignoredBullets.Contains(bullet)) return false;

            health -= bullet.BulletData.damage;
            if (health <= 0) Kill();
            else if (ApplyFlinch(bullet))
            {
                Idle();
                ApplyKnockback(bullet);
            }

            damageFlashTimeLeft = DAMAGE_FLASH_LENGTH;

            ignoredBullets.Add(bullet);

            return true;
        }

        public virtual bool Heal(int healing)
        {
            return false;
        }

        public virtual bool ApplyFlinch(Bullet bullet)
        {
            flinch += bullet.BulletData.flinchStrength;
            if (flinch <= poise) return false;

            flinchLength = flinchTimeLeft = bullet.BulletData.flinchLength;

            return true;
        }

        public virtual void ApplyKnockback(Bullet bullet)
        {
            if (bullet.BulletData.knockback == 0) return;

            knockbackVelocity = bullet.Knockback(this);
        }

        public virtual void Kill()
        {
            terminated = true;
        }

        public virtual void PlayAnimation(string animationName, AnimationFollowup animationFollowup = null)
        {
            if (animationFollowup == null) animatedSprite.PlayAnimation(animationName);
            else animatedSprite.PlayAnimation(animationName, animationFollowup);
        }

        public virtual void OrientedAnimation(string animationName, AnimationFollowup animationFollowup = null)
        {
            PlayAnimation(animationName + orientation.ToString(), animationFollowup);
        }

        public override Vector2 Position { set { position = value; } }
        public Rectangle BoundingBox { get => boundingBox; }
        public Rectangle Bounds { get => currentBounds; }
        public Vector2 Center { get => new Vector2((currentBounds.Left + currentBounds.Right) / 2, currentBounds.Center.Y); }
        public Vector2 Bottom { get => new Vector2((currentBounds.Left + currentBounds.Right) / 2, currentBounds.Bottom); }
        public override float SpriteBottom { get => currentBounds.Bottom; }

        public virtual List<Rectangle> ActorColliders { get => null; }
        public List<Rectangle> NearbyColliders
        {
            get
            {
                int tileStartX = currentBounds.Left / Tile.TILE_SIZE - 1;
                int tileEndX = currentBounds.Right / Tile.TILE_SIZE + 1;
                int tileStartY = currentBounds.Top / Tile.TILE_SIZE - 1;
                int tileEndY = currentBounds.Bottom / Tile.TILE_SIZE + 1;

                List<Rectangle> colliderList = new List<Rectangle>();
                for (int x = tileStartX; x <= tileEndX; x++)
                {
                    for (int y = tileStartY; y <= tileEndY; y++)
                    {
                        colliderList.AddRange(mapScene.GameMap.GetTile(x, y).ColliderList);
                    }
                }

                return colliderList;
            }
        }
        
        public int Poise { get => poise; set => poise = value; }

        public bool Visible { get => parentScene.Camera.View.Intersects(currentBounds); }
        public bool IgnoreObstacles { get => ignoreObstacles; }
        public bool Hurting { get => damageFlashTimeLeft > 0; }
        public bool Flinching { get => flinchTimeLeft > 0; }
        public bool Invincible { get => invincibleTimeLeft > 0; }
        public bool Dead { get => health <= 0; }

        public Orientation Orientation { get => orientation; }

        public Vector2 Displacement { get => displacement; }
        public Vector2 BlockedDisplacement { get => blockedDisplacement; }
        public Vector2 DesiredVelocity { get => desiredVelocity; set => desiredVelocity = value; }
        public List<Controller> ControllerList { get => controllerList; }

        public virtual Vector2 ShootPosition
        {
            get
            {
                switch (orientation)
                {
                    case Orientation.Up: return new Vector2(currentBounds.Center.X, currentBounds.Top);
                    case Orientation.Right: return new Vector2(currentBounds.Right, currentBounds.Center.Y);
                    case Orientation.Down: return new Vector2(currentBounds.Center.X, currentBounds.Bottom);
                    case Orientation.Left: return new Vector2(currentBounds.Left, currentBounds.Center.Y);
                    default: return position;
                }
            }
        }
    }
}
