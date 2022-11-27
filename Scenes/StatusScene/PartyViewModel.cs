using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

using MonsterTrainer.Models;
using MonsterTrainer.SceneObjects.Widgets;

namespace MonsterTrainer.Scenes.StatusScene
{
    public class PartyMemberModel
    {
        public ModelProperty<AnimatedSprite> PlayerSprite { get; set; }
        public ModelProperty<HeroModel> HeroModel { get; set; }
    }

    public class PartyViewModel : ViewModel, IStatusSubView
    {

        StatusScene statusScene;

        public ViewModel ChildViewModel { get; set; }

        public bool SuppressCancel { get; set; }

        public ModelCollection<PartyMemberModel> PartyMembers { get; private set; }

        public PartyViewModel(StatusScene iScene, ModelCollection<PartyMemberModel> iPartyMembers)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;

            PartyMembers = iPartyMembers;

            LoadView(GameView.StatusScene_PartyView);

            Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // if (Input.CurrentInput.CommandPressed(Command.Cancel)) Ter
        }

        public void ResetSlot()
        {

        }

        public void MoveAway()
        {

        }

        public bool SuppressLeftRight { get => false; }
    }
}
