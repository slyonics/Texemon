using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.Models;
using Texemon.SceneObjects.Widgets;

namespace Texemon.Scenes.MatchScene
{
    public class ItemViewModel : ViewModel
    {
        private MatchScene matchScene;
        private DataGrid itemGrid;
        private int itemIndex = -1;
        private int cooldown = 50;

        public ItemViewModel(MatchScene iScene)
            : base(iScene, PriorityLevel.MenuLevel, GameView.MatchScene_ItemView)
        {
            matchScene = iScene;

            itemGrid = GetWidget<DataGrid>("ItemGrid");
            if (itemGrid.ChildList.Count > 0)
            {
                ((Button)itemGrid.ChildList.FirstOrDefault()).RadioSelect();
                itemIndex = 0;

            }
        }

        public override void Update(GameTime gameTime)
        {
            if (cooldown > 0) cooldown -= gameTime.ElapsedGameTime.Milliseconds;
            else if (itemIndex >= 0)
            {
                if (Input.CurrentInput.CommandPressed(Command.Up) && itemIndex > 0)
                {
                    itemIndex--;
                    ((Button)itemGrid.ChildList[itemIndex]).RadioSelect();
                    if (!itemGrid.IsChildVisible(itemGrid.ChildList[itemIndex])) itemGrid.ScrollUp();
                }
                else if (Input.CurrentInput.CommandPressed(Command.Down) && itemIndex < itemGrid.ChildList.Count - 1)
                {
                    itemIndex++;
                    ((Button)itemGrid.ChildList[itemIndex]).RadioSelect();
                    if (!itemGrid.IsChildVisible(itemGrid.ChildList[itemIndex])) itemGrid.ScrollDown();
                }
                else if (Input.CurrentInput.CommandPressed(Command.Confirm))
                {
                    ((Button)itemGrid.ChildList[itemIndex]).Activate();
                }
            }

            base.Update(gameTime);
        }

        public void UseItem(string itemName)
        {
            Close();

            ItemModel itemModel = GameProfile.PlayerProfile.Items.First(x => x.Value.Name == itemName).Value;
            itemModel.Quantity = itemModel.Quantity - 1;
            if (itemModel.Quantity == 0) GameProfile.PlayerProfile.Items.Remove(GameProfile.PlayerProfile.Items.First(x => x.Value.Name == itemName));

            AttackController attackController = new AttackController(matchScene, itemModel.Script);
            matchScene.AddController(attackController);

            matchScene.GameViewModel.CanPush.Value = false;
            matchScene.GameViewModel.FixSelection();
        }

        public void Back()
        {
            Close();
        }
    }
}
