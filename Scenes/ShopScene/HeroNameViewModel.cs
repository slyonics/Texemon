using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texemon.Main;
using Texemon.Models;
using Texemon.SceneObjects.Widgets;
using Texemon.Scenes.StatusScene;

namespace Texemon.Scenes.ShopScene
{
    public class HeroNameViewModel : ViewModel
    {
        Textbox namingBox;
        VoucherRecord voucherRecord;
        public HeroRecord heroRecord;

        int confirmCooldown = 100;

        public HeroNameViewModel(Scene iScene, VoucherRecord iVoucherRecord)
            : base(iScene, PriorityLevel.MenuLevel, GameView.ShopScene_HeroNameView)
        {
            voucherRecord = iVoucherRecord;
            heroRecord = StatusScene.StatusScene.HEROES.First(x => x.Name.ToString() == voucherRecord.Name.Replace(" ", ""));

            namingBox = GetWidget<Textbox>("NamingBox");
            namingBox.Text = heroRecord.BattlerModel.Name.Value;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.CurrentInput.CommandPressed(Command.Cancel))
            {
                GetWidget<Button>("Cancel").RadioSelect();
            }
            else if (Input.CurrentInput.CommandReleased(Command.Cancel))
            {
                Audio.PlaySound(GameSound.Back);
                GetWidget<Button>("Cancel").UnSelect();
                Terminate();
            }
            else if (Input.CurrentInput.CommandPressed(Command.Confirm) && confirmCooldown <= 0)
            {
                GetWidget<Button>("OK").RadioSelect();
                namingBox.Active = false;
            }
            else if (Input.CurrentInput.CommandReleased(Command.Confirm) && confirmCooldown <= 0)
            {
                Audio.PlaySound(GameSound.menu_select);
                GetWidget<Button>("OK").UnSelect();
                Proceed();
            }

            if (confirmCooldown > 0) confirmCooldown -= gameTime.ElapsedGameTime.Milliseconds;
        }

        public void Proceed()
        {
            namingBox.Active = false;
            Terminate();

            HeroModel heroModel = new HeroModel(heroRecord.Name);
            heroModel.Name.Value = namingBox.Text;
            // TODO overflow party to backbench
            GameProfile.PlayerProfile.Party.Add(heroModel);
            GameProfile.SetSaveData<bool>(heroRecord.Name + "Recruited", true);

            Audio.PauseMusic(true);
            Audio.PlaySound(GameSound.JoinParty);
            Task.Delay(1500).ContinueWith(t => Audio.PauseMusic(false));

            var mapScene = CrossPlatformGame.SceneStack.First(x => x is MapScene.MapScene) as MapScene.MapScene;
            mapScene.AddPartyMember(heroModel);

            Confirmed = namingBox.Text;


            foreach (CostRecord costRecord in voucherRecord.Cost)
            {
                GameProfile.Inventory.Remove(GameProfile.Inventory.First(x => x.Value.Name == costRecord.Item));
            }
        }

        public void Cancel()
        {
            namingBox.Active = false;
            Terminate();
        }

        public string Confirmed { get; set; } = null;
    }
}
