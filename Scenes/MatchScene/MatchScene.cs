using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texemon.Models;
using Texemon.SceneObjects.Controllers;

namespace Texemon.Scenes.MatchScene
{
    public class MatchScene : Scene
    {
        public static Dictionary<string, Animation> ACTION_SWAP_ANIMS = new Dictionary<string, Animation>()
        {
            { "Idle", new Animation(0, 0, 56, 56, 1, 1000) },
            { "Disappearing", new Animation(0, 1, 56, 56, 6, 70) },
            { "Hidden", new Animation(1, 0, 56, 56, 1, 1000) }
        };

        private GameViewModel gameViewModel;
        private SceneObjects.Widgets.Panel floorPanel;

        private MatchBoard matchBoardLeft;
        private NinePatch leftFloor;
        private PlayerController cursorControllerLeft;

        private ModelCollection<ItemModel> startingItems;

        public MatchScene(string enemyName)
        {
            Enemy = EnemyModel.Models.First(x => x.Name == enemyName);
            Enemy.CurrentHealth = Enemy.MaxHealth;

            startingItems = new ModelCollection<ItemModel>();
            foreach (ModelProperty<ItemModel> itemModel in GameProfile.PlayerProfile.Items) startingItems.Add(new ItemModel(itemModel.Value));

            gameViewModel = AddView(new GameViewModel(this, GameView.MatchScene_GameView));
            floorPanel = gameViewModel.GetWidget<SceneObjects.Widgets.Panel>("FloorPanel");


            matchBoardLeft = new MatchBoard(this);
            leftFloor = new NinePatch("Windows_GamePanelOpaque", 0.1f);
            leftFloor.Bounds = floorPanel.OuterBounds;
            cursorControllerLeft = AddController(new PlayerController(this, matchBoardLeft));


            Audio.PlaySound(GameSound.enemy_encounter);
        }

        public MatchScene(string enemyName, string flag)
        {
            Enemy = EnemyModel.Models.First(x => x.Name == enemyName);
            Enemy.CurrentHealth = Enemy.MaxHealth;

            startingItems = new ModelCollection<ItemModel>();
            foreach (ModelProperty<ItemModel> itemModel in GameProfile.PlayerProfile.Items) startingItems.Add(new ItemModel(itemModel.Value));

            gameViewModel = AddView(new GameViewModel(this, GameView.MatchScene_GameView));
            floorPanel = gameViewModel.GetWidget<SceneObjects.Widgets.Panel>("FloorPanel");

            matchBoardLeft = new MatchBoard(this);
            leftFloor = new NinePatch("Windows_GamePanelOpaque", 0.1f);
            leftFloor.Bounds = floorPanel.OuterBounds;
            cursorControllerLeft = AddController(new PlayerController(this, matchBoardLeft));


            if (flag == "NoFlee") gameViewModel.CanFlee.Value = false;

            Audio.PlaySound(GameSound.enemy_encounter);
        }

        public override void BeginScene()
        {
            sceneStarted = true;

            Audio.PlayMusic(GameMusic.SMP_BTL);

            gameViewModel.mapScene.MapViewModel.ShowHealthBar.Value = true;
            gameViewModel.mapScene.MapViewModel.CurrentHealthBar.Value = gameViewModel.mapScene.MapViewModel.MaxHealthBar.Value = Enemy.MaxHealth;
        }

        public override void Update(GameTime gameTime, PriorityLevel priorityLevel = PriorityLevel.GameLevel)
        {
            matchBoardLeft.Update(gameTime);

            base.Update(gameTime, priorityLevel);
        }

        public override void DrawBackground(SpriteBatch spriteBatch)
        {
            leftFloor.Draw(spriteBatch, floorPanel.Position);

            int seconds = (matchBoardLeft.turnTimer / 1000);
            string secondsString = seconds.ToString("D2");

            int milliseconds = (matchBoardLeft.turnTimer - (seconds * 1000)) / 10;
            string millisecondsString = milliseconds.ToString("D2");

            Color timerColor = seconds >= 3 ? Color.White : Color.Lerp(Color.White, Color.Red, (float)(Math.Sin(matchBoardLeft.turnTimer / 100) / 2 + 0.5));

            var font = (CrossPlatformGame.Scale > 1) ? GameFont.BigTimer : GameFont.Timer;
            if (((bool)GameViewModel.PlayerTurn.Value) == true) Text.DrawText(spriteBatch, floorPanel.Position + new Vector2(floorPanel.InnerBounds.X + 2, floorPanel.InnerBounds.Y + 2 * CrossPlatformGame.Scale), font, " Player Turn  " + secondsString + ":" + millisecondsString, timerColor, 0.001f);
        }

        public override void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, RenderTarget2D pixelRender, RenderTarget2D compositeRender)
        {
            graphicsDevice.SetRenderTarget(CrossPlatformGame.GameInstance.matchRender);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, null);
            matchBoardLeft.Draw(spriteBatch, Camera);
            spriteBatch.End();

            base.Draw(graphicsDevice, spriteBatch, pixelRender, compositeRender);
        }

        public void EndMatch()
        {
            CrossPlatformGame.Transition(typeof(TitleScene.TitleScene));
        }

        public void Restart()
        {
            GameProfile.PlayerProfile.Items = new ModelCollection<ItemModel>();
            foreach (ModelProperty<ItemModel> itemModel in startingItems) GameProfile.PlayerProfile.Items.Add(new ItemModel(itemModel.Value));

            if (((bool)gameViewModel.CanFlee.Value) == true) CrossPlatformGame.Transition(typeof(MatchScene), Enemy.Name);
            else CrossPlatformGame.Transition(typeof(MatchScene), Enemy.Name, "NoFlee");
        }

        public void PromptRetry()
        {
            AddView(new GameOverViewModel(this, GameView.MatchScene_GameOverView));
        }

        public EnemyModel Enemy { get; private set; }
        public MatchBoard MatchBoard { get => matchBoardLeft; }
        public GameViewModel GameViewModel { get => gameViewModel; }
    }
}
