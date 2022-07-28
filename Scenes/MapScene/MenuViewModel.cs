using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texemon.Models;

namespace Texemon.Scenes.MapScene
{
    public class MenuViewModel : ViewModel
    {
        public MenuViewModel(Scene iScene) : base(iScene, PriorityLevel.MenuLevel, GameView.MapScene_MenuView) { mapScene = iScene as MapScene; }

        private int cooldown = 500;

        private MapScene mapScene;

        private ViewModel subMenu = null;

        public override void Update(GameTime gameTime)
        {
            if (subMenu == null)
            {
                base.Update(gameTime);

                if (cooldown > 0) cooldown -= gameTime.ElapsedGameTime.Milliseconds;

                if (Input.CurrentInput.CommandPressed(Command.Menu) && cooldown <= 0)
                {
                    Terminate();
                }
            }
            else if (subMenu.Terminated)
            {
                subMenu = null;
                cooldown = 500;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (subMenu == null || subMenu.Terminated) base.Draw(spriteBatch);
        }

        public void Items()
        {
            subMenu = mapScene.AddView(new ItemViewModel(mapScene));
        }

        public void Quit()
        {
            mapScene.SaveData();

            CrossPlatformGame.Transition(typeof(TitleScene.TitleScene));
        }

        public ModelProperty<string> ConversationFont { get; set; } = new ModelProperty<string>(CrossPlatformGame.Scale == 1 ? "Tooltip" : "BigTooltip");
    }
}
