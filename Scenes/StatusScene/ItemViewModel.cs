using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

using Texemon.Models;
using Texemon.SceneObjects.Widgets;

namespace Texemon.Scenes.StatusScene
{
    public class ItemViewModel : ViewModel
    {
        StatusScene statusScene;

        public ModelCollection<ItemRecord> AvailableItems { get => GameProfile.Inventory; }

        public ItemViewModel(StatusScene iScene)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;

            //AvailableMenus.Add(new ItemViewModel(statusScene));


            LoadView(GameView.StatusScene_ItemView);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.CurrentInput.CommandPressed(Command.Cancel)) Terminate();
        }
    }
}
