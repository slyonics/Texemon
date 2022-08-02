using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Panderling.GameObjects.Controllers;
using Panderling.Procedural;
using Panderling.Scenes;
using Panderling.UserInterface.Controllers;
using Panderling.UserInterface.Views;

using GameData;

namespace Panderling.GameObjects.Actors
{
    public enum VillagerType
    {
        Millie,
        Snowbird
    }

    public class Villager : Actor, IInteractive
    {
        private const double REQUIRED_TOKEN_WEIGHT = 1.0;
        private const double BONUS_TOKEN_WEIGHT = 1.5;

        private static Texture2D[] spriteArray = new Texture2D[Enum.GetNames(typeof(VillagerType)).Length];
        private static Texture2D[] shadows = new Texture2D[Enum.GetNames(typeof(VillagerType)).Length];
        private static Rectangle[,] boundingBoxes = new Rectangle[Enum.GetNames(typeof(VillagerType)).Length, 2];
        private static Dictionary<string, Animation>[] animations = new Dictionary<string, Animation>[Enum.GetNames(typeof(VillagerType)).Length];

        private string name;
        private VillagerType villagerType;
        private int villagerId;
        private VillagerData villagerData;
        private ScheduleData scheduleData;

        private bool activated;

        public Villager(MapScene iMapScene, Vector2 iPosition, ScheduleData iScheduleData, VillagerType iVillagerType)
            : base(iMapScene, iPosition, spriteArray[(int)iVillagerType], animations[(int)iVillagerType], boundingBoxes[(int)iVillagerType, 0], Orientation.Left)
        {
            villagerType = iVillagerType;
            villagerId = (int)villagerType;
            name = Enum.GetName(typeof(VillagerType), villagerType);
            villagerData = VillagerData.Data.First(x => x.name == name);
            scheduleData = iScheduleData;

            shadow = shadows[villagerId];
        }

        public static new void LoadContent(ContentManager contentManager)
        {
            foreach (int villagerId in Enum.GetValues(typeof(VillagerType)))
            {
                string villagerName = Enum.GetName(typeof(VillagerType), villagerId);
                VillagerData villagerData = VillagerData.Data.First(x => x.name == villagerName);

                spriteArray[villagerId] = contentManager.Load<Texture2D>("Graphics/Villagers/" + villagerName);

                int originX = villagerData.width / 2;
                int originY = villagerData.height;
                boundingBoxes[villagerId, 0] = new Rectangle(villagerData.boundsLeft - originX, villagerData.boundsTop - originY, villagerData.boundsRight - villagerData.boundsLeft, villagerData.boundsBottom - villagerData.boundsTop);
                boundingBoxes[villagerId, 1] = new Rectangle((villagerData.width - villagerData.boundsRight) - originX, villagerData.boundsTop - originY, villagerData.boundsRight - villagerData.boundsLeft, villagerData.boundsBottom - villagerData.boundsTop);

                animations[villagerId] = new Dictionary<string, Animation>();
                foreach (VillagerAnimation villagerAnimation in villagerData.villagerAnimations)
                {
                    Animation animation = new Animation(villagerAnimation.cellX, villagerAnimation.cellY, villagerData.width, villagerData.height, villagerAnimation.frameLengths.Length, villagerAnimation.frameLengths, spriteArray[villagerId].Width);
                    animations[villagerId].Add(villagerAnimation.name, animation);
                }

                shadows[villagerId] = BuildShadow(boundingBoxes[villagerId, 0]);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            activated = false;
        }

        public bool Activate(Player activator)
        {
            activated = true;

            if (villagerData.facePlayer) Reorient(activator.Center - Center);

            string script = scheduleData.activateScript;
            Rectangle areaOfInterest = Rectangle.Union(SpriteBounds, activator.SpriteBounds);
            if (script == null || script == "")
            {
                IEnumerable<VillagerDialogue> validDialogues = villagerData.dialogueData.Where(x => IsDialogueValid(x, scheduleData.conversationTags));
                Dictionary<VillagerDialogue, double> weightedDialogues = validDialogues.ToDictionary(dialogue => dialogue, dialogue => ScoreDialogue(dialogue, scheduleData.conversationTags));

                string text = Rng.WeightedEntry<VillagerDialogue>(weightedDialogues).text;
                DialogueView dialogueView = new DialogueView(mapScene, areaOfInterest, name, text);
                parentScene.AddView(dialogueView);

                DialogueController dialogueController;
                dialogueController = new DialogueController(PriorityLevel.CutsceneLevel, dialogueView);
                parentScene.AddController(dialogueController);

                controllerList.Add(dialogueController);
            }
            else
            {
                EventController eventController = new EventController(PriorityLevel.CutsceneLevel, mapScene, areaOfInterest, script);
                mapScene.AddController(eventController);

                controllerList.Add(eventController);
            }

            return false;
        }

        public override void Reorient(Vector2 movement)
        {
            Rectangle oldBoundingBox = boundingBox;

            if (movement.X > 0.001f) orientation = Orientation.Right;
            else if (movement.X < -0.001f) orientation = Orientation.Left;

            animatedSprite.SpriteEffects = (orientation == Orientation.Left) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            boundingBox = boundingBoxes[(int)villagerType, (orientation == Orientation.Left) ? 0 : 1];
            position.X -= (boundingBox.Center.X - oldBoundingBox.Center.X);
            currentBounds = UpdateBounds(position);
        }

        public bool IsDialogueValid(VillagerDialogue villagerDialogue, string scheduleTags)
        {
            List<string> requiredHashtags = villagerDialogue.requiredTags.Split(' ').Where(x => x != "" && x[0] == '#').ToList();
            List<string> prohibitedHashtags = villagerDialogue.prohibitedTags.Split(' ').Where(x => x != "" && x[0] == '#').ToList();
            string[] scheduleHashtags = scheduleTags.Split(' ');

            if (requiredHashtags.Exists(x => !scheduleHashtags.Contains(x))) return false;
            if (prohibitedHashtags.Exists(x => scheduleHashtags.Contains(x))) return false;
            
            List<string> requiredVariables = villagerDialogue.requiredTags.Split(' ').Where(x => x != "" && x[0] == '$').ToList();
            foreach (string variable in requiredVariables) if (!ParseBool(variable)) return false;

            List<string> prohibitedVariables = villagerDialogue.prohibitedTags.Split(' ').Where(x => x != "" && x[0] == '$').ToList();
            foreach (string variable in prohibitedVariables) if (ParseBool(variable)) return false;

            return true;
        }

        public double ScoreDialogue(VillagerDialogue villagerDialogue, string scheduleTags)
        {
            IEnumerable<string> requiredHashtags = villagerDialogue.requiredTags.Split(' ').Where(x => x != "" && x[0] == '#');
            IEnumerable<string> bonusHashtags = villagerDialogue.bonusTags.Split(' ').Where(x => x != "" && x[0] == '#');            

            List<string> requiredVariables = villagerDialogue.requiredTags.Split(' ').Where(x => x != "" && x[0] == '$').ToList();
            List<string> bonusVariables = villagerDialogue.bonusTags.Split(' ').Where(x => x != "" && x[0] == '$').ToList();

            List<string> scheduleHashtags = scheduleTags.Split(' ').ToList();

            return (requiredHashtags.Count(x => scheduleHashtags.Contains(x)) + requiredVariables.Count(x => ParseBool(x))) * REQUIRED_TOKEN_WEIGHT +
                   (bonusHashtags.Count(x => scheduleHashtags.Contains(x)) + bonusVariables.Count(x => ParseBool(x))) * BONUS_TOKEN_WEIGHT;
        }

        public bool ParseBool(string variable)
        {
            switch (variable)
            {
                case "$day": return mapScene.GameMap.Weather.IsDay;
                case "$night": return mapScene.GameMap.Weather.IsNight;
            }

            return false;
        }

        public string Label { get => name; }
        public Vector2 LabelPosition { get => new Vector2(position.X, position.Y - villagerData.height); }
        public ScheduleData ScheduleData { get => scheduleData; }
        public bool Activated { get => activated; }
    }
}
