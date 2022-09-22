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
        public ModelProperty<StatusScene.HeroModel> PartyLeader { get; set; }
        public ModelProperty<string> PlayerLocation { get; set; }
        public ModelProperty<string> WindowStyle { get; set; }
        public ModelProperty<string> WindowSelectedStyle { get; set; }
        public ModelProperty<int> SaveSlot { get; set; }
        public ModelProperty<AnimatedSprite> AnimatedSprite { get; set; }
    }

    public class TitleViewModel : ViewModel
    {
        public static readonly Dictionary<string, Animation> HERO_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { "Idle", new Animation(0, 0, 24, 32, 4, 400) }
        };

        private SettingsViewModel settingsViewModel;

        public ModelCollection<SaveModel> AvailableSaves { get; set; } = new ModelCollection<SaveModel>();

        public TitleViewModel(Scene iScene, GameView viewName)
            : base(iScene, PriorityLevel.GameLevel)
        {
            int i = 0;
            var saves = GameProfile.GetAllSaveData();
            foreach (var save in saves)
            {
                AnimatedSprite animatedSprite = new AnimatedSprite(AssetCache.SPRITES[((StatusScene.HeroModel)save["PartyLeader"]).Sprite.Value], HERO_ANIMATIONS);
                AvailableSaves.Add(new SaveModel()
                {
                    PartyLeader = new ModelProperty<StatusScene.HeroModel>((StatusScene.HeroModel)save["PartyLeader"]),
                    PlayerLocation = new ModelProperty<string>((string)save["PlayerLocation"]),
                    WindowStyle = new ModelProperty<string>((string)save["WindowStyle"]),
                    WindowSelectedStyle = new ModelProperty<string>(((string)save["WindowStyle"]).Replace("Window", "Selected")),
                    SaveSlot = new ModelProperty<int>(i),
                    AnimatedSprite = new ModelProperty<AnimatedSprite>(animatedSprite)
                });

                i++;
            }

            LoadView(GameView.TitleScene_TitleView);
        }

        public void NewGame()
        {
            CrossPlatformGame.Transition(typeof(IntroScene.IntroScene));
        }

        public void Continue(object saveSlot)
        {
            GameProfile.LoadState("Save" + saveSlot.ToString() + ".sav");

            string mapName = GameProfile.GetSaveData<string>("LastMapName");
            Vector2 mapPosition = new Vector2(GameProfile.GetSaveData<int>("LastPositionX"), GameProfile.GetSaveData<int>("LastPositionY"));

            CrossPlatformGame.Transition(typeof(MapScene.MapScene), mapName, mapPosition);
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
