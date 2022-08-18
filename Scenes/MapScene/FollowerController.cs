using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.SceneObjects.Maps;

namespace Texemon.Scenes.MapScene
{
    public class FollowerController : Controller
    {
        private enum Behavior
        {
            Idling,
            Regrouping,
            Stuck,
            PickupHeart,
            Waiting,
            Aggressive,
            Evasive
        }

        private const int START_REGROUP_DISTANCE = 32;
        private const int END_REGROUP_DISTANCE = 20;
        private const float STUCK_THRESHOLD = 700.0f;
        private const int MOVEMENT_HISTORY_LENGTH = 50;

        private MapScene mapScene;
        private Actor follower;

        private Behavior behavior = Behavior.Idling;
        private float stuckDetection;

        private Vector2 lastPosition;
        private PathingController pathingController;

        private List<Vector3> movementHistory = new List<Vector3>();

        public FollowerController(MapScene iMapScene, Actor iFollower)
            : base(PriorityLevel.GameLevel)
        {
            mapScene = iMapScene;
            follower = iFollower;
        }

        public override void PreUpdate(GameTime gameTime)
        {
            PlayerController humanController = mapScene.ControllerStack[(int)PriorityLevel.GameLevel].Find(x => x is PlayerController) as PlayerController;
            if (humanController == null) return;

            Actor humanPlayer = humanController.Player;
            switch (behavior)
            {
                case Behavior.Idling: IdlingAI(humanPlayer); break;
                case Behavior.Regrouping: RegroupingAI(gameTime, humanPlayer); break;
                case Behavior.Stuck: StuckAI(gameTime, humanPlayer); break;
            }
        }

        private void IdlingAI(Actor humanPlayer)
        {
            movementHistory.Clear();

            if (Vector2.Distance(humanPlayer.Position, follower.Position) > START_REGROUP_DISTANCE)
            {
                behavior = Behavior.Regrouping;
                stuckDetection = 0;
                lastPosition = follower.Position;
                return;
            }

            follower.Idle();
        }

        private void RegroupingAI(GameTime gameTime, Actor humanPlayer)
        {
            if (follower.BlockedDisplacement.Length() > 1) stuckDetection += follower.BlockedDisplacement.Length() * gameTime.ElapsedGameTime.Milliseconds;
            else stuckDetection = 0;
            if (stuckDetection > STUCK_THRESHOLD)
            {
                behavior = Behavior.Stuck;

                pathingController = new PathingController(PriorityLevel.GameLevel, mapScene.Tilemap, follower, humanPlayer, PlayerController.WALKING_SPEED);
                mapScene.AddController(pathingController);

                follower.Idle();
                return;
            }

            if (Vector2.Distance(humanPlayer.Position, follower.Position) < END_REGROUP_DISTANCE)
            {
                behavior = Behavior.Idling;

                follower.Idle();
            }
            else
            {
                movementHistory.Add(new Vector3(humanPlayer.Center - follower.Center, gameTime.ElapsedGameTime.Milliseconds / 1000.0f));
                if (movementHistory.Count > MOVEMENT_HISTORY_LENGTH) movementHistory.RemoveAt(0);

                Vector2 movement = Vector2.Zero;
                foreach (Vector3 vector in movementHistory) movement += new Vector2(vector.X, vector.Y) * vector.Z;

                // TODO remove jitter

                if (movement.Length() < 0.001f) movement = Vector2.Zero;
                else movement.Normalize();
                follower.Walk(movement, PlayerController.WALKING_SPEED);
                lastPosition = follower.Position;
            }
        }

        public void StuckAI(GameTime gameTime, Actor humanPlayer)
        {
            if (Vector2.Distance(follower.Position, humanPlayer.Position) < END_REGROUP_DISTANCE)
            {
                behavior = Behavior.Idling;
                if (pathingController != null) pathingController.Terminate();
                pathingController = null;
                follower.Idle();
                return;
            }

            if (pathingController.PathingError) follower.Teleport(mapScene.Tilemap.GetNavNode(follower, humanPlayer).Center);
            if (pathingController.Terminated)
            {
                behavior = Behavior.Regrouping;
                stuckDetection = 0;
                pathingController = null;
                lastPosition = follower.Position;
            }
        }
    }
}
