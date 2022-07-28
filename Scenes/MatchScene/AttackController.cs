using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texemon.Main;
using Texemon.Models;
using Texemon.SceneObjects;
using Texemon.SceneObjects.Controllers;

namespace Texemon.Scenes.MatchScene
{
    public class AttackController : ScriptController
    {
        private MatchScene matchScene;

        public bool EndGame { get; private set; }

        public AttackController(MatchScene iScene, string script)
            : base(iScene, script, PriorityLevel.GameLevel)
        {
            matchScene = iScene;
        }

        public AttackController(MatchScene iScene, string[] script)
            : base(iScene, script, PriorityLevel.GameLevel)
        {
            matchScene = iScene;
        }

        public override bool ExecuteCommand(string[] tokens)
        {
            switch (tokens[0])
            {
                case "SpawnJunk": SpawnJunk(tokens); break;
                case "DropBlocks": DropBlocks(tokens); break;
                case "Spike": Spike(tokens); break;
                case "Neutralize": Neutralize(tokens); break;
                case "Dialogue": Dialogue(tokens); break;
                case "Armor": Armor(tokens); break;
                case "Calibrate": Calibrate(tokens); break;
                case "Push": Push(tokens); break;
                case "Break": Break(tokens); break;
                default: return false;
            }

            return true;
        }

        public override string ParseParameter(string parameter)
        {
            return parameter;
        }

        public void SpawnJunk(string[] tokens)
        {
            int minimum = int.Parse(tokens[1]);
            int maximum = int.Parse(tokens[2]);

            matchScene.MatchBoard.SpawnJunk(Rng.RandomInt(minimum, maximum));
        }

        public void DropBlocks(string[] tokens)
        {
            TileColor tileColor = (TileColor)Enum.Parse(typeof(TileColor), tokens[1]);
            int magnitude = int.Parse(tokens[2]);

            matchScene.MatchBoard.DumpTiles(tileColor, magnitude);
        }

        public void Spike(string[] tokens)
        {
            TileColor tileColor = (TileColor)Enum.Parse(typeof(TileColor), tokens[1]);
            int magnitude = int.Parse(tokens[2]);

            matchScene.MatchBoard.Spike(tileColor, magnitude);
        }

        public void Neutralize(string[] tokens)
        {
            TileColor tileColor = (TileColor)Enum.Parse(typeof(TileColor), tokens[1]);

            matchScene.MatchBoard.Neutralize(tileColor);
        }

        private void Dialogue(string[] tokens)
        {
            string speaker = tokens[1] == "Blank" ? "" : tokens[1];
            string dialogue = tokens[2] == "Blank" ? "" : string.Join(' ', tokens.Skip(2));
            matchScene.GameViewModel.SetDialogue(speaker, dialogue);
        }

        private void Armor(string[] tokens)
        {
            matchScene.Enemy.Armor = int.Parse(tokens[1]);
        }

        private void Calibrate(string[] tokens)
        {
            var colors = Enum.GetValues<TileColor>().ToList().FindAll(x => x != TileColor.Junk);
            TileColor newImmune1 = colors[Rng.RandomInt(0, colors.Count - 1)];
            var newColors = colors.ToList().FindAll(x => x != newImmune1);
            TileColor newImmune2 = newColors[Rng.RandomInt(0, newColors.Count - 1)];

            matchScene.Enemy.Immune = new TileColor[] { newImmune1, newImmune2 };

            matchScene.GameViewModel.SetDialogue("", String.Format("Immune to {0} and {1} blocks!", matchScene.Enemy.Immune[0], matchScene.Enemy.Immune[1]));
        }

        private void Push(string[] tokens)
        {
            matchScene.MatchBoard.ChargeUp();
        }

        private void Break(string[] tokens)
        {
            TileColor tileColor = (TileColor)Enum.Parse(typeof(TileColor), tokens[1]);
            matchScene.MatchBoard.Break(tileColor);
        }
    }
}
