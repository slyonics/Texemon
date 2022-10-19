using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Texemon.Main;

namespace Texemon.GameObjects
{
    public enum GameBackdrop
    {
        Canyon
    }

    public class LayeredBackdrop
    {
        public class Layer
        {
            public Texture2D background;
            public Vector2 offset;
            public float speed;

            public Layer(Texture2D initialBackground, float initialSpeed)
            {
                background = initialBackground;
                offset = new Vector2();
                speed = initialSpeed;
            }
        }

        private const float STARTING_DEPTH = 0.9f;

        private static readonly Dictionary<GameBackdrop, float[]> BACKGROUND_SCROLLING_SPEEDS = new Dictionary<GameBackdrop, float[]>()
        {
            { GameBackdrop.Canyon, new float[] { 0.5f, 0.08f, 0.06f, 0.04f } }
        };

        private static Dictionary<GameBackdrop, Texture2D[]> backgroundList = new Dictionary<GameBackdrop, Texture2D[]>();

        private GameBackdrop levelBackground;
        private List<Layer> layerList = new List<Layer>();
        private int backdropHeight;
        private Color color = Color.White;

        public LayeredBackdrop(GameBackdrop initialLevelBackground)
        {
            levelBackground = initialLevelBackground;

            int layerCount = BACKGROUND_SCROLLING_SPEEDS[(GameBackdrop)levelBackground].Length;

            backdropHeight = 0;
            layerList = new List<Layer>();
            for (int i = 0; i < layerCount; i++)
            {
                layerList.Add(new Layer(backgroundList[levelBackground][i], BACKGROUND_SCROLLING_SPEEDS[levelBackground][i]));
                if (backgroundList[levelBackground][i].Height > backdropHeight) backdropHeight = backgroundList[levelBackground][i].Height;
            }
        }

        public static void LoadContent(ContentManager contentManager)
        {
            foreach (GameBackdrop levelBackground in Enum.GetValues(typeof(GameBackdrop)))
            {
                int layerCount = BACKGROUND_SCROLLING_SPEEDS[levelBackground].Length;

                backgroundList[levelBackground] = new Texture2D[layerCount];
                for (int i = 0; i < layerCount; i++)
                {
                    backgroundList[levelBackground][i] = (contentManager.Load<Texture2D>("Graphics//" + Enum.GetName(typeof(GameBackdrop), levelBackground) + "//layer" + i));
                }
            }
        }

        public void Update(GameTime gameTime, Camera camera)
        {
            foreach (Layer layer in layerList)
            {
                layer.offset.X = -((Math.Max(camera.Position.X, 0) * layer.speed) % layer.background.Width);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            float depthOffset = 0.0f;

            foreach (Layer layer in layerList)
            {
                if (layer.speed > 0.01f)
                {
                    for (int i = 0; i < Math.Max(2, CrossPlatformGame.ScreenWidth / layer.background.Width); i++)
                    {
                        spriteBatch.Draw(layer.background, layer.offset + new Vector2(layer.background.Width * i, backdropHeight - layer.background.Height), null, color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, STARTING_DEPTH + depthOffset);
                    }
                }
                else
                {
                    spriteBatch.Draw(layer.background, layer.offset + new Vector2(0, backdropHeight - layer.background.Height), null, color, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, STARTING_DEPTH + depthOffset);
                }

                depthOffset += 0.01f;
            }
        }

        public Color Color { set => color = value; }
    }
}
