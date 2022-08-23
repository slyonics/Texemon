using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Texemon.SceneObjects.Particles;

namespace Texemon.Scenes.BattleScene
{
    public abstract class Battler : Entity
    {
        public const int STANDARD_TURN = 10;

        protected const int DAMAGE_DIGIT_INTERVAL = 80;
        private const int DAMAGE_FLASH_DURATION = 600;

        protected const float SHADOW_DEPTH = Camera.MAXIMUM_ENTITY_DEPTH + 0.001f;
        protected const float START_SHADOW = 0.4f;
        protected const float END_SHADOW = 0.7f;
        protected static readonly Color SHADOW_COLOR = new Color(0.0f, 0.0f, 0.0f, 0.5f);

        protected Texture2D shadow = null;
        protected Effect shader;

        protected int damageDigitTime;
        protected int nextDamageDigitIndex;
        protected List<char> damageDigitsLeft = new List<char>();
        protected List<DamageParticle> damageDigitList = new List<DamageParticle>();

        protected int flashTime;
        protected int flashDuration;

        protected BattleScene battleScene;

        protected int actionTime;
        protected bool turnActive;

        protected string name;
        protected int health;
        protected BattlerModel stats;

        public Battler(BattleScene iBattleScene, Vector2 iPosition, Texture2D iSprite, Dictionary<string, Animation> iAnimationList, BattlerModel iStats)
            : base(iBattleScene, iPosition, iSprite, iAnimationList)
        {
            battleScene = iBattleScene;

            stats = new BattlerModel(iStats);
            health = stats.maxHealth;
            actionTime = 0;
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

            damageDigitList.RemoveAll(x => x.Terminated);

            if (damageDigitTime > 0)
            {
                damageDigitTime -= gameTime.ElapsedGameTime.Milliseconds;
                if (damageDigitTime <= 0 && nextDamageDigitIndex < damageDigitsLeft.Count) SpawnDamageDigit();
            }

            if (flashTime > 0)
            {
                flashTime -= gameTime.ElapsedGameTime.Milliseconds;

                if (flashTime > 0) shader.Parameters["flashInterval"].SetValue((float)flashTime / DAMAGE_FLASH_DURATION);
                else shader.Parameters["flashInterval"].SetValue(0.0f);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            DrawShadow(spriteBatch, camera);

            base.Draw(spriteBatch, camera);
        }

        protected virtual void DrawShadow(SpriteBatch spriteBatch, Camera camera)
        {
            if (shadow == null) return;

            Color shadowColor = Color.Lerp(SHADOW_COLOR, Color.TransparentBlack, Math.Min(1.0f, positionZ / (SpriteBounds.Width + SpriteBounds.Height) / 2));
            spriteBatch.Draw(shadow, new Vector2((int)(SpriteBounds.Center.X - shadow.Width / 2), (int)(SpriteBounds.Center.Y) + 1), null, shadowColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, SHADOW_DEPTH);
        }

        public virtual void DrawShader(SpriteBatch spriteBatch, Camera camera, Matrix matrix)
        {

        }

        public virtual void StartTurn()
        {
            turnActive = true;
        }

        public virtual void EndTurn(int initiativeModifier = 0)
        {
            turnActive = false;
            actionTime += initiativeModifier + STANDARD_TURN;
            battleScene.EnqueueInitiative(this);
        }

        public virtual void Animate(string animationName)
        {

        }

        public virtual void Damage(int damage)
        {
            health -= damage;

            nextDamageDigitIndex = 0;
            damageDigitsLeft.Clear();
            damageDigitsLeft.AddRange(damage.ToString());
            SpawnDamageDigit();

            if (Dead) battleScene.InitiativeList.Remove(this);
        }

        protected virtual void SpawnDamageDigit()
        {
            char digit = damageDigitsLeft[nextDamageDigitIndex];
            DamageParticle damageParticle = new DamageParticle(battleScene, position + new Vector2(nextDamageDigitIndex * 7, 0), digit);
            battleScene.AddParticle(damageParticle);
            damageDigitList.Add(damageParticle);

            nextDamageDigitIndex++;
            damageDigitTime = DAMAGE_DIGIT_INTERVAL;
        }

        public void FlashColor(Color flashColor, int duration = DAMAGE_FLASH_DURATION)
        {
            shader.Parameters["flashColor"].SetValue(flashColor.ToVector4());

            flashTime = flashDuration = duration;
        }

        public string Name { get => name; }
        public BattlerModel Stats { get => stats; }
        public int Health { get => health; }
        public bool Dead { get => health <= 0; }

        public int ActionTime { get => actionTime; set => actionTime = value; }
        public virtual bool Busy { get => turnActive || damageDigitList.Count > 0; }
    }
}
