using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texemon.SceneObjects;

namespace Texemon.Main
{
    public static class Input
    {
        public const float FULL_THUMBSTICK_THRESHOLD = 0.9f;
        public const float THUMBSTICK_DEADZONE_THRESHOLD = 0.1f;

        private const float CONTROL_ICON_DEPTH = 0.11f;
        private static Texture2D keyboardSprite;
        private static Texture2D gamepadSprite;

        private static InputFrame inputFrame = new InputFrame();

        private static MouseState oldMouseState;
        private static MouseState newMouseState;

        public static void ApplySettings()
        {
            inputFrame.ApplySettings();

            oldMouseState = newMouseState = Mouse.GetState();
        }

        public static void Update(GameTime gameTime)
        {
            inputFrame.Update(gameTime);

            oldMouseState = newMouseState;
            newMouseState = Mouse.GetState();

            MousePosition = new Vector2(newMouseState.Position.X, newMouseState.Position.Y) / CrossPlatformGame.Scale;

            DeltaMouseGame = new Vector2((newMouseState.Position.X - oldMouseState.Position.X) / 2.0f, (newMouseState.Position.Y - oldMouseState.Position.Y) / 2.0f) / CrossPlatformGame.Scale;
        }
        /*
        public static int DrawCommand(SpriteBatch spriteBatch, Vector2 position, Color color, PlayerNumber playerNumber, Command command)
        {
            switch (GetPlayerInput(playerNumber).InputSprite)
            {
                case InputSprite.Keyboard: return DrawKey(spriteBatch, position, color, GetPlayerInput(playerNumber).KeyBinding(Command.Interact));
                case InputSprite.Xbox: return DrawButton(spriteBatch, position, color, GetPlayerInput(playerNumber).ButtonBinding(Command.Interact));
            }

            return -1;
        }

        private static int DrawKey(SpriteBatch spriteBatch, Vector2 position, Color color, Keys key)
        {
            int keyLength = 11;
            if (key == Keys.Enter)
            {
                keyLength = 12;
                spriteBatch.Draw(keyboardSprite, position + new Vector2(-keyLength / 2 - 2, -2), new Rectangle(0, 9, 12, 11), color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, CONTROL_ICON_DEPTH);
            }
            else if (key == Keys.Space)
            {
                keyLength = 15;
                spriteBatch.Draw(keyboardSprite, position + new Vector2(-keyLength / 2 - 3, 0), new Rectangle(11, 0, 15, 9), color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, CONTROL_ICON_DEPTH);
            }
            else
            {
                string text = key.ToString();
                spriteBatch.Draw(keyboardSprite, position + new Vector2(-keyLength / 2 - 3, 0), new Rectangle(0, 0, 11, 9), color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, CONTROL_ICON_DEPTH);
                Text.DrawText(spriteBatch, position - new Vector2(Text.GetStringLength(GameFont.Tooltip, text) / 2 + 2, 1), GameFont.Tooltip, text, color);
            }
            return keyLength;
        }

        private static int DrawButton(SpriteBatch spriteBatch, Vector2 position, Color color, Buttons button)
        {
            int buttonLength = 13;

            if (button == Buttons.RightShoulder || button == Buttons.LeftShoulder)
            {
                buttonLength = 14;
                Rectangle source = new Rectangle(0, 0, 14, 13);
                if (button == Buttons.RightShoulder) source.X += 14;
                spriteBatch.Draw(gamepadSprite, position + new Vector2(-buttonLength / 2 - 3, -3), source, color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, CONTROL_ICON_DEPTH);
            }
            else
            {
                Rectangle source = new Rectangle(0, 13, 9, 9);
                if (button == Buttons.B) source.X += 9;
                if (button == Buttons.X) source.X += 9 * 2;
                if (button == Buttons.Y) source.X += 9 * 3;
                spriteBatch.Draw(gamepadSprite, position + new Vector2(-buttonLength / 2 + 1, 0), source, color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, CONTROL_ICON_DEPTH);
            }

            return buttonLength;
        }
        */
        public static bool LeftMouseClicked { get => newMouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed; }
        public static bool RightMouseClicked { get => newMouseState.RightButton == ButtonState.Released && oldMouseState.RightButton == ButtonState.Pressed; }
        public static ButtonState LeftMouseState { get => newMouseState.LeftButton; }
        public static ButtonState RightMouseState { get => newMouseState.RightButton; }
        public static Vector2 MousePosition { get; private set; }
        public static Vector2 DeltaMouseGame { get; private set; }

        public static InputFrame CurrentInput { get => inputFrame; }
    }
}

