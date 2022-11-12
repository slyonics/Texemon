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

                if (GameProfile.GetSaveData<bool>(heroRecord.Name + "Recruited")) continue;

                voucherRecord.Sprite = new AnimatedSprite(AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Actors_" + heroRecord.Sprite)], HERO_ANIMATIONS);

                AvailableEntries.Add(voucherRecord);
            }

            

            LoadView(GameView.ShopScene_ConstructView);

            GetWidget<CrawlText>("Description").Text = shopRecord.Intro;

            foreach (Button button in GetWidget<DataGrid>("EntryList").ChildList)
            {
                /*
                if ((button.ChildList[1] as Label).Text == "Repair Drone")
                {
                    (button.ChildList[0] as Image).Sprite.SpriteColor = new Color(128, 128, 128, 255);
                    (button.ChildList[0] as Image).Sprite.AnimationSpeed = 0.0f;
                    button.Enabled = false;
                }
                */
            }

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
                if (input.CommandPressed(Command.Up)) CursorUp();
                else if (input.CommandPressed(Command.Down)) CursorDown();
                else if (input.CommandPressed(Command.Confirm)) StartCursorSelect();
                else if (input.CommandReleased(Command.Confirm)) EndCursorSelect();
                else if (input.CommandPressed(Command.Cancel)) GetWidget<Button>("Back").RadioSelect();
                else if (input.CommandReleased(Command.Cancel))
                {
                    Audio.PlaySound(GameSound.Back);
                    GetWidget<Button>("Back").UnSelect();
                    Terminate();
                }
            }
            else
            {
                if (heroNameViewModel.Terminated)
                {
                    if (heroNameViewModel.Confirmed != null)
                    {
                        ReadyToProceed.Value = false;
                        slot = -1;

                        AvailableEntries.ModelList = AvailableEntries.ModelList.FindAll(x => x.Value.Name.Replace(" ", "") != heroNameViewModel.heroRecord.Name.ToString());

                        GetWidget<CrawlText>("Description").Text = heroNameViewModel.Confirmed + " was added to the party.";
                    }
                    heroNameViewModel = null;
                }
            }
        }

        private void CursorUp()
        {
            if (AvailableEntries.Count() == 0) return;

            Audio.PlaySound(GameSound.menu_select);

            if (slot == -1) slot = 0;
            else if (slot == 0) slot = AvailableEntries.Count() - 1;
            else slot--;

            (GetWidget<DataGrid>("EntryList").ChildList[slot] as Button).RadioSelect();
            SelectEntry(AvailableEntries.ElementAt(slot));
        }

        private void CursorDown()
        {
            if (AvailableEntries.Count() == 0) return;

            Audio.PlaySound(GameSound.menu_select);

            if (slot == -1) slot = 0;
            else if (slot == AvailableEntries.Count() - 1) slot = 0;
            else slot++;

            (GetWidget<DataGrid>("EntryList").ChildList[slot] as Button).RadioSelect();
            SelectEntry(AvailableEntries.ElementAt(slot));
        }

        private void StartCursorSelect()
        {
            if (!ReadyToProceed.Value || !IsAffordable.Value) return;

            Audio.PlaySound(GameSound.menu_select);
            GetWidget<Button>("OK").RadioSelect();
        }

        private void EndCursorSelect()
        {
            if (!ReadyToProceed.Value || !IsAffordable.Value) return;

            GetWidget<Button>("OK").UnSelect();
            Proceed();
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

            bool affordable = true;
            var cost = new List<ModelProperty<CostRecord>>();
            foreach (CostRecord costRecord in record.Cost)
            {
                Color color = Color.IndianRed;
                int have = GameProfile.Inventory.Count(x => x.Value.Name == costRecord.Item);
                if (have >= 1) color = Color.White;
                else affordable = false;
                cost.Add(new ModelProperty<CostRecord>(new CostRecord() { Item = costRecord.Item, Icon = costRecord.Icon, CostColor = color, Have = have, Need = 1 }));
            }
            Cost.ModelList = cost;
            IsAffordable.Value = affordable;

            StatusScene.HeroRecord heroRecord = StatusScene.StatusScene.HEROES.First(x => x.Name.ToString() == record.Name.Replace(" ", ""));
            List<ModelProperty<AbilityRecord>> abilities = new List<ModelProperty<AbilityRecord>>();
            foreach (string abilityName in heroRecord.InitialAbilities)
            {
                ModelProperty<AbilityRecord> property = new ModelProperty<AbilityRecord>(StatusScene.StatusScene.ABILITIES.First(x => x.Name == abilityName));
                abilities.Add(property);
            }
            Abilities.ModelList = abilities;
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
        public ModelCollection<AbilityRecord> Abilities { get; set; } = new ModelCollection<AbilityRecord>();

        public ModelProperty<bool> ReadyToProceed { get; set; } = new ModelProperty<bool>(false);
        public ModelProperty<bool> IsAffordable { get; set; } = new ModelProperty<bool>(true);
    }
}
