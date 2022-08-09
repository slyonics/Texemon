using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Texemon.Scenes.MapScene
{
    public class InteractionPrompt : Overlay
    {
        private const GameFont GAME_FONT = GameFont.Dialogue;

        private IInteractive target;
        private string labelText;

        private NinePatch textbox;

        public InteractionPrompt(IInteractive iTarget, string iLabelText)
        {
            target = iTarget;
            labelText = iLabelText;
            textbox = new NinePatch("Windows_GamePanel", 0.05f);
            SetBounds();
        }

        private void SetBounds()
        {
            int width = Text.GetStringLength(GAME_FONT, labelText);
            int height = Text.GetStringHeight(GAME_FONT);
            textbox.Bounds = new Rectangle(0, 0, width, height);
        }

        public override void Update(GameTime gameTime)
        {
            SetBounds();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            textbox.Draw(spriteBatch, target.LabelPosition);
            Text.DrawCenteredText(spriteBatch, target.LabelPosition + new Vector2(textbox.Bounds.Width / 2), GAME_FONT, labelText);
        }
    }
}
