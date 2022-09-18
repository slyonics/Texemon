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
    public class ItemViewModel : ViewModel
    {
        StatusScene statusScene;
        int enemyWidth;
        int enemyHeight;

        public ModelCollection<Type> AvailableMenus { get; private set; } = new ModelCollection<Type>();

        public ModelProperty<Type> HighlightedMenu { get; private set; }
        public ModelProperty<Type> ActiveMenu { get; private set; }

        public ItemViewModel(StatusScene iScene)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene;

            //AvailableMenus.Add(new ItemViewModel(statusScene));


            LoadView(GameView.StatusScene_StatusView);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);


        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public void StartPlayerTurn(ModelProperty<Type> selectedViewModel)
        {
            ActiveMenu.Value = selectedViewModel.Value;
            statusScene.AddView((ViewModel)Activator.CreateInstance(selectedViewModel.Value));
        }

        public override void Terminate()
        {
            base.Terminate();

            statusScene.EndScene();
        }
    }
}
