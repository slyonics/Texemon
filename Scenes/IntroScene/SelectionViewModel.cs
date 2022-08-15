using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texemon.Main;
using Texemon.Models;
using Texemon.SceneObjects.Widgets;

namespace Texemon.Scenes.IntroScene
{
    public class SelectionViewModel : ViewModel
    {
        public SelectionViewModel(Scene iScene, GameView viewName)
            : base(iScene, PriorityLevel.GameLevel, viewName)
        {

        }

        public void Magic()
        {
            GameProfile.NewState();
            CrossPlatformGame.Transition(typeof(MapScene.MapScene), "City");
        }

        public void Technology()
        {
            GameProfile.NewState();
            CrossPlatformGame.Transition(typeof(MapScene.MapScene), "City");
        }
    }
}
