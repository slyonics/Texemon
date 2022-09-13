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
    public class ConstructViewModel : ViewModel
    {
        public static readonly Dictionary<string, Animation> HERO_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { "Idle", new Animation(0, 0, 24, 32, 4, 400) },
        };

        ShopScene shopScene;
        ShopRecord shopRecord;

        HeroNameViewModel heroNameViewModel;

        int slot = -1;

        public ConstructViewModel(ShopScene iScene, ShopRecord iShopRecord)
            : base(iScene, PriorityLevel.GameLevel)
        {
            shopScene = iScene;
            shopRecord = iShopRecord;

            foreach (VoucherRecord voucherRecord in shopRecord.Vouchers)
            {
                HeroRecord heroRecord = StatusScene.StatusScene.HEROES.First(x => x.Name.ToString() == voucherRecord.Name.Replace(" ", ""));

                voucherRecord.Sprite = new AnimatedSprite(AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Actors_" + heroRecord.Sprite)], HERO_ANIMATIONS);

                AvailableEntries.Add(voucherRecord);
            }

            

            LoadView(GameView.ShopScene_ConstructView);

            GetWidget<CrawlText>("Description").Text = shopRecord.Intro;

            /*
            if (!Input.MOUSE_MODE)
            {
                (GetWidget<DataGrid>("EntryList").ChildList[slot] as Button).RadioSelect();
                SelectEntry(AvailableEntries.ElementAt(slot));
            }
            */
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            //if (Input.MOUSE_MODE) slot = 0;

            var input = Input.CurrentInput;
            if (heroNameViewModel == null)
            {
                if (input.CommandPressed(Command.Left)) CursorLeft();
                else if (input.CommandPressed(Command.Right)) CursorRight();
                else if (input.CommandPressed(Command.Up)) CursorUp();
                else if (input.CommandPressed(Command.Down)) CursorDown();
                else if (input.CommandPressed(Command.Confirm)) CursorSelect();
                else if (input.CommandPressed(Command.Cancel)) Terminate();
            }
            else
            {
                if (input.CommandPressed(Command.Cancel)) heroNameViewModel.Terminate();
                if (heroNameViewModel.Terminated) heroNameViewModel = null;
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
            else if (slot == 0) slot = AvailableEntries.Count() - 1;
            else slot--;

            (GetWidget<DataGrid>("EntryList").ChildList[slot] as Button).RadioSelect();
            SelectEntry(AvailableEntries.ElementAt(slot));
        }

        private void CursorDown()
        {
            Audio.PlaySound(GameSound.menu_select);

            if (slot == -1) slot = 0;
            else if (slot == AvailableEntries.Count() - 1) slot = 0;
            else slot++;

            (GetWidget<DataGrid>("EntryList").ChildList[slot] as Button).RadioSelect();
            SelectEntry(AvailableEntries.ElementAt(slot));
        }

        private void CursorSelect()
        {
            if (slot == -1) return;

            Audio.PlaySound(GameSound.menu_select);

            CommandRecord record = (GetWidget<DataGrid>("EntryList").Items.ElementAt(slot) as IModelProperty).GetValue() as CommandRecord;

            //confirmViewModel = new ConfirmViewModel(battleScene, record);
            //shopScene.AddView(confirmViewModel);
        }

        public void SelectEntry(object parameter)
        {
            VoucherRecord record;
            if (parameter is IModelProperty)
            {
                record = (VoucherRecord)((IModelProperty)parameter).GetValue();
            }
            else record = (VoucherRecord)parameter;

            if (Input.MOUSE_MODE)
            {
                //confirmViewModel = new ConfirmViewModel(battleScene, record);
                //shopScene.AddView(confirmViewModel);
            }

            slot = AvailableEntries.ToList().FindIndex(x => x.Value == record);
            GetWidget<CrawlText>("Description").Text = record.Description;
            ReadyToProceed.Value = true;

            Cost.ModelList.Clear();
            foreach (CostRecord cost in record.Cost) Cost.Add(cost);

            /*
            Description1.Value = record.Description.ElementAtOrDefault(0);
            Description2.Value = record.Description.ElementAtOrDefault(1);
            Description3.Value = record.Description.ElementAtOrDefault(2);
            Description4.Value = record.Description.ElementAtOrDefault(3);
            Description5.Value = record.Description.ElementAtOrDefault(4);
            */
        }

        public void Proceed()
        {
            heroNameViewModel = shopScene.AddView(new HeroNameViewModel(shopScene, AvailableEntries[slot]));
        }

        public void Back()
        {
            Terminate();
        }


        public ModelCollection<VoucherRecord> AvailableEntries { get; set; } = new ModelCollection<VoucherRecord>();
        public ModelCollection<CostRecord> Cost { get; set; } = new ModelCollection<CostRecord>();

        public ModelProperty<bool> ReadyToProceed { get; set; } = new ModelProperty<bool>(false);
        public ModelProperty<bool> IsAffordable { get; set; } = new ModelProperty<bool>(true);

        public ModelProperty<string> Description1 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description2 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description3 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description4 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description5 { get; set; } = new ModelProperty<string>("");
    }
}
