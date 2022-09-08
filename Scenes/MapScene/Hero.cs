using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

using Texemon.SceneObjects.Maps;

namespace Texemon.Scenes.MapScene
{
    public class Hero : Actor
    {
        protected enum HeroAnimation
        {
            IdleDown,
            IdleLeft,
            IdleRight,
            IdleUp,
            WalkDown,
            WalkLeft,
            WalkRight,
            WalkUp
        }

        public const int HERO_WIDTH = 24;
        public const int HERO_HEIGHT = 32;

        public static readonly Rectangle HERO_BOUNDS = new Rectangle(-7, -8, 13, 6);

        private static readonly Dictionary<string, Animation> HERO_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { HeroAnimation.IdleDown.ToString(), new Animation(1, 0, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.IdleLeft.ToString(), new Animation(1, 1, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.IdleRight.ToString(), new Animation(1, 2, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.IdleUp.ToString(), new Animation(1, 3, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.WalkDown.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 4, 240) },
            { HeroAnimation.WalkLeft.ToString(), new Animation(0, 1, HERO_WIDTH, HERO_HEIGHT, 4, 240) },
            { HeroAnimation.WalkRight.ToString(), new Animation(0, 2, HERO_WIDTH, HERO_HEIGHT, 4, 240) },
            { HeroAnimation.WalkUp.ToString(), new Animation(0, 3, HERO_WIDTH, HERO_HEIGHT, 4, 240) }
        };

        private MapScene mapScene;

        public Hero(MapScene iMapScene, Tilemap iTilemap, Vector2 iPosition, StatusScene.HeroModel heroModel, Orientation iOrientation = Orientation.Down)
            : base(iMapScene, iTilemap, iPosition, AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), heroModel.Sprite.Value.ToString())], HERO_ANIMATIONS, HERO_BOUNDS, iOrientation)
        {
            mapScene = iMapScene;

            if (heroModel.FlightHeight.Value > 0)
            {
                SetFlight(heroModel.FlightHeight.Value, AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), heroModel.ShadowSprite.Value.ToString())]);
            }

            /*
            if (mapScene.Tilemap.Name == "TechHomeworld")
            {
                animatedSprite.Scale = new Vector2(0.5f, 0.5f);
                if (shadowSprite != null) shadowSprite.Scale = new Vector2(0.5f, 0.5f);
            }
            */
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            base.Draw(spriteBatch, camera);

            if (Settings.GetProgramSetting<bool>("DebugMode"))
                Debug.DrawBox(spriteBatch, InteractionZone);
        }

        public Rectangle InteractionZone;
    }
}
