using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Texemon.Main;

namespace Texemon.SceneObjects.Shaders
{
    public class ColorFade : Shader
    {
        public ColorFade(Color color, float amount)
            : base(AssetCache.EFFECTS[GameShader.ColorFade].Clone())
        {
            Effect.Parameters["filterRed"].SetValue(color.R);
            Effect.Parameters["filterGreen"].SetValue(color.G);
            Effect.Parameters["filterBlue"].SetValue(color.B);
            Amount = amount;
        }

        public float Amount
        {
            set
            {
                Effect.Parameters["amount"].SetValue(value);
            }
        }
    }
}
