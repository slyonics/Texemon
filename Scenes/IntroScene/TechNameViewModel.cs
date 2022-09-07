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

        public TechNameViewModel(Scene iScene, GameView viewName)
            : base(iScene, PriorityLevel.MenuLevel, viewName)
        {
            namingBox = GetWidget<Textbox>("NamingBox");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.CurrentInput.CommandReleased(Command.Confirm) && !namingBox.Active)
            {
                Audio.PlaySound(GameSound.menu_select);
                GetWidget<Button>("OK").RadioSelect();
                Proceed();
            }
        }

        

        public void Proceed()
        {
            var hero = new HeroModel(ClassType.Android);
            GameProfile.PlayerProfile.Party.Add(hero);
            CrossPlatformGame.Transition(typeof(MapScene.MapScene), "HomeLab", 5, 7, SceneObjects.Maps.Orientation.Up);
        }

        public ModelProperty<bool> ReadyToProceed { get; set; } = new ModelProperty<bool>(false);
    }
}
