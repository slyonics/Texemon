using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Texemon.SceneObjects.Particles;

namespace Texemon.Scenes.MapScene
{
    public enum EmoteType
    {
        Exclamation,
        Question
    }

    public class EmoteParticle : Particle
    {
        private static Dictionary<EmoteType, Texture2D> PARTICLE_SPRITES = new Dictionary<EmoteType, Texture2D>();
        private static Dictionary<string, Animation> PARTICLE_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { EmoteType.Exclamation.ToString(), new Animation(0, 0, 34, 32, 1, 1000) },
            { EmoteType.Question.ToString(), new Animation(0, 0, 34, 32, 1, 1000) }
        };

        private Actor actor;

        public EmoteParticle(Scene iScene, EmoteType iEmoteType, Actor iActor)
            : base(iScene, iActor.Position - new Vector2(0, iActor.BoundingBox.Height), PARTICLE_SPRITES[iEmoteType], PARTICLE_ANIMATIONS, true)
        {
            actor = iActor;

            animatedSprite.PlayAnimation(iEmoteType.ToString(), AnimationFinished);
        }

        public override void Update(GameTime gameTime)
        {
            if (actor.Terminated) terminated = true;
            else
            {
                position = new Vector2(actor.Position.X, actor.Position.Y - actor.BoundingBox.Height - actor.PositionZ);
                base.Update(gameTime);
            }
        }

        public void AnimationFinished()
        {
            terminated = true;
        }
    }
}
