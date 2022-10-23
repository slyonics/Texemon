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
    public class ItemViewModel : ViewModel, IStatusSubView
    {
        StatusScene statusScene;

        private int slot = -1;

        public ModelCollection<ItemRecord> AvailableItems { get => GameProfile.Inventory; }

        public bool SuppressCancel { get; set; }

        public ItemViewModel(StatusScene iScene)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;

            LoadView(GameView.StatusScene_ItemView);

            if (AvailableItems.Count() > 0)
            {
                slot = 0;
                SelectItem(AvailableItems[slot]);
                (GetWidget<DataGrid>("ItemList").ChildList[slot] as Button).RadioSelect();
            }

            Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (AvailableItems.Count() > 0)
            {
                if (Input.CurrentInput.CommandPressed(Command.Up)) CursorUp();
                else if (Input.CurrentInput.CommandPressed(Command.Down)) CursorDown();
                else if (Input.CurrentInput.CommandPressed(Command.Confirm))
                {
                    if (slot == -1)
                    {
                        Audio.PlaySound(GameSound.menu_select);
                        slot = 0;
                        (GetWidget<DataGrid>("ItemList").ChildList[slot] as Button).RadioSelect();
                        SelectItem(AvailableItems.First());
                    }
                }
            }
        }

        private void CursorUp()
        {
            if (slot == -1) return;

            slot--;
            if (slot < 0)
            {
                slot = 0;
                return;
            }
            
            Audio.PlaySound(GameSound.menu_select);

            SelectItem(AvailableItems[slot]);

            var itemGrid = GetWidget<DataGrid>("ItemList");
            (itemGrid.ChildList[slot] as Button).RadioSelect();
            if (!itemGrid.IsChildVisible(itemGrid.ChildList[slot])) itemGrid.ScrollUp();
        }

        private void CursorDown()
        {
            slot++;
            if (slot >= AvailableItems.Count())
            {
                slot = AvailableItems.Count() - 1;
                return;
            }
            
            Audio.PlaySound(GameSound.menu_select);
                        
            SelectItem(AvailableItems[slot]);

            var itemGrid = GetWidget<DataGrid>("ItemList");
            (itemGrid.ChildList[slot] as Button).RadioSelect();
            if (!itemGrid.IsChildVisible(itemGrid.ChildList[slot])) itemGrid.ScrollDown();
        }

        public void SelectItem(object parameter)
        {
            CommandRecord record;
            if (parameter is IModelProperty)
            {
                record = (CommandRecord)((IModelProperty)parameter).GetValue();
            }
            else record = (CommandRecord)parameter;

            slot = AvailableItems.ToList().FindIndex(x => x.Value == record);

            Description1.Value = record.Description.ElementAtOrDefault(0);
            Description2.Value = record.Description.ElementAtOrDefault(1);
            Description3.Value = record.Description.ElementAtOrDefault(2);
            Description4.Value = record.Description.ElementAtOrDefault(3);
            Description5.Value = record.Description.ElementAtOrDefault(4);
        }

        public void ResetSlot()
        {
            if (slot >= 0) (GetWidget<DataGrid>("ItemList").ChildList[slot] as Button).UnSelect();
            slot = -1;

            AvailableItems.ModelList = AvailableItems.ModelList;

            Description1.Value = "";
            Description2.Value = "";
            Description3.Value = "";
            Description4.Value = "";
            Description5.Value = "";
        }

        public void MoveAway()
        {

        }

        public bool SuppressLeftRight { get => false; }

        public ModelProperty<string> Description1 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description2 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description3 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description4 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description5 { get; set; } = new ModelProperty<string>("");
    }
}
