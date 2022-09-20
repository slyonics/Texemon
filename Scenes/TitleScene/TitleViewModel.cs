using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texemon.Main;
using Texemon.Models;
using Texemon.SceneObjects.Widgets;

namespace Texemon.Scenes.TitleScene
{
    public class SaveModel
    {
        public ModelProperty<StatusScene.HeroModel> PartyLeader;
        public ModelProperty<string> PlayerLocation;
        public ModelProperty<string> WindowStyle;
    }

    public class TitleViewModel : ViewModel
    {
        private SettingsViewModel settingsViewModel;

        public ModelCollection<SaveModel> AvailableSaves = new ModelCollection<SaveModel>();

        public TitleViewModel(Scene iScene, GameView viewName)
            : base(iScene, PriorityLevel.GameLevel)
        {
            var saves = GameProfile.GetAllSaveData();
            foreach (var save in saves)
            {
                AvailableSaves.Add(new SaveModel()
                {
                    PartyLeader = new ModelProperty<StatusScene.HeroModel>((StatusScene.HeroModel)save["PartyLeader"]),
                    PlayerLocation = new ModelProperty<string>((string)save["PlayerLocation"]),
                    WindowStyle = new ModelProperty<string>((string)save["WindowStyle"])
                });
            }

            LoadView(GameView.TitleScene_TitleView);

            GetWidget<Button>("LoadButton").Enabled = (GameProfile.SaveList.Count > 0);
        }

        public void NewGame()
        {
            GameProfile.NewState();

            /*
            CrossPlatformGame.Transition(typeof(MapScene.MapScene), "SchoolOrigin");
            */

            CrossPlatformGame.Transition(typeof(MapScene.MapScene), "City");
        }

        public void Continue()
        {
            GameProfile.LoadState("Save0.sav");

            /*
            string mapName = GameProfile.GetSaveData<string>("LastMap");
            int roomX = GameProfile.GetSaveData<int>("LastRoomX");
            int roomY = GameProfile.GetSaveData<int>("LastRoomY");
            MapScene.MapScene.Direction direction = GameProfile.GetSaveData<MapScene.MapScene.Direction>("LastDirection");

            if (GameProfile.GetSaveData<int>("RandomBattle") < 2) GameProfile.SetSaveData<int>("RandomBattle", 2);

            CrossPlatformGame.Transition(typeof(MapScene.MapScene), mapName, roomX, roomY, direction);
            */
        }

        public void SettingsMenu()
        {
            settingsViewModel = new SettingsViewModel(parentScene, GameView.TitleScene_SettingsView);
            parentScene.AddOverlay(settingsViewModel);
        }

        public void Credits()
        {
            CrossPlatformGame.Transition(typeof(CreditsScene.CreditsScene));
        }

        public void Exit()
        {
            Settings.SaveSettings();
            GameProfile.SetSaveData<StatusScene.HeroModel>("PartyLeader", GameProfile.PlayerProfile.Party.First().Value);
            CrossPlatformGame.GameInstance.Exit();
        }

        public override void Terminate()
        {
            base.Terminate();

            settingsViewModel.Terminate();
        }

        public ModelProperty<bool> SaveAvailable { get; set; } = new ModelProperty<bool>(false);
    }
}
