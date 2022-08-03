using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

using Texemon.SceneObjects;

namespace Texemon.Scenes.MapScene
{
    public class InteractionView : Overlay
    {
        private const int TRANSITION_LENGTH = 250;

        private PlayerController playerController;
        private MapScene mapScene;

        private IInteractive interactable;
        private int transitionTime;
        private float transitionInterval;
        private int keypressOffset;

        public InteractionView(MapScene iMapScene, PlayerController iPlayerController)
        {
            mapScene = iMapScene;
            playerController = iPlayerController;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            interactable = playerController.Interactable;

            if (interactable == null || mapScene.PriorityLevel != PriorityLevel.GameLevel) keypressOffset = transitionTime = 0;
            else
            {
                transitionTime += gameTime.ElapsedGameTime.Milliseconds;
                if (transitionTime >= TRANSITION_LENGTH) transitionTime = TRANSITION_LENGTH;
            }

            transitionInterval = (float)transitionTime / TRANSITION_LENGTH;
            keypressOffset += gameTime.ElapsedGameTime.Milliseconds;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            /*
            if (interactable != null && mapScene.PriorityLevel == PriorityLevel.GameLevel && !playerController.Player.ControllerList.Exists(x => x is PettingController))
            {
                Color labelColor = Color.Lerp(Color.TransparentBlack, Color.White, transitionInterval);
                Vector2 cameraOffset = new Vector2((int)mapScene.Camera.Position.X + mapScene.Camera.CenteringOffset, (int)mapScene.Camera.Position.Y);
                int labelLength = Text.GetStringLength(GameFont.Menu, interactable.Label);

                Vector2 keyPosition = interactable.LabelPosition - new Vector2(labelLength / 2, (keypressOffset / 700) % 2) - cameraOffset;
                int keyWidth = Input.DrawCommand(spriteBatch, keyPosition, labelColor, playerController.PlayerNumber, Command.Interact);

                Vector2 position = new Vector2((int)interactable.LabelPosition.X, (int)interactable.LabelPosition.Y) - cameraOffset + new Vector2(keyWidth / 2, 0);
                Textbox.DrawLabel(spriteBatch, labelColor, (int)(position.X - labelLength / 2) - 2, (int)position.Y, labelLength + 4, 9);
                Text.DrawCenteredText(spriteBatch, position, GameFont.Menu, interactable.Label, labelColor);
            }
            */
        }
    }
}
