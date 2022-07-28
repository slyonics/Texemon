using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texemon.Scenes;
using Texemon.Scenes.MatchScene;

namespace Texemon.Scenes.MatchScene
{
    public class AttackPentagram : Particle
    {
        private const int ORB_SIZE = 128;

        private static readonly Dictionary<TileColor, GameSprite> spritelookup = new Dictionary<TileColor, GameSprite>()
        {
            { TileColor.Blue, GameSprite.SpellBlue },
            { TileColor.Cyan, GameSprite.SpellCyan },
            { TileColor.Green, GameSprite.SpellGreen },
            { TileColor.Red, GameSprite.SpellRed },
            { TileColor.Yellow, GameSprite.SpellYellow }
        };

        private Dictionary<string, Animation> ORB_ANIMS = new Dictionary<string, Animation>()
        {
            { "Idle", new Animation(0, 0, ORB_SIZE, ORB_SIZE, 1, 150) }
        };

        private float rotation;
        private float rotationSpeed = 1000;

        public TileColor tileColor;

        private List<AttackOrb> attackOrbs = new List<AttackOrb>();

        private Action terminateAction;

        public AttackPentagram(Scene iScene, Vector2 iPosition, TileColor color)
            : base(iScene, iPosition, false)
        {
            tileColor = color;
            animatedSprite = new AnimatedSprite(AssetCache.SPRITES[spritelookup[tileColor]], ORB_ANIMS);
            animatedSprite.SpriteColor = new Color(255, 255, 255, 128);


        }

        public override void Update(GameTime gameTime)
        {
            UpdatePosition(gameTime);
            UpdateElevation(gameTime);

            rotation += gameTime.ElapsedGameTime.Milliseconds / rotationSpeed;

            attackOrbs.RemoveAll(x => x.Terminated);
            if (attackOrbs.Count == 0)
            {
                terminated = true;
                terminateAction.Invoke();
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            animatedSprite.Rotation = rotation;
            animatedSprite.Scale = new Vector2(CrossPlatformGame.Scale);
            animatedSprite.Draw(spriteBatch, position + new Vector2(ORB_SIZE / 2, ORB_SIZE / 2), camera, 0.20f);
        }

        private bool orbDir;
        public void SpeedUp(int speed, int totalSpeed)
        {
            rotationSpeed -= speed * 50;

            if (rotationSpeed < 150) rotationSpeed = 150;

            attackOrbs.Add(parentScene.AddParticle(new AttackOrb(parentScene, this.position, this.tileColor, orbDir)));
            orbDir = !orbDir;

            foreach (AttackOrb attackOrb in attackOrbs) attackOrb.SpeedUp(totalSpeed);
        }

        public void Attack(Action action)
        {
            if (terminateAction != null) return;

            foreach (AttackOrb attackOrb in attackOrbs) attackOrb.Attack(this.position);
            terminateAction = action;


        }
    }
}
