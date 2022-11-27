using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

using MonsterLegends.Main;
using MonsterLegends.Models;
using MonsterLegends.SceneObjects.Widgets;
using MonsterLegends.Scenes.StatusScene;

namespace MonsterLegends.Scenes.IntroScene
{
    public class SelectionViewModel : ViewModel
    {
        public enum Selection
        {
            None,
            Magic,
            Technology
        }

        Selection selection = Selection.None;
        CrawlText description;

        TechNameViewModel techNameViewModel;

        public SelectionViewModel(Scene iScene, GameView viewName)
            : base(iScene, PriorityLevel.GameLevel, viewName)
        {
            description = GetWidget<CrawlText>("Description");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (techNameViewModel != null)
            {
                if (techNameViewModel.Terminated)
                {
                    techNameViewModel = null;
                    ReadyToProceed.Value = true;
                    GetWidget<Button>("OK").UnSelect();
                }
                else return;
            }

            /*
            if (Input.CurrentInput.CommandPressed(Command.Up))
            {
                Audio.PlaySound(GameSound.menu_select);
                GetWidget<Button>("Magic").RadioSelect();
                Magic();
            }
            else if (Input.CurrentInput.CommandPressed(Command.Down))
            {
                Audio.PlaySound(GameSound.menu_select);
                GetWidget<Button>("Technology").RadioSelect();
                Technology();
            }
            else if (Input.CurrentInput.CommandPressed(Command.Confirm) && selection != Selection.None)
            {
                GetWidget<Button>("OK").RadioSelect();
            }
            else if (Input.CurrentInput.CommandReleased(Command.Confirm) && selection != Selection.None)
            {
                Audio.PlaySound(GameSound.menu_select);
                Proceed();
            }
            */

        }

        public void Autostart()
        {
            GetWidget<Button>("Technology").RadioSelect();
            Technology();
            Proceed();
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
            GameProfile.PlayerProfile.Font.Value = GameFont.SandyForest;

            description.Text = "After the full potential of magic has been explored, the only way to progress is by expanding the limits of what is possible. A new discipline of magic offers a vision of the future inspired by robotics technology, but sparks controversy among existing disciplines.";
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

            description.Text = "A revolutionary android prototype with the capacity for magic is salvaged from a doomed world and may hold the potential to saving it. Seeking their missing components, the android embarks on a journey to the laboratory where they were designed.";
        
        }

        public void Proceed()
        {
            switch (selection)
            {
                case Selection.Magic:
                    techNameViewModel = parentScene.AddView(new TechNameViewModel(parentScene, GameView.IntroScene_TechNameView, Selection.Magic));
                    ReadyToProceed.Value = false;                    
                    break;

                case Selection.Technology:
                    techNameViewModel = parentScene.AddView(new TechNameViewModel(parentScene, GameView.IntroScene_TechNameView, Selection.Technology));
                    ReadyToProceed.Value = false;
                    break;
            }
        }

        public ModelProperty<bool> ReadyToProceed { get; set; } = new ModelProperty<bool>(false);
    }
}
