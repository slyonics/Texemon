using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texemon.SceneObjects
{
    public abstract class Particle : Entity
    {
        private bool foreground;

        public Particle(Scene iScene, Vector2 iPosition, Texture2D iSprite, Dictionary<string, Animation> iAnimationList, bool iForeground = false)
            : base(iScene, iPosition, iSprite, iAnimationList)
        {
            foreground = iForeground;

            if (foreground) position.Y += SpriteBounds.Height / 2;
        }

        public Particle(Scene iScene, Vector2 iPosition, bool iForeground = false)
            : base(iScene, iPosition)
        {
            foreground = iForeground;
        }

        public override float DepthPosition
        {
            get
            {
                if (foreground) return parentScene.Camera.MaxVisibleY;
                else return base.DepthPosition;
            }
        }
    }
}
