using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.Models;

namespace Texemon.Scenes.MatchScene
{
    public class GameOverViewModel : ViewModel
    {
        MatchScene MatchScene { get; set; }

        public GameOverViewModel(Scene iScene, GameView viewName)
            : base(iScene, PriorityLevel.CutsceneLevel, viewName)
        {
            MatchScene = iScene as MatchScene;
        }

        public void Retry()
        {
            //Terminate();
            MatchScene.Restart();
        }

        public void Quit()
        {
            //Terminate();
            MatchScene.EndMatch();
        }

        public ModelProperty<string> ConversationFont { get; set; } = new ModelProperty<string>(CrossPlatformGame.Scale == 1 ? "Tooltip" : "BigTooltip");
    }
}
