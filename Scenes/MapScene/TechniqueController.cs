using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Texemon.SceneObjects.Controllers;

namespace Texemon.Scenes.MapScene
{
    public class BufferedInput
    {
        public TechniqueData techniqueData;
        public Vector2 techniqueMovement;
        public int age;

        public BufferedInput(TechniqueData iTechniqueData, Vector2 iTechniqueMovement)
        {
            techniqueData = iTechniqueData;
            techniqueMovement = iTechniqueMovement;
        }
    }

    public class TechniqueController : Controller, IScripted
    {
        private MapScene mapScene;
        private Actor actor;
        private TechniqueData techniqueData;
        private Vector2 techniqueMovement;

        private ScriptParser scriptParser;

        private List<Bullet> projectiles = new List<Bullet>();
        private List<Bullet> meleeBullets = new List<Bullet>();

        private Vector2[] spline;
        private float splineTime;
        private float splineLength;

        private bool allowWalk;
        private bool techniqueCancellable;
        private bool evadeCancellable;
        private int maxCancelAge;
        private Dictionary<TechniqueData, TechniqueData> combos = new Dictionary<TechniqueData, TechniqueData>();

        public TechniqueController(MapScene iMapScene, Actor iActor, TechniqueData iTechniqueData, Vector2 iTechniqueMovement)
            : base(PriorityLevel.GameLevel)
        {
            mapScene = iMapScene;
            actor = iActor;
            techniqueData = iTechniqueData;
            techniqueMovement = iTechniqueMovement;

            iActor.ControllerList.Add(this);
            iActor.Poise = techniqueData.poise;

            scriptParser = new ScriptParser(mapScene, this);
            scriptParser.RunScript(techniqueData.script);
        }

        public override void PreUpdate(GameTime gameTime)
        {
            if (actor.Terminated || actor.Flinching || scriptParser.Terminated)
            {
                Player player = actor as Player;
                if (player != null) player.Poise = player.PlayerProfile.IdlePoise;
                Terminate();
                return;
            }

            meleeBullets.RemoveAll(x => x.Terminated);
            projectiles.RemoveAll(x => x.Terminated);

            scriptParser.Update(gameTime);

            if (spline != null)
            {
                splineTime += gameTime.ElapsedGameTime.Milliseconds;

                bool finished = splineTime >= splineLength;
                if (finished) splineTime = splineLength;

                actor.DesiredVelocity = Vector2.CatmullRom(spline[0], spline[1], spline[2], spline[3], splineTime / splineLength);

                if (finished) spline = null;
            }
        }

        public override void PostUpdate(GameTime gameTime)
        {
            foreach (Bullet bullet in meleeBullets) bullet.ProjectFrom(actor);
        }

        public BufferedInput ProcessBufferedInput(List<BufferedInput> bufferedInputList)
        {
            BufferedInput result = null;

            foreach (BufferedInput bufferedInput in bufferedInputList)
            {
                bool isCombo = combos.ContainsKey(bufferedInput.techniqueData);

                if (bufferedInput.age < maxCancelAge &&
                    (isCombo || techniqueCancellable || (evadeCancellable && bufferedInput.techniqueData.evasive)) &&
                    (result == null || bufferedInput.age < result.age))
                {
                    if (isCombo) result = new BufferedInput(combos[bufferedInput.techniqueData], bufferedInput.techniqueMovement);
                    else result = bufferedInput;
                }
            }

            return result;
        }

        public override void Terminate()
        {
            base.Terminate();

            foreach (Bullet melee in meleeBullets) melee.Terminate();
            meleeBullets.Clear();
        }

        public void ParseTokens(string[] tokens)
        {
            switch (tokens[0])
            {
                case "Idle": actor.Idle(); break;
                case "Melee": Melee(tokens); break;
                case "StepAhead": StepAhead(tokens); break;
                case "DodgeRoll": DodgeRoll(tokens); break;
                case "OrientedAnimation": OrientedAnimation(tokens); break;
                case "Animation": Animation(tokens); break;
                case "AllowWalk": allowWalk = scriptParser.ParseBool(tokens[1]); break;
                case "Cancellable": Cancellable(tokens); break;
            }
        }

        public bool ParseBool(string token)
        {
            return false;
        }

        public int ParseInt(string token)
        {
            return -1;
        }

        private void Melee(string[] tokens)
        {
            BulletData bulletData = techniqueData.bullets.First(x => x.name == tokens[1]);
            int bulletId = (int)Enum.Parse(typeof(Bullet.BulletType), bulletData.type);
            Bullet bullet = new Bullet(mapScene, actor.Position, bulletData, bulletId);

            bullet.ProjectFrom(actor);
            mapScene.Add(bullet);
            meleeBullets.Add(bullet);
        }

        private void StepAhead(string[] tokens)
        {
            float stepSpeed = float.Parse(tokens[1]);
            float slowDown = float.Parse(tokens[2]);

            Vector2 normal = techniqueMovement;
            if (normal.Length() > 0.001f) normal.Normalize();
            else normal = stepSpeed * Actor.ORIENTATION_UNIT_VECTORS[(int)actor.Orientation];

            spline = new Vector2[4]
            {
                normal,
                normal * slowDown,
                normal * slowDown * 0.5f,
                new Vector2()
            };

            splineTime = 0.0f;
            splineLength = float.Parse(tokens[3]);

            actor.Reorient(normal);
        }

        private void DodgeRoll(string[] tokens)
        {
            Vector2 normal = techniqueMovement;
            if (normal.Length() > 0.001f) normal.Normalize();
            else normal = Actor.ORIENTATION_UNIT_VECTORS[(int)actor.Orientation];

            spline = new Vector2[4]
            {
                normal * float.Parse(tokens[1]),
                normal * float.Parse(tokens[2]),
                normal * float.Parse(tokens[3]),
                normal * float.Parse(tokens[4])
            };

            splineTime = 0.0f;
            splineLength = float.Parse(tokens[5]);

            actor.Reorient(normal);
        }

        private void OrientedAnimation(string[] tokens)
        {
            bool waitForAnimation = tokens[2] == "Block";

            if (waitForAnimation) actor.OrientedAnimation(tokens[1], scriptParser.BlockForAnimation());
            else actor.OrientedAnimation(tokens[1]);
        }

        private void Animation(string[] tokens)
        {
            bool waitForAnimation = tokens[2] == "Block";

            if (waitForAnimation) actor.PlayAnimation(tokens[1], scriptParser.BlockForAnimation());
            else actor.PlayAnimation(tokens[1]);
        }

        private void Cancellable(string[] tokens)
        {
            switch (tokens[1])
            {
                case "Evasion": evadeCancellable = true; techniqueCancellable = false; maxCancelAge = scriptParser.ParseInt(tokens[2]); break;
                case "Technique": evadeCancellable = techniqueCancellable = true; maxCancelAge = scriptParser.ParseInt(tokens[2]); break;
                default:
                    combos.Add(TechniqueData.Data.First(x => x.name == tokens[1]), TechniqueData.Data.First(x => x.name == tokens[2]));
                    maxCancelAge = scriptParser.ParseInt(tokens[3]);
                    break;
            }
        }

        public TechniqueData Data { get => techniqueData; }
        public bool AllowWalk { get => allowWalk; }
    }
}
