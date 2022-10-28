﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

using TiledCS;

using Texemon.SceneObjects.Maps;

namespace Texemon.Scenes.MapScene
{
    public class ChestController : Controller
    {
        Chest chest;
        MapScene mapScene;

        public ChestController(MapScene iMapScene, Chest iChest)
            : base(PriorityLevel.CutsceneLevel)
        {
            mapScene = iMapScene;
            chest = iChest;
            chest.AnimatedSprite.PlayAnimation("Opening", ChestOpened);
        }

        public override void PreUpdate(GameTime gameTime)
        {
            chest.AnimatedSprite.Update(gameTime);
        }

        public void ChestOpened()
        {
            string item = chest.Item;
            if (item[0] == '$')
            {
                switch (item)
                {
                    case "$JunkWeapon":
                        switch (Rng.RandomInt(1, 4))
                        {
                            case 1: item = "Longsword"; break;
                            case 2: item = "Blowtorch"; break;
                            case 3: item = "Arc Welder"; break;
                            case 4: item = "Blaster"; break;
                        }
                        break;

                    case "$JunkConsumable": item = "Repair Kit"; break;

                    case "$JunkArmor":
                        switch (Rng.RandomInt(1, 2))
                        {
                            case 1: item = "Kevlar"; break;
                            case 2: item = "Kevlar"; break;
                        }
                        break;
                }
            }

            string[] script = new string[] { "GiveItem " + item, "SetFlag " + chest.Name + "Opened True" };
            EventController eventController = new EventController(mapScene, script);
            mapScene.AddController(eventController);

            Terminate();
        }
    }

    public class Chest : Npc
    {
        protected enum ChestAnimation
        {
            Closed,
            Opening,
            Open
        }


        public static readonly Rectangle CHEST_BOUNDS = new Rectangle(-7, -10, 13, 8);

        private static readonly Dictionary<string, Animation> CHEST_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { ChestAnimation.Closed.ToString(), new Animation(0, 0, NPC_WIDTH, NPC_HEIGHT, 1, 1000) },
            { ChestAnimation.Opening.ToString(), new Animation(new Rectangle[] { new Rectangle(0,32,24,32), new Rectangle(0, 64, 24, 32),new Rectangle(0, 96, 24, 32) }, new int[] { 200, 200, 50 }) },
            { ChestAnimation.Open.ToString(), new Animation(0, 3, NPC_WIDTH, NPC_HEIGHT, 1, 1000) }
        };

        private MapScene mapScene;

        public string Name { get; set; }
        public string Item { get; set; }

        public Chest(MapScene iMapScene, Tilemap iTilemap, TiledObject tiledObject, string spriteName)
            : base(iMapScene, iTilemap, tiledObject, spriteName, Orientation.Down)
        {
            mapScene = iMapScene;

            Label = "Take";

            Name = tiledObject.name;
            Item = tiledObject.properties.First(x => x.name == "Item").value;

            AnimatedSprite.AnimationList = CHEST_ANIMATIONS;

            CenterOn(Center + new Vector2(0, -9));

            if (Models.GameProfile.GetSaveData<bool>(tiledObject.name + "Opened"))
            {
                Opened = true;
                AnimatedSprite.PlayAnimation("Open");
            }
        }

        public override bool Activate(Hero activator)
        {
            ChestController eventController = new ChestController(mapScene, this);
            mapScene.AddController(eventController);
            controllerList.Add(eventController);

            Opened = true;

            Audio.PlaySound(GameSound.Chest);

            return true;
        }

        public bool Opened { get; set; }

        public override bool Interactive { get => !Opened; }
    }
}
