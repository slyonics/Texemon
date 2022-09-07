using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

using TiledCS;

using Texemon.SceneObjects.Maps;

namespace Texemon.Scenes.MapScene
{
    public class Npc : Actor, IInteractive
    {
        protected enum NpcAnimation
        {
            IdleDown,
            IdleLeft,
            IdleRight,
            IdleUp,
            WalkDown,
            WalkLeft,
            WalkRight,
            WalkUp
        }

        public const int NPC_WIDTH = 24;
        public const int NPC_HEIGHT = 32;

        public static readonly Rectangle NPC_BOUNDS = new Rectangle(-7, -8, 13, 6);

        private static readonly Dictionary<string, Animation> NPC_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { NpcAnimation.IdleDown.ToString(), new Animation(1, 0, NPC_WIDTH, NPC_HEIGHT, 1, 1000) },
            { NpcAnimation.IdleLeft.ToString(), new Animation(1, 1, NPC_WIDTH, NPC_HEIGHT, 1, 1000) },
            { NpcAnimation.IdleRight.ToString(), new Animation(1, 2, NPC_WIDTH, NPC_HEIGHT, 1, 1000) },
            { NpcAnimation.IdleUp.ToString(), new Animation(1, 3, NPC_WIDTH, NPC_HEIGHT, 1, 1000) },
            { NpcAnimation.WalkDown.ToString(), new Animation(0, 0, NPC_WIDTH, NPC_HEIGHT, 4, 240) },
            { NpcAnimation.WalkLeft.ToString(), new Animation(0, 1, NPC_WIDTH, NPC_HEIGHT, 4, 240) },
            { NpcAnimation.WalkRight.ToString(), new Animation(0, 2, NPC_WIDTH, NPC_HEIGHT, 4, 240) },
            { NpcAnimation.WalkUp.ToString(), new Animation(0, 3, NPC_WIDTH, NPC_HEIGHT, 4, 240) }
        };

        private MapScene mapScene;

        private string[] interactionScript = null;

        public Npc(MapScene iMapScene, Tilemap iTilemap, TiledObject tiledObject, string spriteName, Orientation iOrientation = Orientation.Down)
            : base(iMapScene, iTilemap, new Vector2(), NPC_BOUNDS, iOrientation)
        {
            mapScene = iMapScene;

            string sprite = "";
            foreach (TiledProperty tiledProperty in tiledObject.properties)
            {
                switch (tiledProperty.name)
                {
                    case "Behavior": Behavior = tiledProperty.value.Split('\n'); break;
                    case "Interact": interactionScript = tiledProperty.value.Split('\n'); break;
                    case "Label": Label = tiledProperty.value; break;
                    case "Direction": Orientation = (Orientation)Enum.Parse(typeof(Orientation), tiledProperty.value); break;
                    case "Sprite":
                        sprite = tiledProperty.value;
                        break;
                }
            }

            animatedSprite = new AnimatedSprite(AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Actors_" + sprite)], NPC_ANIMATIONS);
            CenterOn(iTilemap.GetTile(new Vector2(tiledObject.x + tiledObject.width / 2, tiledObject.y + tiledObject.height)).Center);

            Idle();
        }

        public bool Activate(Hero activator)
        {
            if (interactionScript == null) return false;

            Rectangle areaOfInterest = Rectangle.Union(SpriteBounds, mapScene.PartyLeader.SpriteBounds);
            EventController eventController = new EventController(mapScene, interactionScript);

            mapScene.AddController(eventController);
            controllerList.Add(eventController);

            Reorient(activator.Center - Center);
            OrientedAnimation("Idle");

            return true;
        }

        public string Label { get; private set; } = "NPC";
        public string[] Behavior { get; private set; } = null;
        public Vector2 LabelPosition { get => new Vector2(position.X, position.Y - animatedSprite.SpriteBounds().Height); }
        public bool Interactive { get => interactionScript != null; }
    }
}
