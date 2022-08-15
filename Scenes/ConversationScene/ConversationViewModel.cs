using System;
using System.Linq;

using Texemon.Main;
using Texemon.Models;
using Texemon.SceneObjects;
using Texemon.SceneObjects.Widgets;

namespace Texemon.Scenes.ConversationScene
{
    public class ConversationViewModel : ViewModel
    {
        private ConversationScene conversationScene;
        private ConversationModel conversationData;
        private DialogueModel currentDialogue;
        private int dialogueIndex;

        private CrawlText crawlText;

        public ConversationViewModel(ConversationScene iScene, GameView viewName, ConversationModel iConversationData)
            : base(iScene, PriorityLevel.GameLevel, viewName)
        {

            conversationScene = (parentScene as ConversationScene);
            conversationData = iConversationData;
            currentDialogue = conversationData.DialogueData[dialogueIndex];

            Speaker.Value = string.IsNullOrEmpty(currentDialogue.Speaker) ? "" : currentDialogue.Speaker;
            Dialogue.Value = currentDialogue.Text;

            crawlText = GetWidget<CrawlText>("ConversationText");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (crawlText.ReadyToProceed && !ReadyToProceed.Value)
            {
                if (!conversationScene.IsScriptRunning())
                {
                    ReadyToProceed.Value = true;
                }

                OnDialogueScrolled?.Invoke();
                OnDialogueScrolled = null;
            }

            if (!Closed && !ChildList.Any(x => x.Transitioning))
            {
                if (Input.CurrentInput.CommandPressed(Command.Confirm))
                {
                    if (!crawlText.ReadyToProceed)
                    {
                        crawlText.FinishText();
                        conversationScene.FinishDialogue();
                    }
                    else NextDialogue();
                }
            }

            if (terminated)
            {
                parentScene.EndScene();
            }
        }

        public override void LeftClickChild(Vector2 mouseStart, Vector2 mouseEnd, Widget clickWidget, Widget otherWidget)
        {
            switch (clickWidget.Name)
            {
                case "ConversationText":
                    if (!crawlText.ReadyToProceed)
                    {
                        crawlText.FinishText();
                        conversationScene.FinishDialogue();
                    }
                    else NextDialogue();
                    break;
            }
        }

        public void NextDialogue()
        {
            dialogueIndex++;

            if (dialogueIndex >= conversationData.DialogueData.Length)
            {
                if (conversationData.EndScript != null)
                {
                    ConversationController conversationController = conversationScene.AddController(new ConversationController(conversationScene, conversationData.EndScript));
                    conversationController.OnTerminated += EndConversation;
                }
                else EndConversation();

                return;
            }

            currentDialogue = conversationData.DialogueData[dialogueIndex];

            Dialogue.Value = currentDialogue.Text;
            Speaker.Value = string.IsNullOrEmpty(currentDialogue.Speaker) ? "" : currentDialogue.Speaker;

            ReadyToProceed.Value = false;

            if (currentDialogue.Script != null) conversationScene.RunScript(currentDialogue.Script);
        }

        private void EndConversation()
        {
            if (string.IsNullOrEmpty(conversationData.Background)) Close();
            else
            {
                if (conversationScene.EndGame) CrossPlatformGame.Transition(typeof(TitleScene.TitleScene));
                else CrossPlatformGame.Transition(typeof(TitleScene.TitleScene));
            }
        }

        public event Action OnDialogueScrolled;

        public ModelProperty<bool> ReadyToProceed { get; set; } = new ModelProperty<bool>(false);
        public ModelProperty<string> ConversationFont { get; set; } = new ModelProperty<string>("Adapa");
        public ModelProperty<string> Dialogue { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Speaker { get; set; } = new ModelProperty<string>("");
    }
}
