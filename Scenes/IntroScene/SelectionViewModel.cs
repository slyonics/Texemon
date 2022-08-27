using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texemon.Main;
using Texemon.Models;
using Texemon.SceneObjects.Widgets;
using Texemon.Scenes.StatusScene;

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
            if (selection == Selection.Magic) return;

            selection = Selection.Magic;
            ReadyToProceed.Value = true;
                        
            GameProfile.PlayerProfile.WindowStyle.Value = "MagicWindow";
            GameProfile.PlayerProfile.FrameStyle.Value = "MagicFrame";
            GameProfile.PlayerProfile.SelectedStyle.Value = "MagicSelected";
            GameProfile.PlayerProfile.FrameSelectedStyle.Value = "MagicFrameSelected";
            GameProfile.PlayerProfile.LabelStyle.Value = "MagicLabel";
            GameProfile.PlayerProfile.Font.Value = GameFont.Silver;

            description.Text = "The use of magic is strictly regulated by the secret authorities of Panopticon where sorcerers may practice only one color of magic. You alone defy this edict to study every color of magic in your quest for power.";
        }

        public void Technology()
        {
            if (selection == Selection.Technology) return;

            selection = Selection.Technology;
            ReadyToProceed.Value = true;

            GameProfile.PlayerProfile.WindowStyle.Value = "TechWindow";
            GameProfile.PlayerProfile.FrameStyle.Value = "TechFrame";
            GameProfile.PlayerProfile.SelectedStyle.Value = "TechSelected";
            GameProfile.PlayerProfile.FrameSelectedStyle.Value = "TechFrameSelected";
            GameProfile.PlayerProfile.LabelStyle.Value = "TechLabel";
            GameProfile.PlayerProfile.Font.Value = GameFont.Pixel;

            description.Text = "When the Castle Corporation ordered you to destroy your revolutionary AI you fled with the last surviving backup. Now, in the twilight of civilization, you return with your greatest creation so that she may realize her true destiny.";
        }

        public void Proceed()
        {
            switch (selection)
            {
                case Selection.Magic:
                    
                    CrossPlatformGame.Transition(typeof(MapScene.MapScene), "City");
                    break;

                case Selection.Technology:
                    GameProfile.PlayerProfile.Party.Add(new HeroModel(ClassType.Inventor));
                    GameProfile.PlayerProfile.Party.Add(new HeroModel(ClassType.Android));
                    CrossPlatformGame.Transition(typeof(MapScene.MapScene), "City");
                    break;
            }
        }

        public ModelProperty<bool> ReadyToProceed { get; set; } = new ModelProperty<bool>(false);
    }
}
