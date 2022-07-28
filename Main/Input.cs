﻿using Microsoft.Xna.Framework;
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

            MousePosition = new Vector2(newMouseState.Position.X, newMouseState.Position.Y);

            DeltaMouseGame = new Vector2((newMouseState.Position.X - oldMouseState.Position.X) / 2.0f, (newMouseState.Position.Y - oldMouseState.Position.Y) / 2.0f);
        }

        public static bool LeftMouseClicked { get => newMouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed; }
        public static bool RightMouseClicked { get => newMouseState.RightButton == ButtonState.Released && oldMouseState.RightButton == ButtonState.Pressed; }
        public static ButtonState LeftMouseState { get => newMouseState.LeftButton; }
        public static ButtonState RightMouseState { get => newMouseState.RightButton; }
        public static Vector2 MousePosition { get; private set; }
        public static Vector2 DeltaMouseGame { get; private set; }

        public static InputFrame CurrentInput { get => inputFrame; }
    }
}

