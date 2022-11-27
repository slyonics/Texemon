﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

using MonsterLegends.SceneObjects.Maps;

namespace MonsterLegends.Scenes.MapScene
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
        public const float RUN_SPEED = 180.0f;

        private MapScene mapScene;

        private IInteractive interactable;
        private InteractionPrompt interactionView;

        public Hero Player { get; set; }

        public PlayerController(MapScene iMapScene, Hero iPlayer)
            : base(PriorityLevel.GameLevel)
        {
            mapScene = iMapScene;
            Player = iPlayer;

            interactionView = mapScene.AddOverlay(new InteractionPrompt(mapScene));
        }

        public override void PreUpdate(GameTime gameTime)
        {
            InputFrame inputFrame = Input.CurrentInput;

            /*
            if (Input.CurrentInput.CommandPressed(Command.Cancel))
            {
                Controller suspendController = mapScene.AddController(new Controller(PriorityLevel.MenuLevel));

                StatusScene.StatusScene statusScene = new StatusScene.StatusScene();
                statusScene.OnTerminated += new TerminationFollowup(suspendController.Terminate);
                CrossPlatformGame.StackScene(statusScene);

                return;
            }
            */

            Vector2 movement = Vector2.Zero;
            if (Input.LeftMouseState == Microsoft.Xna.Framework.Input.ButtonState.Pressed &&
                Input.MousePosition.X >= 0 && Input.MousePosition.Y >= 0 && Input.MousePosition.X < CrossPlatformGame.ScreenWidth && Input.MousePosition.Y < CrossPlatformGame.ScreenHeight)
            {
                movement = Input.MousePosition + mapScene.Camera.Position - Player.Position + new Vector2(mapScene.Camera.CenteringOffsetX, mapScene.Camera.CenteringOffsetY);
                if (movement.Length() > 1.0f) movement.Normalize();
                else movement = Vector2.Zero;
            }
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

                /*
                if (inputFrame.CommandDown(Command.Run))
                {
                    Player.Run(movement, RUN_SPEED);
                }
                else Player.Walk(movement, WALKING_SPEED);
                */

                Player.Walk(movement, WALKING_SPEED);
            }

            Rectangle cameraView = mapScene.Camera.View;
            cameraView.Height -= 32;
            cameraView.Y += 32;
            if (!cameraView.Intersects(Player.Bounds) && !cameraView.Contains(Player.Bounds)) mapScene.HandleOffscreen();
        }

        public override void PostUpdate(GameTime gameTime)
        {
            List<IInteractive> interactableList = new List<IInteractive>(mapScene.NPCs);
            interactableList.AddRange(mapScene.EventTriggers.FindAll(x => x.Interactive));

            IOrderedEnumerable<IInteractive> sortedInteractableList = interactableList.OrderBy(x => Player.Distance(x.Bounds));
            Rectangle interactionZone = Player.Bounds;

            interactionZone.Width = 16;
            interactionZone.Height = 16;
            interactionZone.X -= 1;
            interactionZone.Y -= 9;

            switch (Player.Orientation)
            {
                case Orientation.Up: interactionZone.Y -= (int)(8); break;
                case Orientation.Right: interactionZone.X += (int)(18); break;
                case Orientation.Down: interactionZone.Y += (int)(24); break;
                case Orientation.Left: interactionZone.X -= (int)(18); break;
            }


            

            FindInteractables();
        }

        private void FindInteractables()
        {
            List<IInteractive> interactableList = new List<IInteractive>();
            interactableList.AddRange(mapScene.NPCs.FindAll(x => x.Interactive));
            interactableList.AddRange(mapScene.EventTriggers.FindAll(x => x.Interactive));
            interactableList.AddRange(mapScene.Party.FindAll(x => x.Interactive));

            Hero player = mapScene.PartyLeader;
            IOrderedEnumerable<IInteractive> sortedInteractableList = interactableList.OrderBy(x => player.Distance(x.Bounds));
            Rectangle interactZone = player.Bounds;
            int zoneWidth = mapScene.Tilemap.TileWidth;
            int zoneHeight = mapScene.Tilemap.TileHeight;
            switch (player.Orientation)
            {
                case Orientation.Up:
                    interactZone = new Rectangle((int)player.Position.X - 1 - zoneWidth / 2, (int)player.Position.Y - zoneHeight - 4, zoneWidth, zoneHeight);
                    break;
                case Orientation.Right:
                    interactZone = new Rectangle((int)player.Position.X + 1, (int)player.Position.Y - zoneHeight, zoneWidth, zoneHeight);
                    break;
                case Orientation.Down: player.InteractionZone.Y += mapScene.Tilemap.TileHeight;
                    interactZone = new Rectangle((int)player.Position.X - 1 - zoneWidth / 2, (int)player.Position.Y - zoneHeight / 2, zoneWidth, zoneHeight);
                    break;
                case Orientation.Left:
                    interactZone = new Rectangle((int)player.Position.X - 1 - zoneWidth, (int)player.Position.Y - zoneHeight, zoneWidth, zoneHeight);
                    break;
            }
            player.InteractionZone = interactZone;
            interactable = sortedInteractableList.FirstOrDefault(x => GetInteractBounds(x).Intersects(player.InteractionZone));
            interactionView.Target(interactable);
        }

        private Rectangle GetInteractBounds(IInteractive interactive)
        {
            if (interactive is Hero)
            {
                Rectangle result = interactive.Bounds;
                result.Y -= 8;
                result.Height = 16;
                return result;
            }
            else return interactive.Bounds;
        }
    }
}
