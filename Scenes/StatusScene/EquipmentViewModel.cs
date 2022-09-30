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
    public class EquipmentViewModel : ViewModel, IStatusSubView
    {
        public static readonly Dictionary<string, Animation> HERO_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { "Idle", new Animation(0, 0, 24, 32, 4, 400) }
        };

        StatusScene statusScene;

        private int partySlot = -1;
        private int equipmentSlot = -1;

        public SwapEquipViewModel ChildViewModel { get; set; }

        public bool SuppressCancel { get => SuppressLeftRight; }

        public ModelCollection<PartyMemberModel> PartyMembers { get; private set; } = new ModelCollection<PartyMemberModel>();
        public ModelCollection<ItemRecord> EquipmentList { get; private set; } = new ModelCollection<ItemRecord>();

        public EquipmentViewModel(StatusScene iScene)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;

            foreach (ModelProperty<HeroModel> heroModelProperty in GameProfile.PlayerProfile.Party)
            {
                Texture2D sprite = AssetCache.SPRITES[heroModelProperty.Value.Sprite.Value];
                AnimatedSprite animatedSprite = new AnimatedSprite(sprite, HERO_ANIMATIONS);
                PartyMemberModel partyMember = new PartyMemberModel()
                {
                    PlayerSprite = new ModelProperty<AnimatedSprite>(animatedSprite),
                    HeroModel = new ModelProperty<HeroModel>(heroModelProperty.Value)
                };
                PartyMembers.Add(partyMember);
            }

            LoadView(GameView.StatusScene_EquipmentView);

            Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (ChildViewModel != null)
            {
                if (ChildViewModel.Terminated)
                {                    
                    ChildViewModel = null;
                    this.Visible = true;

                    var newEquipList = new List<ModelProperty<ItemRecord>>(PartyMembers[partySlot].HeroModel.Value.Equipment.ModelList);
                    while (newEquipList.Count < PartyMembers[partySlot].HeroModel.Value.EquipmentSlots.Value)
                    {
                        newEquipList.Add(new ModelProperty<ItemRecord>(new ItemRecord()
                        {
                            Icon = "Blank",
                            Name = "- Empty Slot -",
                            Charges = -1,
                            ChargesLeft = -1,
                            Description = new string[] { "", "Select to equip", "a new item", "", "" }
                        }));
                    }
                    EquipmentList.ModelList = newEquipList;
                                        
                    if (Input.MOUSE_MODE)
                    {
                        equipmentSlot = -1;
                        ShowDescription.Value = false;
                    }
                    else
                    {
                        (GetWidget<DataGrid>("EquipmentList").ChildList[equipmentSlot] as Button).RadioSelect();
                        return;
                    }
                }
                else
                {
                    //SuppressCancel = true;
                    return;
                }
            }

            base.Update(gameTime);

            //SuppressCancel = false;

            if (ShowDescription.Value)
            {
                if (Input.CurrentInput.CommandPressed(Command.Up)) EquipmentCursorUp();
                else if (Input.CurrentInput.CommandPressed(Command.Down)) EquipmentCursorDown();
                else if (Input.CurrentInput.CommandPressed(Command.Confirm))
                {
                    Audio.PlaySound(GameSound.Cursor);
                    HeroModel heroModel = PartyMembers[partySlot].HeroModel.Value;
                    ChildViewModel = statusScene.AddView(new SwapEquipViewModel(statusScene, this, heroModel, PartyMembers[partySlot].PlayerSprite.Value, EquipmentList, equipmentSlot));
                    this.Visible = false;
                }
                else if (Input.CurrentInput.CommandPressed(Command.Cancel) || Input.CurrentInput.CommandPressed(Command.Left))
                {
                    Audio.PlaySound(GameSound.Back);
                    (GetWidget<DataGrid>("EquipmentList").ChildList[equipmentSlot] as Button).UnSelect();
                    equipmentSlot = -1;
                    //SuppressCancel = true;
                    ShowDescription.Value = false;
                }
            }
            else
            {
                if (Input.CurrentInput.CommandPressed(Command.Up)) PartyCursorUp();
                else if (Input.CurrentInput.CommandPressed(Command.Down)) PartyCursorDown();
                else if (Input.CurrentInput.CommandPressed(Command.Confirm))
                {
                    if (partySlot == -1)
                    {
                        Audio.PlaySound(GameSound.Cursor);
                        SelectParty(PartyMembers[0].HeroModel);
                        equipmentSlot = -1;
                        (GetWidget<DataGrid>("PartyList").ChildList[partySlot] as Button).RadioSelect();
                    }
                    else
                    {
                        Audio.PlaySound(GameSound.Cursor);
                        SelectParty(PartyMembers[partySlot].HeroModel);

                        equipmentSlot = 0;
                        var item = EquipmentList.First().Value;
                        (GetWidget<DataGrid>("EquipmentList").ChildList[equipmentSlot] as Button).RadioSelect();

                        Description1.Value = item.Description.ElementAtOrDefault(0);
                        Description2.Value = item.Description.ElementAtOrDefault(1);
                        Description3.Value = item.Description.ElementAtOrDefault(2);
                        Description4.Value = item.Description.ElementAtOrDefault(3);
                        Description5.Value = item.Description.ElementAtOrDefault(4);

                        ShowDescription.Value = true;
                    }
                }
                else if (Input.CurrentInput.CommandPressed(Command.Right) && partySlot != -1 && equipmentSlot == -1)
                {
                    Audio.PlaySound(GameSound.Cursor);
                    SelectParty(PartyMembers[partySlot].HeroModel);

                    equipmentSlot = 0;
                    var item = EquipmentList.First().Value;
                    (GetWidget<DataGrid>("EquipmentList").ChildList[equipmentSlot] as Button).RadioSelect();

                    Description1.Value = item.Description.ElementAtOrDefault(0);
                    Description2.Value = item.Description.ElementAtOrDefault(1);
                    Description3.Value = item.Description.ElementAtOrDefault(2);
                    Description4.Value = item.Description.ElementAtOrDefault(3);
                    Description5.Value = item.Description.ElementAtOrDefault(4);

                    ShowDescription.Value = true;
                }
                else if (Input.CurrentInput.CommandPressed(Command.Cancel) && partySlot != -1)
                {
                    Audio.PlaySound(GameSound.Back);

                    (GetWidget<DataGrid>("PartyList").ChildList[partySlot] as Button).UnSelect();
                    ResetSlot();
                }
            }
        }

        private void PartyCursorUp()
        {
            partySlot--;
            if (partySlot < 0)
            {
                partySlot = 0;
                return;
            }

            Audio.PlaySound(GameSound.menu_select);

            SelectParty(PartyMembers[partySlot].HeroModel);

            equipmentSlot = -1;

            (GetWidget<DataGrid>("PartyList").ChildList[partySlot] as Button).RadioSelect();
        }

        private void PartyCursorDown()
        {
            partySlot++;
            if (partySlot >= GameProfile.PlayerProfile.Party.Count())
            {
                partySlot = GameProfile.PlayerProfile.Party.Count() - 1;
                return;
            }

            Audio.PlaySound(GameSound.menu_select);

            SelectParty(PartyMembers[partySlot].HeroModel);

            equipmentSlot = -1;

            (GetWidget<DataGrid>("PartyList").ChildList[partySlot] as Button).RadioSelect();
        }

        private void EquipmentCursorUp()
        {
            equipmentSlot--;
            if (equipmentSlot < 0)
            {
                equipmentSlot = 0;
                return;
            }

            Audio.PlaySound(GameSound.menu_select);

            SelectItem(EquipmentList[equipmentSlot]);
            (GetWidget<DataGrid>("EquipmentList").ChildList[equipmentSlot] as Button).RadioSelect();
        }

        private void EquipmentCursorDown()
        {
            equipmentSlot++;
            if (equipmentSlot >= EquipmentList.Count())
            {
                equipmentSlot = EquipmentList.Count() - 1;
                return;
            }

            Audio.PlaySound(GameSound.menu_select);

            SelectItem(EquipmentList[equipmentSlot]);
            (GetWidget<DataGrid>("EquipmentList").ChildList[equipmentSlot] as Button).RadioSelect();
        }

        public void SelectParty(object parameter)
        {
            HeroModel record;
            if (parameter is IModelProperty)
            {
                record = (HeroModel)((IModelProperty)parameter).GetValue();
            }
            else record = (HeroModel)parameter;

            partySlot = PartyMembers.ToList().FindIndex(x => x.Value.HeroModel.Value == record);

            var newEquipList = new List<ModelProperty<ItemRecord>>(record.Equipment.ModelList);
            while (newEquipList.Count < record.EquipmentSlots.Value)
            {
                newEquipList.Add(new ModelProperty<ItemRecord>(new ItemRecord()
                {
                    Icon = "Blank",
                    Name = "- Empty Slot -",
                    Charges = -1,
                    ChargesLeft = -1,
                    Description = new string[] { "", "Select to equip", "a new item", "", "" }
                }));
            }
            EquipmentList.ModelList = newEquipList;

            ShowEquipment.Value = true;

            if (Input.MOUSE_MODE)
            {
                equipmentSlot = -1;
                ShowDescription.Value = false;
            }
        }

        public void SelectItem(object parameter)
        {
            CommandRecord record;
            if (parameter is IModelProperty)
            {
                record = (CommandRecord)((IModelProperty)parameter).GetValue();
            }
            else record = (CommandRecord)parameter;

            int oldSlot = equipmentSlot;
            equipmentSlot = EquipmentList.ToList().FindIndex(x => x.Value == record);

            if (equipmentSlot == oldSlot && Input.MOUSE_MODE)
            {
                HeroModel heroModel = PartyMembers[partySlot].HeroModel.Value;
                ChildViewModel = statusScene.AddView(new SwapEquipViewModel(statusScene, this, heroModel, PartyMembers[partySlot].PlayerSprite.Value, EquipmentList, equipmentSlot));
                this.Visible = false;
            }
            else
            {
                Description1.Value = record.Description.ElementAtOrDefault(0);
                Description2.Value = record.Description.ElementAtOrDefault(1);
                Description3.Value = record.Description.ElementAtOrDefault(2);
                Description4.Value = record.Description.ElementAtOrDefault(3);
                Description5.Value = record.Description.ElementAtOrDefault(4);

                ShowDescription.Value = true;
            }
        }

        public void ResetSlot()
        {
            if (partySlot >= 0) (GetWidget<DataGrid>("PartyList").ChildList[partySlot] as Button).UnSelect();
            partySlot = -1;
            equipmentSlot = -1;

            EquipmentList.ModelList = new List<ModelProperty<ItemRecord>>();
            ShowDescription.Value = false;
            ShowEquipment.Value = false;
        }

        public void MoveAway()
        {
            ChildViewModel?.Terminate();
            ChildViewModel = null;
        }

        public bool SuppressLeftRight { get => partySlot != -1; }

        public ModelProperty<bool> ShowEquipment { get; set; } = new ModelProperty<bool>(false);
        public ModelProperty<bool> ShowDescription { get; set; } = new ModelProperty<bool>(false);

        public ModelProperty<string> Description1 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description2 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description3 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description4 { get; set; } = new ModelProperty<string>("");
        public ModelProperty<string> Description5 { get; set; } = new ModelProperty<string>("");
    }
}
