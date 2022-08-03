using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

namespace Texemon.Scenes.MapScene
{
    public class Player : Actor
    {
        protected enum PlayerAnimation
        {
            IdleUp,
            IdleRight,
            IdleDown,
            IdleLeft,
            WalkUp,
            WalkRight,
            WalkDown,
            WalkLeft,
            AttackUp,
            AttackRight,
            AttackDown,
            AttackLeft,
            DodgeUp,
            DodgeRight,
            DodgeDown,
            DodgeLeft,
            HurtUp,
            HurtRight,
            HurtDown,
            HurtLeft,
            Dead,
            Cheer,
            Nod,
            BeginVictory,
            LoopVictory,
            EndVictory,
            BeginPettingRight,
            LoopPettingRight,
            EndPettingRight,
            BeginPettingLeft,
            LoopPettingLeft,
            EndPettingLeft
        }

        public enum PlayerType
        {
            Mockup
        }

        public const int PLAYER_WIDTH = 32;
        public const int PLAYER_HEIGHT = 32;

        public const float WALKING_SPEED = 90.0f;
        public const int HIT_INVINCIBLE_TIME = 800;

        private const int AFTER_IMAGE_SPAWN_RATE = 50;
        private const int DUST_SPAWN_RATE = 40;

        private static Texture2D[] spriteArray = new Texture2D[Enum.GetNames(typeof(PlayerType)).Length];

        private static readonly Vector2[] BULLET_OFFSETS = { new Vector2(0, -12), new Vector2(0, -10), new Vector2(0, -10), new Vector2(0, -10) };
        private static readonly Vector2[] PETTING_OFFSETS = { new Vector2(7, -6), new Vector2(-8, -6) };
        protected static Texture2D PLAYER_SHADOW;
        protected static readonly Rectangle PLAYER_BOUNDING_BOX = new Rectangle(-7, -9, 14, 7);
        private static readonly Dictionary<string, Animation> PLAYER_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { PlayerAnimation.IdleUp.ToString(), new Animation(0, 0, PLAYER_WIDTH, PLAYER_HEIGHT, 1, 1000) },
            { PlayerAnimation.IdleRight.ToString(), new Animation(1, 0, PLAYER_WIDTH, PLAYER_HEIGHT, 1, 1000) },
            { PlayerAnimation.IdleDown.ToString(), new Animation(2, 0, PLAYER_WIDTH, PLAYER_HEIGHT, 1, 1000) },
            { PlayerAnimation.IdleLeft.ToString(), new Animation(3, 0, PLAYER_WIDTH, PLAYER_HEIGHT, 1, 1000) },
            { PlayerAnimation.WalkUp.ToString(), new Animation(0, 1, PLAYER_WIDTH, PLAYER_HEIGHT, 6, 80) },
            { PlayerAnimation.WalkRight.ToString(), new Animation(0, 2, PLAYER_WIDTH, PLAYER_HEIGHT, 6, 80) },
            { PlayerAnimation.WalkDown.ToString(), new Animation(0, 3, PLAYER_WIDTH, PLAYER_HEIGHT, 6, 80) },
            { PlayerAnimation.WalkLeft.ToString(), new Animation(0, 4, PLAYER_WIDTH, PLAYER_HEIGHT, 6, 80) },
            { PlayerAnimation.AttackUp.ToString(), new Animation(0, 5, PLAYER_WIDTH, PLAYER_HEIGHT, 4, 80) },
            { PlayerAnimation.AttackRight.ToString(), new Animation(0, 6, PLAYER_WIDTH, PLAYER_HEIGHT, 4, 80) },
            { PlayerAnimation.AttackDown.ToString(), new Animation(0, 7, PLAYER_WIDTH, PLAYER_HEIGHT, 4, 80) },
            { PlayerAnimation.AttackLeft.ToString(), new Animation(0, 8, PLAYER_WIDTH, PLAYER_HEIGHT, 4, 80) },
            { PlayerAnimation.DodgeUp.ToString(), new Animation(0, 9, PLAYER_WIDTH, PLAYER_HEIGHT, 4, new int[] { 70, 80, 90, 200 }) },
            { PlayerAnimation.DodgeRight.ToString(), new Animation(0, 10, PLAYER_WIDTH, PLAYER_HEIGHT, 4, new int[] { 70, 80, 90, 200 }) },
            { PlayerAnimation.DodgeDown.ToString(), new Animation(0, 11, PLAYER_WIDTH, PLAYER_HEIGHT, 4, new int[] { 70, 80, 90, 200 }) },
            { PlayerAnimation.DodgeLeft.ToString(), new Animation(0, 12, PLAYER_WIDTH, PLAYER_HEIGHT, 4, new int[] { 70, 80, 90, 200 }) },
            { PlayerAnimation.HurtUp.ToString(), new Animation(4, 0, PLAYER_WIDTH, PLAYER_HEIGHT, 1, 1000) },
            { PlayerAnimation.HurtRight.ToString(), new Animation(7, 0, PLAYER_WIDTH, PLAYER_HEIGHT, 1, 1000) },
            { PlayerAnimation.HurtDown.ToString(), new Animation(6, 0, PLAYER_WIDTH, PLAYER_HEIGHT, 1, 1000) },
            { PlayerAnimation.HurtLeft.ToString(), new Animation(5, 0, PLAYER_WIDTH, PLAYER_HEIGHT, 1, 1000) },
            { PlayerAnimation.Dead.ToString(), new Animation(6, 5, PLAYER_WIDTH, PLAYER_HEIGHT, 2, new int[] { 600, 1600 }) },
            { PlayerAnimation.Cheer.ToString(), new Animation(1, 15, PLAYER_WIDTH, PLAYER_HEIGHT, 5, 150) },
            { PlayerAnimation.Nod.ToString(), new Animation(1, 13, PLAYER_WIDTH, PLAYER_HEIGHT, 6, 200) },
            { PlayerAnimation.BeginVictory.ToString(), new Animation(1, 13, PLAYER_WIDTH, PLAYER_HEIGHT, 1, 150) },
            { PlayerAnimation.LoopVictory.ToString(), new Animation(2, 13, PLAYER_WIDTH, PLAYER_HEIGHT, 4, new int[] { 2000, 100, 200, 150 }) },
            { PlayerAnimation.EndVictory.ToString(), new Animation(6, 13, PLAYER_WIDTH, PLAYER_HEIGHT, 1, 150) },
            { PlayerAnimation.BeginPettingRight.ToString(), new Animation(0, 16, PLAYER_WIDTH, PLAYER_HEIGHT, 3, 200) },
            { PlayerAnimation.LoopPettingRight.ToString(), new Animation(3, 16, PLAYER_WIDTH, PLAYER_HEIGHT, 2, new int[] { 400, 200}) },
            { PlayerAnimation.EndPettingRight.ToString(), new Animation(5, 16, PLAYER_WIDTH, PLAYER_HEIGHT, 3, 120) },
            { PlayerAnimation.BeginPettingLeft.ToString(), new Animation(0, 17, PLAYER_WIDTH, PLAYER_HEIGHT, 3, 200) },
            { PlayerAnimation.LoopPettingLeft.ToString(), new Animation(3, 17, PLAYER_WIDTH, PLAYER_HEIGHT, 2, new int[] { 400, 200}) },
            { PlayerAnimation.EndPettingLeft.ToString(), new Animation(5, 17, PLAYER_WIDTH, PLAYER_HEIGHT, 3, 120) }
        };

        protected PlayerProfile playerProfile;

        protected Light light;
        protected int afterImageTime;
        protected int dustTime;

        public Player(MapScene iMapScene, Vector2 iPosition, PlayerProfile iPlayerProfile, Orientation iOrientation)
            : base(iMapScene, iPosition, AssetCache.SPRITES[Enum.GetName(typeof(PlayerType), spriteId)], PLAYER_ANIMATIONS, PLAYER_BOUNDING_BOX, iOrientation)
        {
            playerProfile = iPlayerProfile;

            shadow = PLAYER_SHADOW;

            health = playerProfile.CurrentHealth;

            light = new Light(new Vector2(currentBounds.Center.X, currentBounds.Center.Y), position.Y);
            light.Color = Color.WhiteSmoke;
            light.Intensity = 72.0f;
            mapScene.Tilemap.Weather.LightList.Add(light);

            PLAYER_SHADOW = BuildShadow(PLAYER_BOUNDING_BOX);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            ClampPosition();
            light.Position = new Vector2(currentBounds.Center.X, currentBounds.Center.Y);
            light.PositionZ = SpriteBottom;
            light.Flicker(gameTime, 100.0f, 100.0f, 20000.0f, 1.0f);

            if (animatedSprite.AnimationName.Contains("Dodge"))
            {
                if (animatedSprite.Frame == 3)
                {
                    dustTime -= gameTime.ElapsedGameTime.Milliseconds;
                    if (dustTime <= 0.0f)
                    {
                        mapScene.AddParticle(new DustParticle(mapScene, Bottom + new Vector2(0, 8), SpriteBottom - 0.1f));
                        dustTime = DUST_SPAWN_RATE;
                    }
                }
                else
                {
                    afterImageTime -= gameTime.ElapsedGameTime.Milliseconds;
                    if (afterImageTime <= 0.0f)
                    {
                        mapScene.AddParticle(new AfterImageParticle(parentScene, position, animatedSprite, 200));
                        afterImageTime = AFTER_IMAGE_SPAWN_RATE;
                    }
                }
            }
        }

        public override bool Hurt(Bullet bullet)
        {
            if (base.Hurt(bullet))
            {
                // Audio.PlaySound(GameSound.PlayerHit);
                playerProfile.CurrentHealth = health;
                invincibleTimeLeft = HIT_INVINCIBLE_TIME;

                if (Flinching && !Dead) PlayAnimation("Hurt" + orientation.ToString(), Idle);

                return true;
            }
            else return false;
        }

        public override bool Heal(int healing)
        {
            if (health >= playerProfile.MaximumHealth) return false;

            health = Math.Max(health + healing, playerProfile.MaximumHealth);
            playerProfile.CurrentHealth = health;

            return true;
        }

        public override void Kill()
        {
            PlayAnimation("Dead", mapScene.PlayerWipeout);
        }

        private void ClampPosition()
        {
            Rectangle playableArea = parentScene.Camera.View;

            if (position.X - PLAYER_WIDTH / 4 <= playableArea.Left) position.X = playableArea.Left + PLAYER_WIDTH / 4;
            if (position.X + PLAYER_WIDTH / 4 >= playableArea.Right) position.X = playableArea.Right - PLAYER_WIDTH / 4;
            if (position.Y - PLAYER_HEIGHT <= playableArea.Top) position.Y = playableArea.Top + PLAYER_HEIGHT;
            if (position.Y >= playableArea.Bottom) position.Y = playableArea.Bottom;

            currentBounds = UpdateBounds(position);
        }

        public override Vector2 ShootPosition => position + BULLET_OFFSETS[(int)orientation];
        public Vector2 PettingPosition { get => position + PETTING_OFFSETS[orientation == Orientation.Right ? 0 : 1]; }
        public override List<Rectangle> ActorColliders
        {
            get
            {
                List<Rectangle> colliderList = new List<Rectangle>();
                foreach (Villager villager in mapScene.VillagerList) colliderList.Add(villager.Bounds);

                return colliderList;
            }
        }

        public PlayerProfile PlayerProfile { get => playerProfile; }
        public int Health { get => health; }
    }
}
