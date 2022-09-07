using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texemon.Models;

namespace Texemon.Scenes.MapScene
{
    public class InteractionPrompt : Overlay
    {
        private const GameFont GAME_FONT = GameFont.Dialogue;

        private MapScene mapScene;
        private IInteractive target;

        private NinePatch textbox;

        private Color color = new Color(252, 224, 168);

        public InteractionPrompt(MapScene iMapScene)
        {
            mapScene = iMapScene;
            textbox = new NinePatch(GameProfile.PlayerProfile.LabelStyle.Value, 0.05f);
        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (CrossPlatformGame.CurrentScene != mapScene) return;

            if (target != null && mapScene.PriorityLevel == PriorityLevel.GameLevel)
            {
                int width = Text.GetStringLength(GAME_FONT, target.Label);
                int height = Text.GetStringHeight(GAME_FONT);
                textbox.Bounds = new Rectangle(0, 0, width + 8, height + 2);

                Vector2 cameraOffset = new Vector2(mapScene.Camera.CenteringOffsetX, mapScene.Camera.CenteringOffsetY);

                textbox.Draw(spriteBatch, target.LabelPosition - mapScene.Camera.Position - new Vector2(textbox.Bounds.Width / 2, 0) - cameraOffset);
                Text.DrawCenteredText(spriteBatch, target.LabelPosition + new Vector2(0, 5) - mapScene.Camera.Position - cameraOffset, GAME_FONT, target.Label, color, 0.03f);
            }
        }

        public void Target(IInteractive newTarget)
        {
            target = newTarget;
        }
    }
}
