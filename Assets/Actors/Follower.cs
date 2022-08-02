using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Panderling.GameObjects.Controllers;
using Panderling.GameObjects.Maps;
using Panderling.GameObjects.Particles;
using Panderling.Gameplay;
using Panderling.Main;
using Panderling.Scenes;

namespace Panderling.GameObjects.Actors
{
    public class Follower : Actor, IInteractive
    {
        protected enum PlayerAnimation
        {
            IdleUp,
            IdleRight,
            IdleDown,
            IdleLeft,
            WalkUp,
            WalkRight,
            WalkDown,
            WalkLeft
        }

        public enum FollowerType
        {
            Dog
        }

        public const int FOLLOWER_WIDTH = 26;
        public const int FOLLOWER_HEIGHT = 36;

        private const float WALKING_SPEED = 90.0f;

        private static Texture2D[] spriteArray = new Texture2D[Enum.GetNames(typeof(FollowerType)).Length];

        protected static Texture2D FOLLOWER_SHADOW;
        protected static readonly Rectangle FOLLOWER_BOUNDING_BOX = new Rectangle(-7, -10, 14, 9);
        private static readonly Dictionary<string, Animation> FOLLOWER_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { PlayerAnimation.IdleUp.ToString(), new Animation(0, 3, FOLLOWER_WIDTH, FOLLOWER_HEIGHT, 1, 1000) },
            { PlayerAnimation.IdleRight.ToString(), new Animation(0, 2, FOLLOWER_WIDTH, FOLLOWER_HEIGHT, 1, 1000) },
            { PlayerAnimation.IdleDown.ToString(), new Animation(0, 0, FOLLOWER_WIDTH, FOLLOWER_HEIGHT, 1, 1000) },
            { PlayerAnimation.IdleLeft.ToString(), new Animation(0, 1, FOLLOWER_WIDTH, FOLLOWER_HEIGHT, 1, 1000) },
            { PlayerAnimation.WalkUp.ToString(), new Animation(0, 3, FOLLOWER_WIDTH, FOLLOWER_HEIGHT, 4, 80) },
            { PlayerAnimation.WalkRight.ToString(), new Animation(0, 2, FOLLOWER_WIDTH, FOLLOWER_HEIGHT, 4, 80) },
            { PlayerAnimation.WalkDown.ToString(), new Animation(0, 0, FOLLOWER_WIDTH, FOLLOWER_HEIGHT, 4, 80) },
            { PlayerAnimation.WalkLeft.ToString(), new Animation(0, 1, FOLLOWER_WIDTH, FOLLOWER_HEIGHT, 4, 80) }
        };

        private static readonly Dictionary<FollowerType, Vector2[]> PETTING_OFFSETS = new Dictionary<FollowerType, Vector2[]>()
        {
            { FollowerType.Dog, new Vector2[] { new Vector2(7, -8), new Vector2(-8, -8) } }
        };

        private FollowerType followerType;

        public Follower(MapScene iMapScene, Vector2 iPosition, FollowerType iFollowerType, Orientation iOrientation)
            : base(iMapScene, iPosition, spriteArray[(int)iFollowerType], FOLLOWER_ANIMATIONS, FOLLOWER_BOUNDING_BOX, iOrientation)
        {
            shadow = FOLLOWER_SHADOW;
            followerType = iFollowerType;
        }

        public static new void LoadContent(ContentManager contentManager)
        {
            foreach (int spriteId in Enum.GetValues(typeof(FollowerType)))
            {
                spriteArray[spriteId] = contentManager.Load<Texture2D>("Graphics/" + Enum.GetName(typeof(FollowerType), spriteId));
            }

            FOLLOWER_SHADOW = BuildShadow(FOLLOWER_BOUNDING_BOX);
        }

        public override void Teleport(Vector2 destination)
        {
            AnimationParticle smokeParticle = new AnimationParticle(parentScene, position, AnimationType.Smoke);
            smokeParticle.Position = new Vector2(position.X, Center.Y + smokeParticle.SpriteBounds.Height / 2);
            parentScene.AddParticle(smokeParticle);

            base.Teleport(destination);
        }

        public bool Activate(Player activator)
        {
            PettingController pettingController = new PettingController(mapScene, activator, this);
            parentScene.AddController(pettingController);

            activator.ControllerList.Add(pettingController);
            controllerList.Add(pettingController);

            return true;
        }

        public bool Interactive { get => (mapScene.GameMap.Peaceful || mapScene.EnemyList.Count == 0) && NearbyColliders.Count == 0; }
        public string Label { get => Enum.GetName(typeof(FollowerType), followerType); }
        public Vector2 LabelPosition { get => new Vector2(position.X, position.Y - FOLLOWER_HEIGHT); }
        public Vector2 PettingOffset { get => PETTING_OFFSETS[followerType][orientation == Orientation.Right ? 0 : 1]; }
    }
}
