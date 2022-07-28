using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texemon.Main;
using Texemon.Models;
using Texemon.SceneObjects;

namespace Texemon.Scenes.ConversationScene
{
    public class ConversationScene : Scene
    {
        private ConversationModel conversationData;
        private Texture2D backgroundSprite;
        private ConversationViewModel conversationViewModel;
        private ConversationController conversationController;

        public ConversationScene(string conversationName)
            : base()
        {
            conversationData = ConversationModel.Models.FirstOrDefault(x => x.Name == conversationName);

            string[] conversationScript = conversationData.DialogueData[0].Script;
            if (conversationScript != null) RunScript(conversationData.DialogueData[0].Script);

            conversationViewModel = new ConversationViewModel(this, GameView.ConversationScene_ConversationView, conversationData);
            AddOverlay(conversationViewModel);

            if (!string.IsNullOrEmpty(conversationData.Background))
                backgroundSprite = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Background_" + conversationData.Background)];
        }

        public ConversationScene(ConversationModel iConversationData)
            : base()
        {
            conversationData = iConversationData;

            string[] conversationScript = conversationData.DialogueData[0].Script;
            if (conversationScript != null) RunScript(conversationData.DialogueData[0].Script);

            conversationViewModel = new ConversationViewModel(this, GameView.ConversationScene_ConversationView, conversationData);
            AddOverlay(conversationViewModel);

            if (!string.IsNullOrEmpty(conversationData.Background))
                backgroundSprite = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Background_" + conversationData.Background)];
        }

        public override void BeginScene()
        {
            sceneStarted = true;

            if (!string.IsNullOrEmpty(conversationData.Background))
            {
                base.BeginScene();

                Audio.PlayMusic(GameMusic.SMP_DUN);
            }
        }

        public override void Update(GameTime gameTime, PriorityLevel priorityLevel = PriorityLevel.GameLevel)
        {
            base.Update(gameTime, priorityLevel);

            portraits.RemoveAll(x => x.Terminated);
        }

        public override void DrawBackground(SpriteBatch spriteBatch)
        {
            if (backgroundSprite != null)
                spriteBatch.Draw(backgroundSprite, new Rectangle(0, 0, CrossPlatformGame.ScreenWidth, CrossPlatformGame.ScreenHeight), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
        }

        public void FinishDialogue()
        {
            foreach (Portrait portrait in portraits)
            {
                portrait.FinishTransition();
            }
        }

        public void AddPortrait(Portrait portrait)
        {
            portraits.Add(portrait);
            AddEntity(portrait);
        }

        public void RunScript(string[] script)
        {
            conversationController = AddController(new ConversationController(this, script));
        }

        public bool IsScriptRunning()
        {
            return conversationController != null && !conversationController.Terminated;
        }

        private List<Portrait> portraits = new List<Portrait>();
        public List<Portrait> Portraits { get => portraits; }
        public ConversationViewModel ConversationViewModel { get => conversationViewModel; }

        public bool EndGame { get => conversationController.EndGame; }
    }
}
