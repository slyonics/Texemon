using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

using Texemon.Main;
using Texemon.Models;
using Texemon.SceneObjects.Widgets;
using Texemon.Scenes.StatusScene;

namespace Texemon.Scenes.ConversationScene
{
    public class SelectionViewModel : ViewModel
    {
        public SelectionViewModel(Scene iScene, List<string> options)
            : base(iScene, PriorityLevel.MenuLevel)
        {
            foreach (string option in options) AvailableOptions.Add(option);

            LoadView(GameView.ConversationScene_SelectionView);
        }

        public override void Terminate()
        {
            GameProfile.SetSaveData<string>("LastSelection", "No");
            base.Terminate();
        }

        public ModelCollection<string> AvailableOptions { get; set; } = new ModelCollection<string>();

        public ModelProperty<Rectangle> Window { get; set; } = new ModelProperty<Rectangle>(new Rectangle(-120, 20, 240, 60));
    }
}
