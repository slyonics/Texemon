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
    public class SwapEquipViewModel : ViewModel
    {
        public static readonly Dictionary<string, Animation> HERO_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { "Idle", new Animation(0, 0, 24, 32, 4, 400) }
        };

        StatusScene statusScene;
        EquipmentViewModel equipmentViewModel;

        private int equipmentSlot = -1;
        private int slot = 0;

        int confirmCooldown = 100;

        public ModelCollection<ItemRecord> AvailableItems { get; set; }

        public HeroModel HeroModel { get; private set; }
        public ModelProperty<AnimatedSprite> PlayerSprite { get; private set; }
        public ModelCollection<ItemRecord> EquipmentList { get; private set; }

        public SwapEquipViewModel(StatusScene iScene, EquipmentViewModel iEquipmentViewModel, HeroModel iHeroModel, AnimatedSprite iAnimatedSprite, ModelCollection<ItemRecord> iEquipList, int iEquipSlot)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;
            equipmentViewModel = iEquipmentViewModel;
            HeroModel = iHeroModel;
            PlayerSprite = new ModelProperty<AnimatedSprite>(iAnimatedSprite);
            equipmentSlot = iEquipSlot;

            EquipmentList = iEquipList;

            AvailableItems = new ModelCollection<ItemRecord>();
            if (EquipmentList[iEquipSlot].Name == "- Empty Slot -")
            {
                AvailableItems.Add(new ItemRecord()
                {
                    Icon = "Blank",
                    Name = " - CANCEL - ",
                    Charges = -1,
                    ChargesLeft = -1,
                    Description = new string[] { "", "Return to the", "previous menu", "", "" }
                });
            }
            else
            {
                AvailableItems.Add(new ItemRecord()
                {
                    Icon = "Blank",
                    Name = " - CANCEL - ",
                    Charges = -1,
                    ChargesLeft = -1,
                    Description = new string[] { "", "Return to the", "previous menu", "", "" }
                });

                AvailableItems.Add(new ItemRecord()
                {
                    Icon = "Blank",
                    Name = " - REMOVE - ",
                    Charges = -1,
                    ChargesLeft = -1,
                    Description = new string[] { "", "", "Unequip this slot", "", "" }
                });
            }

            AvailableItems.ModelList.AddRange(GameProfile.Inventory.Where(x => x.Value.ItemType == ItemType.Weapon || x.Value.ItemType == ItemType.Consumable));

            Description2.Value = AvailableItems.First().Value.Description[1];
            Description3.Value = AvailableItems.First().Value.Description[2];

            LoadView(GameView.StatusScene_SwapEquipView);

            (GetWidget<DataGrid>("EquipmentList").ChildList[equipmentSlot] as Button).RadioSelect();
            (GetWidget<DataGrid>("ItemList").ChildList[slot] as Button).RadioSelect();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.CurrentInput.CommandPressed(Command.Up)) CursorUp();
            else if (Input.CurrentInput.CommandPressed(Command.Down)) CursorDown();
            else if (Input.CurrentInput.CommandPressed(Command.Confirm) && confirmCooldown <= 0)
            {
                Audio.PlaySound(GameSound.Cursor);
                SwapItem();
            }
            else if (Input.CurrentInput.CommandPressed(Command.Cancel))
            {
                Audio.PlaySound(GameSound.Back);
                Terminate();
            }

            if (confirmCooldown > 0) confirmCooldown -= gameTime.ElapsedGameTime.Milliseconds;
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

            int previousSlot = slot;
            slot = AvailableItems.ToList().FindIndex(x => x.Value == record);

            if (Input.MOUSE_MODE && slot == previousSlot)
            {
                SwapItem();
            }
            else
            {
                Description1.Value = record.Description.ElementAtOrDefault(0);
                Description2.Value = record.Description.ElementAtOrDefault(1);
                Description3.Value = record.Description.ElementAtOrDefault(2);
                Description4.Value = record.Description.ElementAtOrDefault(3);
                Description5.Value = record.Description.ElementAtOrDefault(4);
            }
        }

        private void SwapItem()
        {
            Terminate();

            if (AvailableItems[slot].Name == " - CANCEL - ")
            {

            }
            else if (AvailableItems[slot].Name == " - REMOVE - ")
            {
                var oldItem = EquipmentList.ElementAt(equipmentSlot);
                EquipmentList.Remove(oldItem);
                GameProfile.Inventory.Add(oldItem.Value as ItemRecord);

                EquipmentList.Add(new ItemRecord()
                {
                    Icon = "Blank",
                    Name = "- Empty Slot -",
                    Charges = -1,
                    ChargesLeft = -1,
                    Description = new string[] { "", "Select to equip", "a new item", "", "" }
                });

                equipmentViewModel.EquipmentList.ModelList = EquipmentList.ModelList;
                HeroModel.Equipment.ModelList = EquipmentList.ModelList.Where(x => x.Value.Name != "- Empty Slot -").ToList();
            }
            else
            {
                var oldItem = EquipmentList.ElementAt(equipmentSlot).Value;
                if (EquipmentList[equipmentSlot].Name != "- Empty Slot -") GameProfile.Inventory.Add(oldItem as ItemRecord);

                EquipmentList.ElementAt(equipmentSlot).Value = AvailableItems[slot];                
                GameProfile.Inventory.Remove(AvailableItems.ElementAt(slot));

                equipmentViewModel.EquipmentList.ModelList = EquipmentList.ModelList;
                HeroModel.Equipment.ModelList = EquipmentList.ModelList.Where(x => x.Value.Name != "- Empty Slot -").ToList();
            }
        }


        public ModelProperty<string> Description1 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description2 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description3 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description4 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description5 { get; set; } = new ModelProperty<string>("");
    }
}
