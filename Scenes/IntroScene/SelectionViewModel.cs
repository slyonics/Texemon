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
        enum Selection
        {
            None,
            Magic,
            Technology
        }

        Selection selection = Selection.None;

        public SelectionViewModel(Scene iScene, GameView viewName)
            : base(iScene, PriorityLevel.GameLevel, viewName)
        {

        }

        public void Magic()
        {
            selection = Selection.Magic;
            ReadyToProceed.Value = true;
        }

        public void Technology()
        {
            selection = Selection.Technology;
            ReadyToProceed.Value = true;
        }

        public void Proceed()
        {
            GameProfile.NewState();

            switch (selection)
            {
                case Selection.Magic: CrossPlatformGame.Transition(typeof(MapScene.MapScene), "City"); break;
                case Selection.Technology: CrossPlatformGame.Transition(typeof(MapScene.MapScene), "City"); break;
            }
        }

        public ModelProperty<bool> ReadyToProceed { get; set; } = new ModelProperty<bool>(false);
    }
}
