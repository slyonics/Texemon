global using Microsoft.Xna.Framework;
global using Texemon.Main;
global using Texemon.SceneObjects;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Texemon.SceneObjects.Controllers;
using Texemon.SceneObjects.Shaders;
using Texemon.Scenes.SplashScene;
using Texemon.Scenes.TitleScene;

namespace Texemon.Main
{
    public class CrossPlatformGame : Game
    {
        public const string CONTENT_DIRECTORY = "Assets";
        public const string GAME_NAME = "Texemon";
        public static readonly string SETTINGS_DIRECTORY = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "AppData\\Local") + "\\" + CrossPlatformGame.GAME_NAME;

        private const int WINDOWED_MARGIN = 34;
        public const int TARGET_SCREEN_WIDTH = 320;
        public const int TARGET_SCREEN_HEIGHT = 240;
        private const int MAXIMUM_SCREEN_WIDTH = 1920;
        private const int MAXIMUM_SCREEN_HEIGHT = 1080;

        public static readonly Color CLEAR_COLOR = new Color(100, 98, 93, 255);

        private GraphicsDeviceManager graphicsDeviceManager;
        private SpriteBatch spriteBatch;

        private RenderTarget2D gameRender;
        private RenderTarget2D compositeRender;

        private static int scaledScreenWidth = TARGET_SCREEN_WIDTH;
        private static int scaledScreenHeight = TARGET_SCREEN_HEIGHT;
        private static int screenScale = 1;
        private bool fullscreen = false;

        private static Shader transitionShader;
        private static Scene pendingScene;
        public static Scene CurrentScene { get; private set; }
        private static List<Scene> sceneStack = new List<Scene>();
        public static List<Scene> SceneStack { get => sceneStack; }

        private static CrossPlatformGame crossPlatformGame;

        public CrossPlatformGame()
        {
            crossPlatformGame = this;
            graphicsDeviceManager = new GraphicsDeviceManager(this);
            graphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = CONTENT_DIRECTORY;

            Exiting += CrossPlatformGame_Exiting;
        }

        protected override void UnloadContent()
        {
            Audio.Deinitialize();
        }

        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Settings.LoadSettings();
            Audio.ApplySettings();
            Input.ApplySettings();
            ApplySettings();

            base.Initialize();

            Debug.Initialize(GraphicsDevice);
            Audio.Initialize();
            Text.Initialize(GraphicsDevice);

            AssetCache.LoadContent(Content, GraphicsDevice);
            Scenes.ConversationScene.ConversationScene.Initialize();
            Scenes.BattleScene.BattleScene.Initialize();
            Scenes.StatusScene.StatusScene.Initialize();
            Scenes.ShopScene.ShopScene.Initialize();

            CurrentScene = new SplashScene();
        }

        public void ApplySettings()
        {
            fullscreen = Settings.GetProgramSetting<bool>("Fullscreen");
            string targetResolution = Settings.GetProgramSetting<string>("TargetResolution");

            if (fullscreen)
            {
                DisplayModeCollection displayModes = GraphicsDevice.Adapter.SupportedDisplayModes;
                IEnumerable<DisplayMode> bestModes = displayModes.Where(x => x.Width >= TARGET_SCREEN_WIDTH && x.Width <= MAXIMUM_SCREEN_WIDTH &&
                                                                             x.Height >= TARGET_SCREEN_HEIGHT && x.Height <= MAXIMUM_SCREEN_HEIGHT);

                DisplayMode targetMode = bestModes.OrderByDescending(x => x.Width).FirstOrDefault();
                scaledScreenWidth = targetMode.Width;
                scaledScreenHeight = targetMode.Height;
                int scale = targetMode.Height / TARGET_SCREEN_HEIGHT;
                screenScale = scale;
            }
            else
            {
                int availableHeight = GraphicsDevice.Adapter.CurrentDisplayMode.TitleSafeArea.Height - WINDOWED_MARGIN;
                int scale = availableHeight / TARGET_SCREEN_HEIGHT;

                screenScale = scale;
                scaledScreenWidth = TARGET_SCREEN_WIDTH * scale;
                scaledScreenHeight = TARGET_SCREEN_HEIGHT * scale;
            }

            IsMouseVisible = true; // !fullscreen;

            graphicsDeviceManager.IsFullScreen = fullscreen;
            graphicsDeviceManager.PreferredBackBufferWidth = scaledScreenWidth;
            graphicsDeviceManager.PreferredBackBufferHeight = scaledScreenHeight;
            graphicsDeviceManager.ApplyChanges();

            gameRender = new RenderTarget2D(graphicsDeviceManager.GraphicsDevice, TARGET_SCREEN_WIDTH, TARGET_SCREEN_HEIGHT);
            compositeRender = new RenderTarget2D(graphicsDeviceManager.GraphicsDevice, TARGET_SCREEN_WIDTH, TARGET_SCREEN_HEIGHT, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
        }

        private void CrossPlatformGame_Exiting(object sender, EventArgs e)
        {
            Settings.SaveSettings();
        }

        protected override void Update(GameTime gameTime)
        {
            Input.Update(gameTime);

            if (Settings.GetProgramSetting<bool>("DebugMode") && Input.CurrentInput.CommandPressed(Command.Menu))
            {
                Transition(typeof(TitleScene));
            }

            if (transitionShader != null)
            {
                CurrentScene.Update(gameTime);
                transitionShader.Update(gameTime, null);
                if (transitionShader.Terminated) transitionShader = null;
            }
            else
            {
                int i = 0;
                while (i < sceneStack.Count)
                {
                    sceneStack[i].Update(gameTime);
                    i++;
                }

                CurrentScene.Update(gameTime);
            }

            if (pendingScene != null)
            {
                sceneStack.Clear();
                CrossPlatformGame.SetCurrentScene(pendingScene);
                pendingScene = null;
            }

            while (CurrentScene.SceneEnded && sceneStack.Count > 0)
            {
                CurrentScene = sceneStack.Last();
                CurrentScene.ResumeScene();
                sceneStack.Remove(CurrentScene);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            ClearedCompositeRender = false;

            lock (sceneStack)
            {
                foreach (Scene scene in sceneStack)
                {
                    scene.Draw(GraphicsDevice, spriteBatch, gameRender, compositeRender);
                }
            }

            CurrentScene.Draw(GraphicsDevice, spriteBatch, gameRender, compositeRender);

            Effect shader = (transitionShader == null) ? null : transitionShader.Effect;
            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, shader, Matrix.CreateScale((int)Scale, (int)Scale, 1));
            spriteBatch.Draw(compositeRender, Vector2.Zero, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public static void ShowCursor(bool show = true)
        {
            GameInstance.IsMouseVisible = show;
        }

        public static void Transition(Type sceneType, params object[] args)
        {
            Transition(CurrentScene, sceneType, args);
        }

        public static void Transition(Scene parentScene, Type sceneType, params object[] args)
        {
            TransitionController transitionController = new TransitionController(TransitionDirection.Out, 600);
            ColorFade colorFade = new ColorFade(Color.Black, transitionController.TransitionProgress);
            transitionController.UpdateTransition += new Action<float>(t => colorFade.Amount = t);
            parentScene.AddController(transitionController);
            transitionShader = colorFade;

            Task.Run(() => Activator.CreateInstance(sceneType, args)).ContinueWith(t =>
            {
                while (!transitionController.Terminated) ;
                pendingScene = (Scene)t.Result;
            });
        }

        public static void SetCurrentScene(Scene newScene)
        {
            CurrentScene.EndScene();

            transitionShader.Terminate();
            CurrentScene = newScene;
            newScene.BeginScene();
        }

        public static void StackScene(Scene newScene)
        {
            lock (sceneStack)
            {
                sceneStack.Add(CurrentScene);
            }

            CurrentScene = newScene;
            newScene.BeginScene();
        }

        public static T GetScene<T>() where T : Scene
        {
            if (CurrentScene is T) return (T)CurrentScene;
            else
            {
                T result;
                lock (sceneStack)
                {
                    result = (T)sceneStack.FirstOrDefault(x => x is T);
                }

                return result;
            }
        }

        public Shader TransitionShader { set => transitionShader = value; }

        public static int ScreenWidth { get => TARGET_SCREEN_WIDTH; }
        public static int ScreenHeight { get => TARGET_SCREEN_HEIGHT; }
        public static int Scale { get => screenScale; }
        public static CrossPlatformGame GameInstance { get => crossPlatformGame; }
        public static bool ClearedCompositeRender { get; set; }
    }
}
