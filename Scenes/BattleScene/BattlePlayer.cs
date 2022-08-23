using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;

namespace Texemon.Scenes.BattleScene
{
    public class BattlePlayer : Battler
    {
        public const int HERO_WIDTH = 16;
        public const int HERO_HEIGHT = 21;

        private static Texture2D HERO_SHADOW;
        private static Effect PLAYER_BATTLER_EFFECT;

        protected enum HeroAnimation
        {
            Ready,
            Victory,
            Guarding,
            Attack,
            Chanting,
            Spell,
            Hit,
            Hurting,
            Dead
        }

        public static readonly Dictionary<string, Animation> HERO_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { HeroAnimation.Ready.ToString(), new Animation(0, 4, HERO_WIDTH, HERO_HEIGHT, 4, 200) },
            { HeroAnimation.Victory.ToString(), new Animation(0, 5, HERO_WIDTH, HERO_HEIGHT, 4, 200) },
            { HeroAnimation.Guarding.ToString(), new Animation(0, 6, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.Attack.ToString(), new Animation(0, 6, HERO_WIDTH, HERO_HEIGHT, 4, 80) },
            { HeroAnimation.Chanting.ToString(), new Animation(0, 7, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.Spell.ToString(), new Animation(0, 7, HERO_WIDTH, HERO_HEIGHT, 3, 80) },
            { HeroAnimation.Hit.ToString(), new Animation(0, 8, HERO_WIDTH, HERO_HEIGHT, 1, 600) },
            { HeroAnimation.Hurting.ToString(), new Animation(1, 8, HERO_WIDTH, HERO_HEIGHT, 1, 1000) },
            { HeroAnimation.Dead.ToString(), new Animation(2, 8, HERO_WIDTH, HERO_HEIGHT, 1, 1000) }
        };

        private HeroProfile heroProfile;
        private ClassType heroClass;
        private int partyOrder;

        private int labelX;
        private int labelY;
        private int lastCommandIndex = 0;
        private CommandCategory commandCategory = CommandCategory.Equipment;
        private WidgetMenuModel commandMenu;
        private TabIndicator categoryTabs;
        private ScrollPanel descriptionPanel;

        public BattlePlayer(BattleScene iBattleScene, Vector2 iPosition, HeroProfile iHeroProfile, int iPartyOrder)
            : base(iBattleScene, iPosition, HERO_SPRITES[iHeroProfile.Class], HERO_ANIMATIONS, iHeroProfile.Stats)
        {
            heroProfile = iHeroProfile;
            heroClass = heroProfile.Class;
            partyOrder = iPartyOrder;

            shader = PLAYER_BATTLER_EFFECT.Clone();
            shader.Parameters["flashInterval"].SetValue(0.0f);

            shadow = HERO_SHADOW;

            name = heroProfile.Name;
            health = heroProfile.Health;
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            DrawShadow(spriteBatch, camera);
        }

        public override void DrawShader(SpriteBatch spriteBatch, Camera camera, Matrix matrix)
        {
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, shader, matrix);
            animatedSprite.Draw(spriteBatch, position - new Vector2(0.0f, positionZ), camera, 0.0f);
            spriteBatch.End();
        }

        public void DrawOverlay(SpriteBatch spriteBatch)
        {
            Color healthColor = Color.White;
            float healthPercentage = (float)health / heroProfile.Stats.maxHealth;
            if (healthPercentage <= 0.5f) healthColor = Color.Yellow;
            else if (healthPercentage <= 0.25f) healthColor = Color.Red;

            string hpString = health + "/" + heroProfile.Stats.maxHealth;
            int labelWidth = Text.GetStringLength(GameFont.Menu, hpString) + 3;
            int labelHeight = Text.GetStringHeight(GameFont.Menu);
            switch (partyOrder)
            {
                case 0: labelX = SpriteBounds.Left - labelWidth; labelY = SpriteBounds.Top; break;
                case 1: labelX = SpriteBounds.Right; labelY = SpriteBounds.Top; break;
                case 2: labelX = SpriteBounds.Left - labelWidth; labelY = SpriteBounds.Bottom; break;
                case 3: labelX = SpriteBounds.Right; labelY = SpriteBounds.Bottom; break;
            }

            Textbox.DrawLabel(spriteBatch, Color.White, labelX, labelY, labelWidth, labelHeight, 0.08f);
            Text.DrawText(spriteBatch, new Vector2(labelX + 2, labelY), GameFont.Menu, health.ToString(), healthColor, 0.05f);
            Text.DrawText(spriteBatch, new Vector2(labelX + 2, labelY), GameFont.Menu, hpString, 0.06f);
        }

        public void SaveState()
        {
            heroProfile.Health = health;
        }

        public override void StartTurn()
        {
            base.StartTurn();

            PlayAnimation("Ready");

            commandMenu?.Terminate();
            Rectangle commandRectangle = new Rectangle(-142, 45, 89, 59);
            Textbox commandTextbox = new Textbox(new Rectangle(-142, 45, 187, 59), false, 0.06f);
            commandTextbox.WindowDepth = 2.0f;
            commandMenu = new WidgetMenuModel(commandRectangle, false, false, PriorityLevel.GameLevel);
            PopulateCommandWidgets();
            commandMenu.MenuView.AddTextbox(commandTextbox);

            commandMenu.MenuController.OnNewSelection += NewSelection;
            commandMenu.MenuController.OnLeft += PreviousCommandList;
            commandMenu.MenuController.OnRight += NextCommandList;

            battleScene.AddMenu(commandMenu);
        }

        public override void EndTurn(int initiativeModifier = 0)
        {
            base.EndTurn(initiativeModifier);

            commandMenu.Terminate();
        }

        public override void Damage(int damage)
        {
            base.Damage(damage);

            PlayAnimation("Hit", Idle);
        }

        public override void Animate(string animationName)
        {
            PlayAnimation(animationName, Idle);
        }

        public void Idle()
        {
            if (health > heroProfile.Stats.maxHealth / 4) PlayAnimation("Guarding");
            else if (health > 0) PlayAnimation("Hurting");
            else PlayAnimation("Dead");
        }

        private void PopulateCommandWidgets()
        {
            foreach (CommandEntry commandEntry in heroProfile.Commands[commandCategory])
            {
                commandMenu.AddWidget(new ListItem(commandMenu.FlowPosition, commandEntry.Name, CommandSelected, commandEntry, commandEntry.Icon));
            }

            string categoryHeader = CommandEntry.COMMAND_CATEGORY_NAMES[commandCategory];
            List<string> categoryNames = new List<string>();
            List<Color> categoryColors = new List<Color>();
            foreach (KeyValuePair<CommandCategory, List<CommandEntry>> commandCategory in heroProfile.Commands)
            {
                categoryNames.Add(CommandEntry.COMMAND_CATEGORY_NAMES[commandCategory.Key]);
                categoryColors.Add(commandCategory.Value.Any() ? Color.White : Color.DarkSlateGray);
            }

            categoryTabs = new TabIndicator(new Rectangle(0, -13, BattleScene.COMMAND_WINDOW.Width, 16), categoryNames, categoryHeader, 0.95f);
            categoryTabs.HeaderColors = categoryColors;

            commandMenu.SelectedWidget = commandMenu.WidgetList[lastCommandIndex];

            descriptionPanel = new ScrollPanel(new Rectangle(92, 8, 70, 60), ((commandMenu.SelectedWidget as ListItem).Item as CommandEntry).Description, GameFont.Menu, Alignment.Left);

            commandMenu.AddWidget(categoryTabs);
            commandMenu.AddWidget(descriptionPanel);
        }

        private void PreviousCommandList()
        {
            Array commandCategories = Enum.GetValues(typeof(CommandCategory));

            do
            {
                if ((int)commandCategory == 0)
                {
                    commandCategory = (CommandCategory)commandCategories.GetValue(commandCategories.Length - 1);
                }
                else commandCategory--;
            } while (heroProfile.Commands[commandCategory].Count == 0);

            lastCommandIndex = 0;
            commandMenu.ClearWidgets();
            PopulateCommandWidgets();

            Audio.PlaySound(GameSound.Cursor, 0.4f);
        }

        private void NextCommandList()
        {
            Array commandCategories = Enum.GetValues(typeof(CommandCategory));

            do
            {
                if ((int)commandCategory == commandCategories.Length - 1)
                {
                    commandCategory = (CommandCategory)0;
                }
                else commandCategory++;
            } while (heroProfile.Commands[commandCategory].Count == 0);

            lastCommandIndex = 0;
            commandMenu.ClearWidgets();
            PopulateCommandWidgets();

            Audio.PlaySound(GameSound.Cursor, 0.4f);
        }

        private void NewSelection()
        {
            descriptionPanel.ScrollText = ((commandMenu.SelectedWidget as ListItem).Item as CommandEntry).Description;
        }

        private void CommandSelected(object selectedItem)
        {
            CommandEntry selectedCommand = selectedItem as CommandEntry;
            PlayAnimation(selectedCommand.Animation);

            TargetController targetController = new TargetController(battleScene, this, selectedCommand);
            commandMenu.MenuController.AddChild(targetController);

            lastCommandIndex = commandMenu.WidgetList.IndexOf(commandMenu.SelectedWidget);
        }

        public HeroProfile HeroProfile { get => heroProfile; }
    }
}
