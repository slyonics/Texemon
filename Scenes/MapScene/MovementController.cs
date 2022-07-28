﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texemon.Main;
using Texemon.Models;
using Texemon.SceneObjects;
using Texemon.SceneObjects.Controllers;

namespace Texemon.Scenes.MapScene
{
    public class MovementController : Controller
    {
        private MapScene mapScene;

        public MovementController(MapScene iScene) : base(PriorityLevel.GameLevel)
        {
            mapScene = iScene;
        }

        public override void PreUpdate(GameTime gameTime)
        {
            InputFrame inputFrame = Input.CurrentInput;
            if (inputFrame.CommandDown(Command.Left)) { Path.Clear(); mapScene.TurnLeft(); }
            else if (inputFrame.CommandDown(Command.Right)) { Path.Clear(); mapScene.TurnRight(); }
            else if (inputFrame.CommandDown(Command.Up)) { Path.Clear(); mapScene.MoveForward(); }
            else if (Input.CurrentInput.CommandPressed(Command.Confirm))
            {
                Path.Clear();
                mapScene.Activate();

                /*
                mapViewModel.ModelProperties["MapActor"].Value = "Actors_Commando";

                Task.Run(() => Activator.CreateInstance(typeof(MatchScene.MatchScene))).ContinueWith(t =>
                {
                    CrossPlatformGame.StackScene((Scene)t.Result);
                });
                */
            }
            else if (Input.CurrentInput.CommandPressed(Command.Menu))
            {
                Path.Clear();
                mapScene.AddView(new MenuViewModel(mapScene));
            }
            else if (Path.Count > 0)
            {
                MapRoom nextRoom = Path.First();
                if (mapScene.roomX == nextRoom.RoomX && mapScene.roomY == nextRoom.RoomY) Path.RemoveAt(0);
                else mapScene.MoveTo(nextRoom);
            }
        }

        public List<MapRoom> Path { get; set; } = new List<MapRoom>();
    }
}
