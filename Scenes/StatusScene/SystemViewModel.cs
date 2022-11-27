using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

using MonsterLegends.Models;
using MonsterLegends.SceneObjects.Widgets;

namespace MonsterLegends.Scenes.StatusScene
{
    public class SystemViewModel : ViewModel, IStatusSubView
    {
        StatusScene statusScene;

        public ViewModel ChildViewModel { get; set; }

        private int saveSlot = -1;

        public ModelCollection<TitleScene.SaveModel> AvailableSaves { get; set; } = new ModelCollection<TitleScene.SaveModel>();

        public bool SuppressCancel { get; set; }

        public SystemViewModel(StatusScene iScene)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;

            var saves = GameProfile.GetAllSaveData();
            for (int i = 0; i < 4; i++)
            {
                if (saves.ContainsKey(i))
                {
                    var save = saves[i];
                    AnimatedSprite animatedSprite = new AnimatedSprite(AssetCache.SPRITES[((HeroModel)save["PartyLeader"]).Sprite.Value], StatusViewModel.HERO_ANIMATIONS);
                    AvailableSaves.Add(new TitleScene.SaveModel()
                    {
                        PartyLeader = new ModelProperty<HeroModel>((HeroModel)save["PartyLeader"]),
                        PlayerLocation = new ModelProperty<string>((string)save["PlayerLocation"]),
                        WindowStyle = new ModelProperty<string>((string)save["WindowStyle"]),
                        WindowSelectedStyle = new ModelProperty<string>(((string)save["WindowStyle"]).Replace("Window", "Selected")),
                        Font = new ModelProperty<GameFont>((GameFont)save["Font"]),
                        SaveSlot = new ModelProperty<int>(i),
                        AnimatedSprite = new ModelProperty<AnimatedSprite>(animatedSprite)
                    });
                }
                else
                {
                    HeroModel heroModel = new HeroModel(HeroType.Inventor);
                    heroModel.Name.Value = "- Empty Save -";
                    AnimatedSprite animatedSprite = new AnimatedSprite(AssetCache.SPRITES[GameSprite.Actors_Blank], StatusViewModel.HERO_ANIMATIONS);
                    AvailableSaves.Add(new TitleScene.SaveModel()
                    {
                        PartyLeader = new ModelProperty<HeroModel>(heroModel),
                        PlayerLocation = new ModelProperty<string>(""),
                        WindowStyle = new ModelProperty<string>(GameProfile.PlayerProfile.WindowStyle.Value),
                        WindowSelectedStyle = new ModelProperty<string>(GameProfile.PlayerProfile.SelectedStyle.Value),
                        Font = new ModelProperty<GameFont>(GameProfile.PlayerProfile.Font.Value),
                        SaveSlot = new ModelProperty<int>(i),
                        AnimatedSprite = new ModelProperty<AnimatedSprite>(animatedSprite)
                    });
                }
            }

            LoadView(GameView.StatusScene_SystemView);

            Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            InputFrame currentInput = Input.CurrentInput;
            if (currentInput.CommandPressed(Command.Up)) CursorUp();
            else if (currentInput.CommandPressed(Command.Down)) CursorDown();
            else if (currentInput.CommandPressed(Command.Confirm))
            {
                Audio.PlaySound(GameSound.Cursor);
                if (saveSlot == 4) Exit();
                else if (saveSlot == -1)
                {
                    saveSlot = 0;
                    (GetWidget<DataGrid>("Saves").ChildList[saveSlot] as Button).RadioSelect();
                }
                else Save(saveSlot);
            }
        }

        private void CursorUp()
        {
            if (saveSlot == -1) return;

            saveSlot--;
            if (saveSlot < 0)
            {
                saveSlot = 0;
                return;
            }

            Audio.PlaySound(GameSound.menu_select);

            (GetWidget<DataGrid>("Saves").ChildList[saveSlot] as Button).RadioSelect();
            GetWidget<Button>("Exit").UnSelect();
        }

        private void CursorDown()
        {
            saveSlot++;
            
            if (saveSlot >= 5)
            {
                saveSlot = 4;
                return;
            }

            Audio.PlaySound(GameSound.menu_select);

            if (saveSlot == 4)
            {
                GetWidget<Button>("Exit").RadioSelect();
                (GetWidget<DataGrid>("Saves").ChildList[3] as Button).UnSelect();
            }
            else (GetWidget<DataGrid>("Saves").ChildList[saveSlot] as Button).RadioSelect();
        }

        public void Exit()
        {
            GetWidget<Button>("Exit").UnSelect();

            CrossPlatformGame.Transition(typeof(TitleScene.TitleScene));
        }

        public void Save(object parameter)
        {
            if (parameter is IModelProperty)
            {
                saveSlot = (int)((IModelProperty)parameter).GetValue();
            }
            else saveSlot = (int)parameter;
            GameProfile.SaveSlot = saveSlot;

            ((MapScene.MapScene)CrossPlatformGame.SceneStack.First(x => x is MapScene.MapScene)).SaveMapPosition();
            GameProfile.SetSaveData<HeroModel>("PartyLeader", GameProfile.PlayerProfile.Party.First().Value);
            GameProfile.SaveState();
            
            var save = GameProfile.SaveData;
            AnimatedSprite animatedSprite = new AnimatedSprite(AssetCache.SPRITES[((HeroModel)save["PartyLeader"]).Sprite.Value], StatusViewModel.HERO_ANIMATIONS);

            AvailableSaves[saveSlot].PartyLeader.Value.Name.Value = ((HeroModel)save["PartyLeader"]).Name.Value;
            AvailableSaves[saveSlot].PlayerLocation.Value = (string)save["PlayerLocation"];
            AvailableSaves[saveSlot].WindowStyle.Value = (string)save["WindowStyle"];
            AvailableSaves[saveSlot].WindowSelectedStyle.Value = ((string)save["WindowStyle"]).Replace("Window", "Selected");
            AvailableSaves[saveSlot].Font.Value = (GameFont)save["Font"];
            AvailableSaves[saveSlot].AnimatedSprite.Value = animatedSprite;
        }

        public void ResetSlot()
        {
            if (saveSlot != 4 && saveSlot != -1) (GetWidget<DataGrid>("Saves").ChildList[saveSlot] as Button).UnSelect();
            GetWidget<Button>("Exit").UnSelect();
            saveSlot = -1;
        }

        public void MoveAway()
        {

        }

        public bool SuppressLeftRight { get => false; }
    }
}
