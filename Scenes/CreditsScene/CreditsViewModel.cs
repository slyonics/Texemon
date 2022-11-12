using System;
using System.Collections.Generic;
using System.Text;
using Texemon.Main;

namespace Texemon.Scenes.CreditsScene
{
    public class CreditsViewModel : ViewModel
    {
        public CreditsViewModel(Scene iScene, GameView viewName)
            : base(iScene, PriorityLevel.GameLevel, viewName)
        {

        }

        public void Back()
        {
            //CrossPlatformGame.Transition(typeof(TitleScene.TitleScene));
            Close();
        }
    }
}
