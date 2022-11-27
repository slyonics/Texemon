using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonsterTrainer.SceneObjects.Particles;
using MonsterTrainer.SceneObjects.Widgets;
using MonsterTrainer.Scenes.StatusScene;

namespace MonsterTrainer.Scenes.BattleScene
{
    public abstract class Battler : Widget
    {
        public const int STANDARD_TURN = 10;

        private const int DAMAGE_FLASH_DURATION = 600;

        protected const float SHADOW_DEPTH = Camera.MAXIMUM_ENTITY_DEPTH + 0.001f;
        protected const float START_SHADOW = 0.4f;
        protected const float END_SHADOW = 0.7f;
        protected static readonly Color SHADOW_COLOR = new Color(0.0f, 0.0f, 0.0f, 0.5f);

        protected BattleScene battleScene;

        protected Texture2D shadow = null;
        protected float positionZ = 0;
        protected Effect shader;
        protected int flashTime;
        protected int flashDuration;

        protected int actionTime;
        protected bool turnActive;

        protected BattlerModel stats;
        public BattlerModel Stats { get => stats; }

        public bool Defending { get; set; }
        public bool Delaying { get; set; }

        public AnimatedSprite AnimatedSprite { get; protected set; }

        public List<Particle> ParticleList { get; } = new List<Particle>();

        public int BiggestStat { get => Math.Max(Stats.Mana.Value, Math.Max(Stats.Strength.Value, Stats.Agility.Value)); }
        public int Initiative { get; set; }

        protected bool drawSprite = false;

        public Battler(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {
            actionTime = 0;

            battleScene = GetParent<ViewModel>().ParentScene as BattleScene;
        }

        protected static Texture2D BuildShadow(Rectangle bounds)
        {
            int shadowWidth = (int)Math.Max(1, bounds.Width * 1.0f);
            int shadowHeight = (int)Math.Max(1, bounds.Height * 1.5f);
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
            base.Update(gameTime);

            drawSprite = false;

            if (flashTime > 0)
            {
                flashTime -= gameTime.ElapsedGameTime.Milliseconds;

                if (flashTime > 0) shader.Parameters["flashInterval"].SetValue((float)flashTime / DAMAGE_FLASH_DURATION);
                else shader.Parameters["flashInterval"].SetValue(0.0f);
            }

            AnimatedSprite.Update(gameTime);

            ParticleList.RemoveAll(x => x.Terminated);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            drawSprite = true;
        }

        public virtual void DrawShadow(SpriteBatch spriteBatch)
        {
            if (shadow == null) return;

            Color shadowColor = Color.Lerp(SHADOW_COLOR, new Color(0, 0, 0, 0), Math.Min(1.0f, positionZ / (currentWindow.Width + currentWindow.Height) / 2));
            spriteBatch.Draw(shadow, new Vector2((int)(Top.X - shadow.Width / 2), (int)(Top.Y) + 1), null, shadowColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, SHADOW_DEPTH);
        }

        public virtual void StartTurn()
        {
            turnActive = true;
            Defending = false;
            Delaying = false;
        }

        public virtual void EndTurn(int initiativeModifier = 0)
        {
            turnActive = false;
            if (Delaying) actionTime += initiativeModifier + STANDARD_TURN / 2;
            else actionTime += initiativeModifier + STANDARD_TURN;
            battleScene.EnqueueInitiative(this);
        }

        public virtual void Animate(string animationName)
        {

        }

        public virtual void PlayAnimation(string animationName, AnimationFollowup animationFollowup = null)
        {
            if (animationFollowup == null) AnimatedSprite.PlayAnimation(animationName);
            else AnimatedSprite.PlayAnimation(animationName, animationFollowup);
        }

        public virtual void Damage(int damage)
        {
            if (Defending)
            {
                damage /= 2;                
            }

            Stats.Health.Value = Math.Max(0, Stats.Health.Value - damage);

            ParticleList.Add(battleScene.AddParticle(new DamageParticle(battleScene, Bottom, damage.ToString())));

            if (Dead) battleScene.InitiativeList.Remove(this);
        }

        public virtual void Miss()
        {
            ParticleList.Add(battleScene.AddParticle(new DamageParticle(battleScene, Bottom, "MISS")));
        }

        public virtual void Repair(int healing)
        {
            if (stats.Class.Value != ClassType.Android && stats.Class.Value != ClassType.Drone) return;
            Heal(healing);
        }

        public virtual void Heal(int healing)
        {
            if (Dead)
            {
                actionTime = 0;
                battleScene.EnqueueInitiative(this);
            }

            Stats.Health.Value = Math.Min(Stats.MaxHealth.Value, Stats.Health.Value + healing);

            ParticleList.Add(battleScene.AddParticle(new DamageParticle(battleScene, Bottom, healing.ToString(), new Color(28, 210, 160))));
        }

        public void FlashColor(Color flashColor, int duration = DAMAGE_FLASH_DURATION)
        {
            shader.Parameters["flashColor"].SetValue(flashColor.ToVector4());

            flashTime = flashDuration = duration;
        }
        
        public bool Dead { get => Stats.Health.Value <= 0; }

        public int ActionTime { get => actionTime; set => actionTime = value; }
        public virtual bool Busy { get => turnActive || ParticleList.Count > 0; }

        public override bool Transitioning { get => GetParent<Panel>().Transitioning; }

        public virtual Vector2 Bottom { get => new Vector2(currentWindow.Center.X, currentWindow.Center.Y + bounds.Y + bounds.Height / 2) + Position; }
        public virtual Vector2 Top { get => new Vector2(currentWindow.Center.X, currentWindow.Center.Y + bounds.Y - bounds.Height / 2) + Position; }
        public virtual Vector2 Center { get => new Vector2(currentWindow.Center.X, currentWindow.Center.Y + bounds.Y) + Position; }


        public virtual Rectangle SpriteBounds
        {
            get
            {
                return new Rectangle(currentWindow.Left + (int)Position.X, currentWindow.Top + (int)Position.Y, currentWindow.Width, currentWindow.Height);
            }
        }
    }
}
