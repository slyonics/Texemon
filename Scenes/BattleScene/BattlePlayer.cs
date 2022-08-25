using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Texemon.Scenes.StatusScene;

namespace Texemon.Scenes.BattleScene
{
    public class BattlePlayer : Battler
    {
        public const int HERO_WIDTH = 16;
        public const int HERO_HEIGHT = 21;

        private static Texture2D HERO_SHADOW;
        private static Effect PLAYER_BATTLER_EFFECT;

        protected enum HeroAnimation
        {
            Ready,
            Victory,
            Guarding,
            Attack,
            Chanting,
            Spell,
            Hit,
            Hurting,
            Dead
        }

        public static readonly Dictionary<string, Animation> HERO_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { HeroAnimation.Ready.ToString(), new Animation(0, 4, HERO_WIDTH, HERO_HEIGHT, 4, 200) },
            { HeroAnimation.Victory.ToString(), new Animation(0, 5, HERO_WIDTH, HERO_HEIGHT, 4, 200) },
            { HeroAnimation.Guarding.ToString(), new Animation(0, 6, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.Attack.ToString(), new Animation(0, 6, HERO_WIDTH, HERO_HEIGHT, 4, 80) },
            { HeroAnimation.Chanting.ToString(), new Animation(0, 7, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.Spell.ToString(), new Animation(0, 7, HERO_WIDTH, HERO_HEIGHT, 3, 80) },
            { HeroAnimation.Hit.ToString(), new Animation(0, 8, HERO_WIDTH, HERO_HEIGHT, 1, 600) },
            { HeroAnimation.Hurting.ToString(), new Animation(1, 8, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.Dead.ToString(), new Animation(2, 8, HERO_WIDTH, HERO_HEIGHT, 1, 1000) }
        };

        private HeroModel heroProfile;
        private int partyOrder;

        public BattlePlayer(BattleScene iBattleScene, Vector2 iPosition, HeroModel iHeroProfile, int iPartyOrder)
            : base(iBattleScene, iPosition, AssetCache.SPRITES[iHeroProfile.Sprite.Value], HERO_ANIMATIONS, iHeroProfile.Stats.Value)
        {
            heroProfile = iHeroProfile;
            partyOrder = iPartyOrder;

            shader = PLAYER_BATTLER_EFFECT.Clone();
            shader.Parameters["flashInterval"].SetValue(0.0f);

            shadow = HERO_SHADOW;

            name = heroProfile.Stats.Value.Name.Value;
            health = heroProfile.Stats.Value.Health.Value;
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            DrawShadow(spriteBatch, camera);
        }

        public override void DrawShader(SpriteBatch spriteBatch, Camera camera, Matrix matrix)
        {
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, shader, matrix);
            animatedSprite.Draw(spriteBatch, position - new Vector2(0.0f, positionZ), camera, 0.0f);
            spriteBatch.End();
        }

        public override void StartTurn()
        {
            base.StartTurn();

            PlayAnimation("Ready");

            // MENU STUFF
        }

        public override void EndTurn(int initiativeModifier = 0)
        {
            base.EndTurn(initiativeModifier);

            // commandMenu.Terminate();
        }

        public override void Damage(int damage)
        {
            base.Damage(damage);

            PlayAnimation("Hit", Idle);
        }

        public override void Animate(string animationName)
        {
            PlayAnimation(animationName, Idle);
        }

        public void Idle()
        {
            if (health > heroProfile.Stats.Value.MaxHealth.Value / 4) PlayAnimation("Guarding");
            else if (health > 0) PlayAnimation("Hurting");
            else PlayAnimation("Dead");
        }

        public HeroModel HeroProfile { get => heroProfile; }
    }
}
