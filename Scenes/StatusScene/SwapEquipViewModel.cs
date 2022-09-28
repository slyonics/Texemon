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

        public ModelCollection<ItemRecord> AvailableItems { get; set; }

        public HeroModel HeroModel { get; private set; }
        public ModelProperty<AnimatedSprite> PlayerSprite { get; private set; }
        public ModelCollection<CommandRecord> EquipmentList { get; private set; }

        public SwapEquipViewModel(StatusScene iScene, EquipmentViewModel iEquipmentViewModel, HeroModel iHeroModel, AnimatedSprite iAnimatedSprite, ModelCollection<CommandRecord> iEquipList, int iEquipSlot)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;
            equipmentViewModel = iEquipmentViewModel;
            HeroModel = iHeroModel;
            PlayerSprite = new ModelProperty<AnimatedSprite>(iAnimatedSprite);
            equipmentSlot = iEquipSlot;

            EquipmentList = iEquipList;

            AvailableItems = new ModelCollection<ItemRecord>();
            if (string.IsNullOrEmpty(EquipmentList[iEquipSlot].Name))
            {
                AvailableItems.Add(new ItemRecord()
                {
                    Icon = "Blank",
                    Name = " - CANCEL - ",
                    Charges = -1,
                    ChargesLeft = -1,
                    Description = new string[] { "", "", "Leave slot empty", "", "" }
                });
            }
            else
            {
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
            else if (Input.CurrentInput.CommandPressed(Command.Cancel))
            {
                Audio.PlaySound(GameSound.Back);
                Terminate();
                equipmentViewModel.Visible = true;
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

            slot = AvailableItems.ToList().FindIndex(x => x.Value == record);

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
