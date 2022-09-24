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
    public class StatusViewModel : ViewModel
    {
        public static readonly Dictionary<string, Animation> HERO_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { "Idle", new Animation(0, 0, 24, 32, 4, 400) }
        };

        List<ViewModel> SubViews { get; set; } = new List<ViewModel>();

        StatusScene statusScene;

        public ViewModel ChildViewModel { get; set; }

        public StatusViewModel(StatusScene iScene)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;

            SubViews.Add(new PartyViewModel(statusScene));
            SubViews.Add(new ItemViewModel(statusScene));
                        
            LoadView(GameView.StatusScene_StatusView);

            GetWidget<Button>("PartyButton").RadioSelect();
            SelectParty();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            ChildViewModel?.Update(gameTime);
            
            if (Input.CurrentInput.CommandPressed(Command.Cancel)) Back();
            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            ChildViewModel?.Draw(spriteBatch);
        }

        public override void Terminate()
        {
            base.Terminate();

            statusScene.EndScene();
        }

        public void SelectParty()
        {
            ChildViewModel = SubViews.First(x => x is PartyViewModel);
        }

        public void SelectItems()
        {
            ChildViewModel = SubViews.First(x => x is ItemViewModel);
        }

        public void SelectQuit()
        {
            ((MapScene.MapScene)CrossPlatformGame.SceneStack.First(x => x is MapScene.MapScene)).SaveMapPosition();
            GameProfile.SetSaveData<HeroModel>("PartyLeader", GameProfile.PlayerProfile.Party.First().Value);
            GameProfile.SaveState();
            CrossPlatformGame.Transition(typeof(TitleScene.TitleScene));
        }

        public void Back()
        {
            Close();
        }
    }
}
