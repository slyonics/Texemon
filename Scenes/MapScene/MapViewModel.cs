using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texemon.Main;
using Texemon.Models;
using Texemon.SceneObjects.Controllers;
using Texemon.SceneObjects.Widgets;

namespace Texemon.Scenes.MapScene
{
    public class MapViewModel : ViewModel
    {
        private static readonly Dictionary<string, Animation> ACTOR_ANIMS = new Dictionary<string, Animation>()
        {
            { "Idle", new Animation(0, 0, 128, 128, 1, 1000) },
            { "Talk", new Animation(0, 0, 128, 128, 2, 150) }
        };

        private MapScene mapScene;

        private GameSprite oldActor = GameSprite.Actors_Blank;

        public MapViewModel(MapScene iScene, GameView viewName)
            : base(iScene, PriorityLevel.GameLevel, viewName)
        {
            mapScene = iScene;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);


        }

        public override void LeftClickChild(Vector2 mouseStart, Vector2 mouseEnd, Widget clickWidget, Widget otherWidget)
        {
            switch (clickWidget.Name)
            {
                case "MiniMap":
                    mapScene.MiniMapClick(mouseEnd - clickWidget.AbsolutePosition);
                    break;
            }
        }

        public void SetActor(string spriteName)
        {
            GameSprite newActor = (GameSprite)Enum.Parse(typeof(GameSprite), spriteName);

            if (oldActor == GameSprite.Actors_Blank && newActor != oldActor)
            {
                MapActor.Value = new AnimatedSprite(AssetCache.SPRITES[newActor], ACTOR_ANIMS);
                MapActor.Value.SpriteColor = Color.Transparent;
                TransitionController transitionController = new TransitionController(TransitionDirection.In, 250, PriorityLevel.TransitionLevel);
                transitionController.UpdateTransition += new Action<float>(t => MapActor.Value.SpriteColor = Color.Lerp(Color.Transparent, mapScene.RoomLighting, t));
                parentScene.AddController(transitionController);
            }
            else if (newActor == GameSprite.Actors_Blank && newActor != oldActor)
            {
                TransitionController transitionController = new TransitionController(TransitionDirection.In, 250, PriorityLevel.TransitionLevel);
                transitionController.UpdateTransition += new Action<float>(t => MapActor.Value.SpriteColor = Color.Lerp(mapScene.RoomLighting, Color.Transparent, t));
                transitionController.FinishTransition += new Action<TransitionDirection>(t => MapActor.Value = new AnimatedSprite(AssetCache.SPRITES[newActor], ACTOR_ANIMS));
                parentScene.AddController(transitionController);
            }
            else
            {
                MapActor.Value = new AnimatedSprite(AssetCache.SPRITES[newActor], ACTOR_ANIMS);

                MapActor.Value.SpriteColor = mapScene.RoomLighting;
            }

            oldActor = newActor;
        }

        public void AnimateActor(string animationName)
        {
            MapActor.Value.PlayAnimation(animationName);
        }

        public ModelProperty<string> MapName { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> ConversationFont { get; set; } = new ModelProperty<string>(CrossPlatformGame.Scale == 1 ? "Tooltip" : "BigTooltip");

        public ModelProperty<AnimatedSprite> MapActor { get; set; } = new ModelProperty<AnimatedSprite>(new AnimatedSprite(AssetCache.SPRITES[GameSprite.Actors_Blank], ACTOR_ANIMS));
        public ModelProperty<bool> ShowHealthBar { get; set; } = new ModelProperty<bool>(false);
        public ModelProperty<float> CurrentHealthBar { get; set; } = new ModelProperty<float>(1.0f);
        public ModelProperty<float> MaxHealthBar { get; set; } = new ModelProperty<float>(1.0f);

        public ModelProperty<RenderTarget2D> MapRender { get; set; } = new ModelProperty<RenderTarget2D>(CrossPlatformGame.GameInstance.mapRender);
        public ModelProperty<RenderTarget2D> MiniMapRender { get; set; } = new ModelProperty<RenderTarget2D>(CrossPlatformGame.GameInstance.minimapRender);
    }
}
