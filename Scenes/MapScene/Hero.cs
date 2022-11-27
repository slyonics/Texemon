﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

using Texemon.SceneObjects.Maps;

namespace Texemon.Scenes.MapScene
{
    public class Hero : Actor, IInteractive
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
            WalkUp,
            RunDown,
            RunLeft,
            RunRight,
            RunUp
        }

        public const int HERO_WIDTH = 16;
        public const int HERO_HEIGHT = 16;

        public static readonly Rectangle HERO_BOUNDS = new Rectangle(-7, -7, 14, 7);

        private static readonly Dictionary<string, Animation> HERO_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { HeroAnimation.IdleDown.ToString(), new Animation(0, 2, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.IdleLeft.ToString(), new Animation(0, 3, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.IdleRight.ToString(), new Animation(0, 1, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.IdleUp.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.WalkDown.ToString(), new Animation(0, 2, HERO_WIDTH, HERO_HEIGHT, 2, 240) },
            { HeroAnimation.WalkLeft.ToString(), new Animation(0, 3, HERO_WIDTH, HERO_HEIGHT, 2, 240) },
            { HeroAnimation.WalkRight.ToString(), new Animation(0, 1, HERO_WIDTH, HERO_HEIGHT, 2, 240) },
            { HeroAnimation.WalkUp.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 2, 240) },
            { HeroAnimation.RunDown.ToString(), new Animation(0, 2, HERO_WIDTH, HERO_HEIGHT, 2, 120) },
            { HeroAnimation.RunLeft.ToString(), new Animation(0, 3, HERO_WIDTH, HERO_HEIGHT, 2, 120) },
            { HeroAnimation.RunRight.ToString(), new Animation(0, 1, HERO_WIDTH, HERO_HEIGHT, 2, 120) },
            { HeroAnimation.RunUp.ToString(), new Animation(0, 0, HERO_WIDTH, HERO_HEIGHT, 2, 120) }
        };

        private MapScene mapScene;

        private SceneObjects.Shaders.Light light;

        public Hero(MapScene iMapScene, Tilemap iTilemap, Vector2 iPosition, GameSprite gameSprite, Orientation iOrientation = Orientation.Down)
            : base(iMapScene, iTilemap, iPosition, AssetCache.SPRITES[gameSprite], HERO_ANIMATIONS, HERO_BOUNDS, iOrientation)
        {
            mapScene = iMapScene;

            
        }

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

            if (mapScene.SceneShader != null && mapScene.SceneShader is SceneObjects.Shaders.DayNight)
            {
                light = new SceneObjects.Shaders.Light(position - new Vector2(0, 6), 0.0f);
                light.Color = Color.AntiqueWhite;
                light.Intensity = 50;
                (mapScene.SceneShader as SceneObjects.Shaders.DayNight).Lights.Add(light);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (light != null) light.Position = position - new Vector2(0, 6);
        }

        public override void CenterOn(Vector2 destination)
        {
            base.CenterOn(destination);

            if (light != null) light.Position = position - new Vector2(0, 6);
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            base.Draw(spriteBatch, camera);

            if (Settings.GetProgramSetting<bool>("DebugMode"))
                Debug.DrawBox(spriteBatch, InteractionZone);
        }

        public Rectangle InteractionZone;

        public string Label { get; set; } = "";
        public Vector2 LabelPosition { get => new Vector2(position.X, position.Y - 26); }
        public virtual bool Interactive { get => !string.IsNullOrEmpty(Label); }

        public virtual bool Activate(Hero activator)
        {
            string[] interactionScript = new string[]
                {                    
                    "GiveAffection",
                    "Conversation PetMonster"
                };
            EventController eventController = new EventController(mapScene, interactionScript);
            eventController.ActorSubject = this;

            mapScene.AddController(eventController);
            controllerList.Add(eventController);

            Reorient(activator.Center - Center);
            OrientedAnimation("Idle");

            return true;
        }
    }
}
