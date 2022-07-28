using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texemon.Models;

namespace Texemon.Scenes.MapScene
{
    public class ItemViewModel : ViewModel
    {
        public ItemViewModel(Scene iScene) : base(iScene, PriorityLevel.MenuLevel, GameView.MapScene_ItemView) { mapScene = iScene as MapScene; }

        private MapScene mapScene;



        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.CurrentInput.CommandPressed(Command.Menu))
            {
                Terminate();
            }
        }

        public void Back()
        {
            Terminate();
        }

        public ModelProperty<string> ConversationFont { get; set; } = new ModelProperty<string>(CrossPlatformGame.Scale == 1 ? "Tooltip" : "BigTooltip");
    }
}
