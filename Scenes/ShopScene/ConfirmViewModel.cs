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
    public class ConfirmViewModel : ViewModel
    {
        ShopScene shopScene;

        public ConfirmViewModel(ShopScene iScene)
            : base(iScene, PriorityLevel.GameLevel)
        {
            shopScene = iScene;

            LoadView(GameView.BattleScene_CommandView);

            if (!Input.MOUSE_MODE)
            {

            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

        }


    }
}
