using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;
using Texemon.SceneObjects.Widgets;
using Texemon.Scenes.StatusScene;

namespace Texemon.Scenes.BattleScene
{
    public class BattlePlayer : Battler
    {
        public const int HERO_WIDTH = 24;
        public const int HERO_HEIGHT = 32;

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
            { HeroAnimation.Ready.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 4, 400) },
            { HeroAnimation.Victory.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 4, 200) },
            { HeroAnimation.Guarding.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.Attack.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 4, 80) },
            { HeroAnimation.Chanting.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.Spell.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 3, 80) },
            { HeroAnimation.Hit.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 1, 600) },
            { HeroAnimation.Hurting.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 2, 100) },
            { HeroAnimation.Dead.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 1, 1000) }
        };

        public HeroModel HeroProfile { get; set; }

        public BattlePlayer(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {
            shader = AssetCache.EFFECTS[GameShader.BattlePlayer].Clone();
            shader.Parameters["flashInterval"].SetValue(0.0f);
        }

        public override void LoadAttributes(XmlNode xmlNode)
        {
            base.LoadAttributes(xmlNode);
            stats = HeroProfile;
            AnimatedSprite = new AnimatedSprite(AssetCache.SPRITES[HeroProfile.Sprite.Value], HERO_ANIMATIONS);
            bounds = AnimatedSprite.SpriteBounds();
            battleScene.AddBattler(this);
        }

        public void DrawShader(SpriteBatch spriteBatch)
        {
            if (!drawSprite) return;

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, shader, null);
            AnimatedSprite.Draw(spriteBatch, Bottom, null, Depth);
            spriteBatch.End();
        }

        public override void StartTurn()
        {
            base.StartTurn();

            PlayAnimation("Ready");

            // MENU STUFF
            battleScene.BattleViewModel.PlayerTurn.Value = true;
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
            if (Stats.Health.Value > HeroProfile.MaxHealth.Value / 4) PlayAnimation("Ready");
            else if (Stats.Health.Value > 0) PlayAnimation("Hurting");
            else PlayAnimation("Dead");
        }
    }
}
