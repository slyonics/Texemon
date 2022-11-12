using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Texemon.Main;

namespace Texemon.SceneObjects.Particles
{
    public delegate void FrameFollowup();

    public enum AnimationType
    {
        Slash,
        Star,
        Heart,
        Sparks,
        Smoke,
        Flame,
        Gunshot,
        Bash,
        Electric,
        Shock,
        GreenHeal,
        BlueHeal
    }

    public class AnimationParticle : Particle
    {
        private static readonly Dictionary<string, Animation> PARTICLE_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { AnimationType.Slash.ToString(), new Animation(0, 0, 64, 64, 7, 50) },
            { AnimationType.Star.ToString(), new Animation(0, 0, 16, 16, 9, 80) },
            { AnimationType.Heart.ToString(), new Animation(0, 0, 256, 256, 30, 40, 1536) },
            { AnimationType.Sparks.ToString(), new Animation(0, 0, 48, 48, 12, 30) },
            { AnimationType.Flame.ToString(), new Animation(0, 0, 96, 96, 7, 90) },
            { AnimationType.Smoke.ToString(), new Animation(0, 0, 32, 32, 7, 50) },
            { AnimationType.Gunshot.ToString(), new Animation(0, 0, 48, 48, 7, 50) },
            { AnimationType.Bash.ToString(), new Animation(0, 0, 96, 96, 4, 50) },
            { AnimationType.Electric.ToString(), new Animation(0, 0, 96, 96, 6, 50) },
            { AnimationType.Shock.ToString(), new Animation(0, 0, 96, 96, 4, 70) },
            { AnimationType.GreenHeal.ToString(), new Animation(0, 0, 16, 48, 13, 30) },
            { AnimationType.BlueHeal.ToString(), new Animation(0, 0, 16, 48, 13, 30) },
        };

        private List<Tuple<int, FrameFollowup>> frameEventList = new List<Tuple<int, FrameFollowup>>();

        public AnimationParticle(Scene iScene, Vector2 iPosition, AnimationType iAnimationType, bool iForeground = false)
            : base(iScene, iPosition, iForeground)
        {
            parentScene = iScene;
            position = iPosition;
            animatedSprite = new AnimatedSprite(AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Particles_" + iAnimationType)], PARTICLE_ANIMATIONS);
            animatedSprite.PlayAnimation(iAnimationType.ToString(), AnimationFinished);

            if (iAnimationType == AnimationType.BlueHeal || iAnimationType == AnimationType.GreenHeal)
            {

            }
            else if (Foreground) position.Y += SpriteBounds.Height / 2;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Tuple<int, FrameFollowup> frameEvent = frameEventList.Find(x => x.Item1 == animatedSprite.Frame);
            if (frameEvent != null)
            {
                frameEvent.Item2();
                frameEventList.Remove(frameEvent);
            }
        }

        public void AnimationFinished()
        {
            terminated = true;
        }

        public void AddFrameEvent(int frame, FrameFollowup frameEvent)
        {
            frameEventList.Add(new Tuple<int, FrameFollowup>(frame, frameEvent));
        }
    }
}
