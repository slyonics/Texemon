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
        CrawlText description;

        public SelectionViewModel(Scene iScene, GameView viewName)
            : base(iScene, PriorityLevel.GameLevel, viewName)
        {
            description = GetWidget<CrawlText>("Description");
        }

        public void Magic()
        {
            selection = Selection.Magic;
            ReadyToProceed.Value = true;

            description.AddLines("Mgaico!");
        }

        public void Technology()
        {
            selection = Selection.Technology;
            ReadyToProceed.Value = true;

            description.AddLines("When the Castle Corporation ordered you to destroy your revolutionary AI you fled with the last surviving backup. Now, in the twilight of civilization, you return with your greatest creation so that she may realize her true destiny.");
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
