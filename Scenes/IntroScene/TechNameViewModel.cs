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
    public class TechNameViewModel : ViewModel
    {
        enum Selection
        {
            None,
            Magic,
            Technology
        }

        Textbox namingBox;

        int confirmCooldown = 100;

        public TechNameViewModel(Scene iScene, GameView viewName)
            : base(iScene, PriorityLevel.MenuLevel, viewName)
        {
            namingBox = GetWidget<Textbox>("NamingBox");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.CurrentInput.CommandPressed(Command.Cancel))
            {
                Terminate();
            }
            else if (Input.CurrentInput.CommandPressed(Command.Confirm) && confirmCooldown <= 0)
            {
                GetWidget<Button>("OK").RadioSelect();
                namingBox.Active = false;
            }
            else if (Input.CurrentInput.CommandReleased(Command.Confirm) && confirmCooldown <= 0)
            {
                Audio.PlaySound(GameSound.menu_select);
                GetWidget<Button>("OK").UnSelect();
                Proceed();
            }

            if (confirmCooldown > 0) confirmCooldown -= gameTime.ElapsedGameTime.Milliseconds;
        }

        public void Proceed()
        {
            namingBox.Active = false;

            GameProfile.SetSaveData<bool>("NewTechGame", true);

            var hero = new HeroModel(HeroType.TechHero);
            hero.Name.Value = namingBox.Text;
            GameProfile.PlayerProfile.Party.Add(hero);

            GameProfile.SetSaveData<HeroModel>("PartyLeader", hero);
            GameProfile.SetSaveData<string>("WindowStyle", GameProfile.PlayerProfile.WindowStyle.Value);

            CrossPlatformGame.Transition(typeof(MapScene.MapScene), "HomeLab", 5, 7, SceneObjects.Maps.Orientation.Up);
        }

        public ModelProperty<bool> ReadyToProceed { get; set; } = new ModelProperty<bool>(false);
    }
}
