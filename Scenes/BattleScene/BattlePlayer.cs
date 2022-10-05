using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;
using Texemon.Models;
using Texemon.SceneObjects.Particles;
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
            { HeroAnimation.Dead.ToString(), new Animation(1, 3, HERO_WIDTH, HERO_HEIGHT, 1, 1000) }
        };

        private Dictionary<string, int> exercise = new Dictionary<string, int>();

        private AnimatedSprite shadowSprite;

        private HeroModel heroModel;
        public HeroModel HeroModel { get => heroModel; set { heroModel = value; if (HeroModel.FlightHeight.Value > 1)
            {
                shadowSprite = new AnimatedSprite(AssetCache.SPRITES[HeroModel.ShadowSprite.Value], HERO_ANIMATIONS);
            }
        } }

        public BattlePlayer(Widget iParent, float widgetDepth)
            : base(iParent, widgetDepth)
        {
            shader = AssetCache.EFFECTS[GameShader.BattlePlayer].Clone();
            shader.Parameters["flashInterval"].SetValue(0.0f);
        }

        public override void LoadAttributes(XmlNode xmlNode)
        {
            base.LoadAttributes(xmlNode);
            stats = HeroModel;
            AnimatedSprite = new AnimatedSprite(AssetCache.SPRITES[HeroModel.Sprite.Value], HERO_ANIMATIONS);
            bounds = AnimatedSprite.SpriteBounds();
            battleScene.AddBattler(this);

            if (Stats.Health.Value > HeroModel.MaxHealth.Value / 4) HeroModel.HealthColor.Value = new Color(252, 252, 252, 255);
            else if (Stats.Health.Value > 0) HeroModel.HealthColor.Value = new Color(228, 0, 88, 255);
            else HeroModel.HealthColor.Value = new Color(136, 20, 0, 255);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            shadowSprite?.Draw(spriteBatch, Bottom, null, Depth);
        }

        public void DrawShader(SpriteBatch spriteBatch)
        {
            if (!drawSprite) return;

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, shader, null);
            AnimatedSprite.Draw(spriteBatch, Bottom - new Vector2(0, Dead ? 0 : HeroModel.FlightHeight.Value), null, Depth);
            spriteBatch.End();
        }

        public override void StartTurn()
        {
            base.StartTurn();

            PlayAnimation("Ready");

            HeroModel.NameColor.Value = new Color(206, 109, 10);

            // MENU STUFF
            battleScene.BattleViewModel.StartPlayerTurn(this);            
        }

        public override void EndTurn(int initiativeModifier = 0)
        {
            base.EndTurn(initiativeModifier);

            HeroModel.NameColor.Value = new Color(252, 252, 252, 255);

            battleScene.BattleViewModel.EndPlayerTurn(this);
        }

        public void Exercise(string stat)
        {
            if (exercise.ContainsKey(stat)) exercise[stat] = exercise[stat] + 1;
            else exercise.Add(stat, 1);
        }

        public List<string> GrowAfterBattle()
        {
            List<string> reports = new List<string>();
            foreach (var statUsage in exercise)
            {
                switch (statUsage.Key)
                {
                    case "Mana":
                        Stats.Mana.Value = Stats.Mana.Value + statUsage.Value;
                        reports.Add(Stats.Name.Value + " gained " + statUsage.Value + " point" +
                            ((statUsage.Value > 1) ? "s of " : " of ") + "@Staff MANA!");
                        break;
                }
            }

            return reports;
        }

        public override void Damage(int damage)
        {
            base.Damage(damage);

            PlayAnimation("Hit", Idle);

            if (Stats.Health.Value > HeroModel.MaxHealth.Value / 4) HeroModel.HealthColor.Value = new Color(252, 252, 252, 255);
            else if (Stats.Health.Value > 0) HeroModel.HealthColor.Value = new Color(228, 0, 88, 255);
            else HeroModel.HealthColor.Value = new Color(136, 20, 0, 255);
        }

        public override void Heal(int healing)
        {
            base.Heal(healing);

            if (Stats.Health.Value > HeroModel.MaxHealth.Value / 4) HeroModel.HealthColor.Value = new Color(252, 252, 252, 255);
            else if (Stats.Health.Value > 0) HeroModel.HealthColor.Value = new Color(228, 0, 88, 255);
            else HeroModel.HealthColor.Value = new Color(136, 20, 0, 255);
        }

        public override void Animate(string animationName)
        {
            PlayAnimation(animationName, Idle);
        }

        public void Idle()
        {
            if (Stats.Health.Value > HeroModel.MaxHealth.Value / 4) PlayAnimation("Ready");
            else if (Stats.Health.Value > 0) PlayAnimation("Hurting");
            else PlayAnimation("Dead");
        }

        public override Vector2 Top { get => new Vector2(currentWindow.Center.X, currentWindow.Center.Y + bounds.Y - bounds.Height / 4) + Position; }
        public override Vector2 Center { get => new Vector2(currentWindow.Center.X, currentWindow.Center.Y + bounds.Y + bounds.Height / 4) + Position; }

        public override Rectangle SpriteBounds
        {
            get
            {
                return new Rectangle(currentWindow.Left - 14 + (int)Position.X, currentWindow.Top - 4 + (int)Position.Y, 130, 23);
            }
        }
    }
}
