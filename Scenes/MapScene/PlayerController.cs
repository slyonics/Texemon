﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

using Texemon.SceneObjects.Maps;

namespace Texemon.Scenes.MapScene
{
    public interface IInteractive
    {
        bool Activate(Hero activator);
        Rectangle Bounds { get; }
        string Label { get; }
        Vector2 LabelPosition { get; }
    }

    public class PlayerController : Controller
    {
        public const float WALKING_SPEED = 90.0f;

        private MapScene mapScene;

        private IInteractive interactable;

        public Hero Player { get; set; }

        public PlayerController(MapScene iMapScene, Hero iPlayer)
            : base(PriorityLevel.GameLevel)
        {
            mapScene = iMapScene;
            Player = iPlayer;
        }

        public override void PreUpdate(GameTime gameTime)
        {
            InputFrame inputFrame = Input.CurrentInput;
            Vector2 movement = Vector2.Zero;

            if (inputFrame.CommandDown(Command.Left)) movement.X -= 1.0f;
            if (inputFrame.CommandDown(Command.Right)) movement.X += 1.0f;
            if (inputFrame.CommandDown(Command.Up)) movement.Y -= 1.0f;
            if (inputFrame.CommandDown(Command.Down)) movement.Y += 1.0f;

            if (inputFrame.CommandPressed(Command.Interact) && interactable != null)
            {
                if (interactable.Activate(Player)) return;
            }

            if (movement.Length() < Input.THUMBSTICK_DEADZONE_THRESHOLD) Player.Idle();
            else
            {
                movement.Normalize();
                Player.Walk(movement, WALKING_SPEED);
            }
        }

        public override void PostUpdate(GameTime gameTime)
        {
            List<IInteractive> interactableList = new List<IInteractive>(mapScene.NPCs);
            interactableList.AddRange(mapScene.EventTriggers.FindAll(x => x.Interactive));

            IOrderedEnumerable<IInteractive> sortedInteractableList = interactableList.OrderBy(x => Player.Distance(x.Bounds));
            Rectangle interactionZone = Player.Bounds;
            switch (Player.Orientation)
            {
                case Orientation.Up: interactionZone.Y -= (int)(Player.BoundingBox.Height * 1.5f); break;
                case Orientation.Right: interactionZone.X += (int)(Player.BoundingBox.Width * 1.5f); break;
                case Orientation.Down: interactionZone.Y += (int)(Player.BoundingBox.Height * 1.5f); break;
                case Orientation.Left: interactionZone.X -= (int)(Player.BoundingBox.Width * 1.5f); break;
            }

            interactable = sortedInteractableList.FirstOrDefault(x => x.Bounds.Intersects(interactionZone));
        }
    }
}
