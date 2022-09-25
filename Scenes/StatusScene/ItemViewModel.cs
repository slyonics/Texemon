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

        private int slot = -1;

        public ModelCollection<ItemRecord> AvailableItems { get => GameProfile.Inventory; }

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
            }
        }

        private void CursorUp()
        {
            slot--;
            if (slot < 0)
            {
                slot = 0;
                return;
            }
            
            Audio.PlaySound(GameSound.Cursor);

            SelectItem(AvailableItems[slot]);
            (GetWidget<DataGrid>("ItemList").ChildList[slot] as Button).RadioSelect();
        }

        private void CursorDown()
        {
            slot++;
            if (slot >= AvailableItems.Count())
            {
                slot = AvailableItems.Count() - 1;
                return;
            }
            
            Audio.PlaySound(GameSound.Cursor);

            SelectItem(AvailableItems[slot]);
            (GetWidget<DataGrid>("ItemList").ChildList[slot] as Button).RadioSelect();
        }

        public void SelectItem(object parameter)
        {
            CommandRecord record;
            if (parameter is IModelProperty)
            {
                record = (CommandRecord)((IModelProperty)parameter).GetValue();
            }
            else record = (CommandRecord)parameter;

            Description1.Value = record.Description.ElementAtOrDefault(0);
            Description2.Value = record.Description.ElementAtOrDefault(1);
            Description3.Value = record.Description.ElementAtOrDefault(2);
            Description4.Value = record.Description.ElementAtOrDefault(3);
            Description5.Value = record.Description.ElementAtOrDefault(4);
        }

        public ModelProperty<string> Description1 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description2 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description3 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description4 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description5 { get; set; } = new ModelProperty<string>("");
    }
}
