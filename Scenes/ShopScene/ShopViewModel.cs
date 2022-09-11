using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

using Texemon.Models;
using Texemon.SceneObjects.Widgets;
using Texemon.Scenes.StatusScene;

namespace Texemon.Scenes.ShopScene
{
    public class ShopViewModel : ViewModel
    {
        ShopScene shopScene;
        ShopRecord shopRecord;

        ConfirmViewModel confirmViewModel;

        int slot = -1;

        public ShopViewModel(ShopScene iScene, ShopRecord iShopRecord)
            : base(iScene, PriorityLevel.GameLevel)
        {
            shopScene = iScene;
            shopRecord = iShopRecord;

            foreach (VoucherRecord voucherRecord in shopRecord.Vouchers)
            {
                AvailableVouchers.Add(voucherRecord);
            }

            LoadView(GameView.ShopScene_ShopView);

            if (!Input.MOUSE_MODE)
            {
                (GetWidget<DataGrid>("CommandList").ChildList[slot] as Button).RadioSelect();
                SelectCommand(AvailableVouchers.ElementAt(slot));
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.MOUSE_MODE) slot = -1;

            var input = Input.CurrentInput;
            if (confirmViewModel == null)
            {
                if (input.CommandPressed(Command.Left)) CursorLeft();
                else if (input.CommandPressed(Command.Right)) CursorRight();
                else if (input.CommandPressed(Command.Up)) CursorUp();
                else if (input.CommandPressed(Command.Down)) CursorDown();
                else if (input.CommandPressed(Command.Confirm)) CursorSelect();
            }
            else
            {
                if (input.CommandPressed(Command.Cancel) && slot == -1) slot = 0;
                if (confirmViewModel.Terminated) confirmViewModel = null;
            }
        }

        private void CursorLeft()
        {
            
        }

        private void CursorRight()
        {
            
        }

        private void CursorUp()
        {
            Audio.PlaySound(GameSound.menu_select);

            if (slot == -1) slot = 0;
            else if (slot == 0) slot = AvailableVouchers.Count() - 1;
            else slot--;

            (GetWidget<DataGrid>("CommandList").ChildList[slot] as Button).RadioSelect();
            SelectCommand(AvailableVouchers.ElementAt(slot));
        }

        private void CursorDown()
        {
            Audio.PlaySound(GameSound.menu_select);

            if (slot == -1) slot = 0;
            else if (slot == AvailableVouchers.Count() - 1) slot = 0;
            else slot++;

            (GetWidget<DataGrid>("CommandList").ChildList[slot] as Button).RadioSelect();
            SelectCommand(AvailableVouchers.ElementAt(slot));
        }

        private void CursorSelect()
        {
            if (slot == -1) return;

            Audio.PlaySound(GameSound.menu_select);

            CommandRecord record = (GetWidget<DataGrid>("CommandList").Items.ElementAt(slot) as IModelProperty).GetValue() as CommandRecord;
            //confirmViewModel = new ConfirmViewModel(battleScene, record);
            shopScene.AddView(confirmViewModel);
        }

        public void SelectCommand(object parameter)
        {
            VoucherRecord record;
            if (parameter is IModelProperty)
            {
                record = (VoucherRecord)((IModelProperty)parameter).GetValue();
            }
            else record = (VoucherRecord)parameter;

            confirmViewModel?.Terminate();
            confirmViewModel = null;

            if (Input.MOUSE_MODE)
            {
                //confirmViewModel = new ConfirmViewModel(battleScene, record);
                shopScene.AddView(confirmViewModel);
            }


            /*
            Description1.Value = record.Description.ElementAtOrDefault(0);
            Description2.Value = record.Description.ElementAtOrDefault(1);
            Description3.Value = record.Description.ElementAtOrDefault(2);
            Description4.Value = record.Description.ElementAtOrDefault(3);
            Description5.Value = record.Description.ElementAtOrDefault(4);
            */
        }


        public ModelCollection<VoucherRecord> AvailableVouchers { get; set; } = new ModelCollection<VoucherRecord>();

        public ModelProperty<string> Description1 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description2 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description3 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description4 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description5 { get; set; } = new ModelProperty<string>("");
    }
}
