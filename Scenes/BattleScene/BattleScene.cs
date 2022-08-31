using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

using Texemon.Models;

namespace Texemon.Scenes.BattleScene
{
    public class BattleScene : Scene
    {
        public static List<EnemyRecord> ENEMIES { get; private set; }
        public static List<EncounterRecord> ENCOUNTERS { get; private set; }

        private static readonly Rectangle[] BACKGROUND_SOURCE = new Rectangle[]
        {
            new Rectangle(0, 0, 16, 112),
            new Rectangle(16, 0, 16, 112),
            new Rectangle(32, 0, 16, 112),
        };

        private EncounterRecord encounterRecord;

        public RenderTarget2D backgroundRender;

        private BattleViewModel battleViewModel;

        private List<Battler> initiativeList = new List<Battler>();

        private bool introFinished = false;

        private GameMusic mapMusic;


        protected List<Particle> overlayParticleList = new List<Particle>();

        public BattleScene(string encounterName)
        {
            encounterRecord = ENCOUNTERS.First(x => x.Name == encounterName);
            
            string[] enemyTokens = encounterRecord.Enemies;
            List<Texture2D> enemySpriteList = new List<Texture2D>();
            int totalEnemyWidth = 0;
            foreach (string enemyName in enemyTokens)
            {
                EnemyRecord enemyData = ENEMIES.First(x => x.Name == enemyName);
                Texture2D enemySprite = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Enemies_" + enemyData.Sprite)];
                totalEnemyWidth += enemySprite.Width;
            }
            BuildBackground(AssetCache.SPRITES[GameSprite.Background_Trees], totalEnemyWidth);

            battleViewModel = AddView(new BattleViewModel(this, ENCOUNTERS.First(x => x.Name == encounterName)));
        }

        ~BattleScene()
        {
            backgroundRender.Dispose();
        }

        public override void BeginScene()
        {
            sceneStarted = true;

            mapMusic = Audio.CurrentMusic;
            Audio.PlayMusic(GameMusic.Battle);            
        }

        public override void EndScene()
        {
            Audio.PlayMusic(mapMusic);

            base.EndScene();
        }

        public static void Initialize()
        {
            ENEMIES = AssetCache.LoadRecords<EnemyRecord>("EnemyData");
            ENCOUNTERS = AssetCache.LoadRecords<EncounterRecord>("EncounterData");

            BattleEnemy.Initialize();
        }

        private void BuildBackground(Texture2D backgroundImage, int width)
        {
            backgroundRender = new RenderTarget2D(CrossPlatformGame.GameInstance.GraphicsDevice, width, 112);
            CrossPlatformGame.GameInstance.GraphicsDevice.SetRenderTarget(backgroundRender);
            CrossPlatformGame.GameInstance.GraphicsDevice.Clear(Color.Transparent);
            SpriteBatch spriteBatch = new SpriteBatch(CrossPlatformGame.GameInstance.GraphicsDevice);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, null);
            spriteBatch.Draw(backgroundImage, Vector2.Zero, BACKGROUND_SOURCE[0], Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.99f);
            for (int i = 1; i < width / 16; i++) spriteBatch.Draw(backgroundImage, new Vector2(i * 16, 0), BACKGROUND_SOURCE[1], Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.99f);
            spriteBatch.Draw(backgroundImage, new Vector2(width, 0) - new Vector2(16, 0), BACKGROUND_SOURCE[2], Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.99f);
            spriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            int i = 0;
            while (i < overlayParticleList.Count) { overlayParticleList[i].Update(gameTime); i++; }
            overlayParticleList.RemoveAll(x => x.Terminated);

            if (battleViewModel.EnemyPanel.Transitioning) return;
            else if (!introFinished)
            {
                introFinished = true;
                var convoRecord = new ConversationScene.ConversationRecord()
                {
                    DialogueRecords = new ConversationScene.DialogueRecord[] {
                            new ConversationScene.DialogueRecord() { Text = encounterRecord.Intro }
                        }
                };
                var convoScene = new ConversationScene.ConversationScene(convoRecord, new Rectangle(-20, 30, 170, 80));
                CrossPlatformGame.StackScene(convoScene);
            }

            EnemyList.RemoveAll(x => x.Terminated);
            if (!controllerList.Any(y => y.Exists(x => x is BattleController)) && !PlayerList.Exists(x => x.Busy) && !EnemyList.Exists(x => x.Busy) && CrossPlatformGame.CurrentScene == this)
            {
                if (EnemyList.Count == 0)
                {
                    var convoRecord = new ConversationScene.ConversationRecord()
                    {
                        DialogueRecords = new ConversationScene.DialogueRecord[] {
                            new ConversationScene.DialogueRecord() { Text = "Victory!" }
                        }
                    };
                    var convoScene = new ConversationScene.ConversationScene(convoRecord, new Rectangle(-20, 30, 170, 80));
                    convoScene.OnTerminated += new TerminationFollowup(EndScene);
                    CrossPlatformGame.StackScene(convoScene);
                }
                else ActivateNextBattler();
            }
        }

        public override void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, RenderTarget2D pixelRender, RenderTarget2D compositeRender)
        {
            graphicsDevice.SetRenderTarget(pixelRender);
            graphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, null);
            DrawBackground(spriteBatch);
            spriteBatch.End();

            Matrix matrix = (Camera == null) ? Matrix.Identity : Camera.Matrix;
            Effect shader = (spriteShader == null) ? null : spriteShader.Effect;
            foreach (Entity entity in entityList) entity.DrawShader(spriteBatch, Camera, matrix);
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, shader, matrix);
            DrawGame(spriteBatch, shader, matrix);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, null);
            DrawOverlay(spriteBatch);
            spriteBatch.End();

            foreach (BattleEnemy battleEnemy in EnemyList) battleEnemy.DrawShader(spriteBatch);
            foreach (BattlePlayer battlePlayer in PlayerList) battlePlayer.DrawShader(spriteBatch);

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, null);
            foreach (Particle particle in overlayParticleList) particle.Draw(spriteBatch, null);
            spriteBatch.End();

            graphicsDevice.SetRenderTarget(compositeRender);

            if (!CrossPlatformGame.ClearedCompositeRender)
            {
                CrossPlatformGame.ClearedCompositeRender = true;
                graphicsDevice.Clear(Color.Transparent);
            }

            shader = (sceneShader == null) ? null : sceneShader.Effect;
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, shader, Matrix.Identity);
            spriteBatch.Draw(pixelRender, Vector2.Zero, null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
            spriteBatch.End();
        }

        public void AddBattler(Battler battler)
        {
            EnqueueInitiative(battler);
            if (battler is BattleEnemy) EnemyList.Add(battler as BattleEnemy);
            else if (battler is BattlePlayer) PlayerList.Add(battler as BattlePlayer);
        }

        public override T AddParticle<T>(T newParticle)
        {
            if (newParticle.Foreground) overlayParticleList.Add(newParticle);
            else particleList.Add(newParticle);
            return newParticle;
        }

        public void ActivateNextBattler()
        {
            Battler readyBattler = initiativeList.First();
            int timeAdvance = readyBattler.ActionTime;
            foreach (Battler battler in initiativeList) battler.ActionTime -= timeAdvance;
            readyBattler.StartTurn();
        }

        public void EnqueueInitiative(Battler battler)
        {
            if (initiativeList.Count == 0)
            {
                initiativeList.Add(battler);
                return;
            }

            initiativeList.Remove(battler);
            int insertIndex = initiativeList.FindLastIndex(x => x.ActionTime <= battler.ActionTime);
            initiativeList.Insert(Math.Max(insertIndex + 1, initiativeList.Count - 1), battler);
        }

        public List<Battler> InitiativeList { get => initiativeList; }
        public List<BattlePlayer> PlayerList { get; } = new List<BattlePlayer>();
        public List<BattleEnemy> EnemyList { get; } = new List<BattleEnemy>();
        public BattleViewModel BattleViewModel { get => battleViewModel; }
    }
}
