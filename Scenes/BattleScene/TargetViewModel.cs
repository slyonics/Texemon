using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.Xna.Framework.Graphics;

using Texemon.Models;
using Texemon.SceneObjects.Widgets;
using Texemon.Scenes.StatusScene;

namespace Texemon.Scenes.BattleScene
{
    public class TargetViewModel : ViewModel
    {
        public class TargetButton
        {
            public string Name { get; set; }
            public bool NameVisible { get; set; }
            public Rectangle Bounds { get; set; }

            public Battler target;
        }

        BattleScene battleScene;


        public TargetViewModel(BattleScene iScene, BattlePlayer iPlayer, CommandRecord iCommandRecord)
            : base(iScene, PriorityLevel.GameLevel)
        {
            battleScene = iScene;

            Player = iPlayer;
            Command = iCommandRecord;

            switch (Command.Targetting)
            {
                case TargetType.SingleEnemy:
                    Targets.ModelList = new List<ModelProperty<TargetButton>>();
                    foreach (BattleEnemy battleEnemy in battleScene.EnemyList) Targets.Add(new TargetButton() { Name = battleEnemy.Stats.Name.Value, NameVisible = true, Bounds=battleEnemy.SpriteBounds, target = battleEnemy });
                    break;

                case TargetType.AllEnemy:
                    {
                        Targets.ModelList = new List<ModelProperty<TargetButton>>();
                        Rectangle bounds = battleScene.EnemyList[0].SpriteBounds;
                        for (int i = 1; i < battleScene.EnemyList.Count; i++) bounds = Rectangle.Union(bounds, battleScene.EnemyList[i].SpriteBounds);
                        Targets.Add(new TargetButton() { Name = "All Enemies", NameVisible = true, Bounds = bounds });
                        break;
                    }

                case TargetType.SingleAlly:
                    Targets.ModelList = new List<ModelProperty<TargetButton>>();
                    foreach (BattlePlayer battlePlayer in battleScene.PlayerList)
                    {
                        Rectangle bounds = battlePlayer.SpriteBounds;
                        Targets.Add(new TargetButton() { Name = "", NameVisible = false, Bounds = bounds, target = battlePlayer });
                    }
                    break;

                case TargetType.Self:
                    Targets.ModelList = new List<ModelProperty<TargetButton>>();
                    Targets.Add(new TargetButton() { Name = "", NameVisible = false, Bounds = Player.SpriteBounds });
                    break;
            }

            LoadView(GameView.BattleScene_TargetView);
        }

        public void SelectTarget(object parameter)
        {
            Terminate();
            Player.EndTurn();

            TargetButton target = (TargetButton)((IModelProperty)parameter).GetValue();

            switch (Command.Targetting)
            {
                case TargetType.Self:
                case TargetType.SingleAlly:
                case TargetType.SingleEnemy:
                    {
                        BattleController battleController = new BattleController(battleScene, Player, target.target, Command.Script);
                        battleScene.AddController(battleController);
                    }
                    break;

                case TargetType.AllEnemy:
                    {
                        int delay = 0;
                        foreach (BattleEnemy battleEnemy in battleScene.EnemyList)
                        {                            
                            string[] script = new string[Command.Script.Count() + 1];
                            script[0] = "Wait " + delay;
                            for (int i = 1; i < script.Count(); i++) script[i] = Command.Script[i - 1];
                            delay += 200;

                            BattleController battleController = new BattleController(battleScene, Player, battleEnemy, script);
                            battleScene.AddController(battleController);
                        }
                    }
                    break;

                case TargetType.AllAlly:
                    {
                        int delay = 0;
                        foreach (BattlePlayer battlePlayer in battleScene.PlayerList)
                        {
                            string[] script = new string[Command.Script.Count() + 1];
                            script[0] = "Wait " + delay;
                            for (int i = 1; i < script.Count(); i++) script[i] = Command.Script[i - 1];                            
                            delay += 200;

                            BattleController battleController = new BattleController(battleScene, Player, battlePlayer, script);
                            battleScene.AddController(battleController);
                        }
                    }
                    break;
            }
        }

        public BattlePlayer Player { get; set; }
        public CommandRecord Command { get; set; }
        public ModelCollection<TargetButton> Targets { get; set; } = new ModelCollection<TargetButton>();
    }
}
