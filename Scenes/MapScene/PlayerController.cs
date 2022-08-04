using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texemon.Scenes.MapScene
{
    public class PlayerController : Controller
    {
        public const float WALKING_SPEED = 90.0f;

        public Hero Player { get; set; }

        public PlayerController(Hero iPlayer)
            : base(PriorityLevel.GameLevel)
        {
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

            if (movement.Length() < Input.THUMBSTICK_DEADZONE_THRESHOLD) Player.Idle();
            else
            {
                movement.Normalize();
                Player.Walk(movement, WALKING_SPEED);
            }
        }
    }
}
