using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texemon.Models;
using Texemon.SceneObjects.Controllers;

namespace Texemon.Scenes.MatchScene
{
    public class MatchBoard : Entity
    {
        public class Combo
        {
            public List<MatchTile> tiles;
            public int chain = 1;
        }

        private static readonly Dictionary<string, Animation> CURSOR_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { "Default", new Animation(0, 0, 84, 44, 2, new int[] { 300, 150 }, 84) }
        };

        public const int TILE_ROWS = 18;
        public const int TILE_COLUMNS = 6;
        public const int BOARD_WIDTH = TILE_COLUMNS * MatchTile.TILE_SIZE;
        public const int BOARD_HEIGHT = TILE_ROWS * MatchTile.TILE_SIZE;

        private const float CURSOR_DEPTH = 0.3f;
        private const int SWAP_LENGTH = 100;
        private const int PUSH_TICK = 100;

        private MatchScene matchScene;

        private MatchTile[,] matchTiles = new MatchTile[TILE_COLUMNS, TILE_ROWS];
        private MatchTile[] upcomingTiles = new MatchTile[TILE_COLUMNS];
        private List<MatchTile> comboCandidates = new List<MatchTile>();

        private AnimatedSprite cursorSprite;

        private int pushCooldown = PUSH_TICK;
        private int turnGracePeriod = 0;

        public int TotalDamage { get; set; }

        private int swapTimeLeft = 0;

        private int rowsToPush = 0;

        AttackPentagram attackPentagram;
        public int turnTimer = 0;

        public SceneObjects.Widgets.Panel gamePanel;

        public MatchBoard(MatchScene iMatchScene)
            : base(iMatchScene, Vector2.Zero)
        {
            matchScene = iMatchScene;

            gamePanel = matchScene.GameViewModel.GetWidget<SceneObjects.Widgets.Panel>("GamePanel");

            List<TileColor> colors = Enum.GetValues<TileColor>().ToList();
            int[] startHeights = new int[TILE_COLUMNS];
            for (int x = 0; x < TILE_COLUMNS; x++)
            {
                startHeights[x] = Rng.RandomInt(2, 4);
                for (int y = TILE_ROWS - startHeights[x]; y < TILE_ROWS; y++)
                {
                    List<TileColor> availableColors = colors.FindAll(x => x != TileColor.Junk);

                    if (x >= 2)
                    {
                        MatchTile previousTile = matchTiles[x - 1, y];
                        MatchTile secondPreviousTile = matchTiles[x - 2, y];
                        if (previousTile != null && secondPreviousTile != null && previousTile.TileColor == matchTiles[x - 1, y].TileColor)
                            availableColors = availableColors.FindAll(c => c != matchTiles[x - 1, y].TileColor);
                    }

                    if (y >= 2)
                    {
                        MatchTile previousTile = matchTiles[x, y - 1];
                        MatchTile secondPreviousTile = matchTiles[x, y - 2];
                        if (previousTile != null && secondPreviousTile != null && previousTile.TileColor == matchTiles[x, y - 1].TileColor)
                            availableColors = availableColors.FindAll(c => c != matchTiles[x, y - 1].TileColor);
                    }

                    MatchTile matchTile = new MatchTile(matchScene, this, availableColors[Rng.RandomInt(0, availableColors.Count - 1)]);
                    matchTile.SetRestingPosition(x, y, position);
                    matchTiles[x, y] = matchTile;
                    matchTile.AppearingTimeLeft = Rng.RandomInt(1, 300);
                }
            }

            for (int x = 0; x < TILE_COLUMNS; x++)
            {
                List<TileColor> availableColors = colors.FindAll(x => x != TileColor.Junk);
                if (x >= 2 && upcomingTiles[x - 2].TileColor == upcomingTiles[x - 1].TileColor)
                    availableColors = availableColors.FindAll(c => c != upcomingTiles[x - 1].TileColor);

                MatchTile matchTile = new MatchTile(matchScene, this, availableColors[Rng.RandomInt(0, availableColors.Count - 1)]);
                matchTile.SetRestingPosition(x, TILE_ROWS, position);
                upcomingTiles[x] = matchTile;
            }

            cursorSprite = new AnimatedSprite(AssetCache.SPRITES[GameSprite.MatchCursor], CURSOR_ANIMATIONS);
        }

        public override void Update(GameTime gameTime)
        {
            if (matchScene.SceneEnded) return;

            base.Update(gameTime);

            comboCandidates.Clear();

            UpdateSwap(gameTime);

            cursorSprite?.Update(gameTime);

            bool tilesEliminated = false;
            for (int x = 0; x < TILE_COLUMNS; x++)
            {
                for (int y = TILE_ROWS - 1; y >= 0; y--)
                {
                    MatchTile matchTile = matchTiles[x, y];
                    if (matchTile != null)
                    {
                        matchTile.Update(gameTime);
                        if (!matchTile.Eliminating) matchTile.UpdateMovement(gameTime);
                    }
                }
            }

            for (int x = 0; x < TILE_COLUMNS; x++)
            {
                upcomingTiles[x].Update(gameTime);
            }

            bool tilesActive = false;
            for (int x = 0; x < TILE_COLUMNS; x++)
            {
                for (int y = TILE_ROWS - 1; y >= 0; y--)
                {
                    MatchTile matchTile = matchTiles[x, y];

                    if (matchTile != null && (matchTile.Sliding || matchTile.Eliminating || matchTile.Falling || matchTile.Breaking || matchTile.Appearing)) tilesActive = true;

                    if (matchTile != null && matchTile.Eliminating && matchTile.UpdateElimination(gameTime))
                    {
                        if (matchTile.TileColor != TileColor.Junk) tilesEliminated = true;

                        matchTiles[x, y] = null;

                        if (y > 0)
                        {
                            MatchTile neighborAbove = matchTiles[x, y - 1];
                            if (neighborAbove != null)
                            {
                                neighborAbove.Chain = matchTile.Chain;
                                //neighborAbove.StartFalling();
                            }
                        }
                    }
                }
            }

            if (tilesEliminated) Audio.PlaySound(GameSound.block_dissapear);


            if (turnGracePeriod > 0)
            {
                turnGracePeriod -= gameTime.ElapsedGameTime.Milliseconds;

            }
            else if (rowsToPush > 0) UpdatePush(gameTime, 1.0f);

            List<Combo> combos = new List<Combo>();
            foreach (MatchTile candidate in comboCandidates)
            {
                tilesActive = true;

                Combo combo = CheckForCombo(candidate);
                if (combo != null)
                {
                    Combo redundantCombo = combos.Find(x => x.tiles.Any(y => combo.tiles.Contains(y)));
                    if (redundantCombo == null) ;
                    else if (combo.tiles.Count <= redundantCombo.tiles.Count) continue;
                    else
                    {
                        combos.Remove(redundantCombo);
                    }

                    Combo adjacentCombo = combos.Find(x => x.tiles.Any(y => combo.tiles.Any(z => Math.Abs(y.BoardX - z.BoardX) + Math.Abs(y.BoardY - z.BoardY) == 1)));
                    if (adjacentCombo != null) adjacentCombo.tiles.AddRange(combo.tiles);
                    else combos.Add(combo);
                }
            }

            bool blockBroken = false;

            foreach (Combo combo in combos)
            {
                Combo maxChain = null;
                foreach (MatchTile matchTile in combo.tiles)
                {
                    if (matchTile.Chain != null && (maxChain == null || maxChain.chain < matchTile.Chain.chain)) maxChain = matchTile.Chain;
                }

                if (maxChain == null) maxChain = combo;
                else maxChain.chain++;

                foreach (MatchTile matchTile in combo.tiles)
                {
                    matchTile.Chain = maxChain;
                    if (matchTile.Eliminate()) blockBroken = true;
                }

                if (!matchScene.GameViewModel.PlayerTurn.Value) continue;

                Vector2 offset = Vector2.Zero;
                if (combo.tiles.Count > 3)
                {
                    Vector2 particlePosition = combo.tiles.OrderBy(x => x.BoardY).ThenBy(x => x.BoardX).First().Position - new Vector2(0, MatchTile.TILE_SIZE * 6);
                    particlePosition *= CrossPlatformGame.Scale;
                    particlePosition += new Vector2(BoardBounds.Left, BoardBounds.Top);
                    ComboParticle comboParticle = new ComboParticle(matchScene, particlePosition - new Vector2(4, MatchTile.TILE_SIZE / 2), combo.tiles.Count.ToString(), GameSprite.Particles_combo_indicator);
                    matchScene.AddOverlay(comboParticle);
                    offset.Y += 32;
                }
                if (maxChain.chain > 1)
                {
                    Vector2 particlePosition = combo.tiles.OrderBy(x => x.BoardY).ThenBy(x => x.BoardX).First().Position - new Vector2(0, MatchTile.TILE_SIZE * 6);
                    particlePosition *= CrossPlatformGame.Scale;
                    particlePosition += new Vector2(BoardBounds.Left, BoardBounds.Top);
                    ComboParticle comboParticle = new ComboParticle(matchScene, particlePosition - new Vector2(4, MatchTile.TILE_SIZE / 2) + offset, maxChain.chain + "x", GameSprite.Particles_chain_indicator);
                    matchScene.AddOverlay(comboParticle);
                }

                int damage = combo.tiles.Count * maxChain.chain;

                if (matchScene.Enemy.Armor < damage)
                {
                    if (TotalDamage == 0)
                    {
                        int enemyHeight = matchScene.GameViewModel.mapScene.MapViewModel.MapActor.Value.SpriteBounds().Height * CrossPlatformGame.Scale;
                        Vector2 particlePosition = new Vector2(matchScene.GameViewModel.mapScene.MapPanel.Position.X, matchScene.GameViewModel.mapScene.MapPanel.Position.Y) + new Vector2(matchScene.GameViewModel.mapScene.MapPanel.InnerBounds.Center.X, matchScene.GameViewModel.mapScene.MapPanel.InnerBounds.Bottom - enemyHeight / 2);
                        attackPentagram = new AttackPentagram(parentScene, particlePosition, combo.tiles[0].TileColor);
                        matchScene.AddParticle(attackPentagram);
                    }

                    TotalDamage += damage;
                    attackPentagram?.SpeedUp(damage, TotalDamage);
                }
            }

            if (combos.Count > 0)
            {
                if (blockBroken) Audio.PlaySound(GameSound.block_junk_break);
                else Audio.PlaySound(GameSound.block_match);
                turnGracePeriod = 3000;
            }

            if (tilesActive) turnGracePeriod = Math.Max(turnGracePeriod, 50);

            if (turnTimer < 15000 && turnTimer > 0) turnTimer -= gameTime.ElapsedGameTime.Milliseconds;
            if (turnGracePeriod <= 0 && turnTimer <= 0 && rowsToPush == 0 && !matchScene.GameViewModel.PlayerCommand.Value && !matchScene.GameViewModel.EnemyTurn.Value)
            {
                if (matchScene.GameViewModel.PlayerTurn.Value)
                {
                    matchScene.GameViewModel.PlayerTurn.Value = false;

                    if (TotalDamage > 0)
                    {
                        attackPentagram.Attack(new Action(DealDamage));
                    }
                    else
                    {
                        matchScene.GameViewModel.EnemyTurn.Value = true;

                        if (matchScene.Enemy.CurrentHealth > 0) Task.Delay(1000).ContinueWith(t => matchScene.GameViewModel.BeginEnemyTurn());
                        else Task.Delay(1000).ContinueWith(t => matchScene.GameViewModel.Victory());
                    }
                }
            }
        }

        private void DealDamage()
        {
            switch (attackPentagram.tileColor)
            {
                case TileColor.Cyan: Audio.PlaySound(GameSound.damange_cyan); break;
                case TileColor.Red: Audio.PlaySound(GameSound.damange_red); break;
                case TileColor.Blue: Audio.PlaySound(GameSound.damange_blue); break;
                case TileColor.Yellow: Audio.PlaySound(GameSound.damange_yellow); break;
                case TileColor.Green: Audio.PlaySound(GameSound.damange_green); break;
            }

            int actualDamage = TotalDamage * 10 + Rng.RandomInt(0, 9);
            if (GameProfile.GetSaveData<bool>("Powerup")) actualDamage = (int)(actualDamage * 1.25f);

            if (matchScene.Enemy.Weakness.Any(x => x == attackPentagram.tileColor))
            {
                actualDamage = (int)(actualDamage * 1.5f);

                Vector2 particlePosition = new Vector2(matchScene.GameViewModel.mapScene.MapPanel.Position.X, matchScene.GameViewModel.mapScene.MapPanel.Position.Y) + new Vector2(matchScene.GameViewModel.mapScene.MapPanel.InnerBounds.Center.X + 20, matchScene.GameViewModel.mapScene.MapPanel.InnerBounds.Bottom - 80);
                ComboParticle comboParticle = new ComboParticle(matchScene, particlePosition - new Vector2(4, MatchTile.TILE_SIZE / 2), "", GameSprite.Particles_ui_weak_indicator);
                matchScene.AddOverlay(comboParticle);
            }
            else if (matchScene.Enemy.Resist.Any(x => x == attackPentagram.tileColor))
            {
                actualDamage /= 2;

                Vector2 particlePosition = new Vector2(matchScene.GameViewModel.mapScene.MapPanel.Position.X, matchScene.GameViewModel.mapScene.MapPanel.Position.Y) + new Vector2(matchScene.GameViewModel.mapScene.MapPanel.InnerBounds.Center.X + 20, matchScene.GameViewModel.mapScene.MapPanel.InnerBounds.Bottom - 80);
                ComboParticle comboParticle = new ComboParticle(matchScene, particlePosition - new Vector2(4, MatchTile.TILE_SIZE / 2), "", GameSprite.Particles_ui_resist_indicator);
                matchScene.AddOverlay(comboParticle);
            }
            else if (matchScene.Enemy.Immune.Any(x => x == attackPentagram.tileColor))
            {
                actualDamage = 0;

                Vector2 particlePosition = new Vector2(matchScene.GameViewModel.mapScene.MapPanel.Position.X, matchScene.GameViewModel.mapScene.MapPanel.Position.Y) + new Vector2(matchScene.GameViewModel.mapScene.MapPanel.InnerBounds.Center.X + 20, matchScene.GameViewModel.mapScene.MapPanel.InnerBounds.Bottom - 80);
                ComboParticle comboParticle = new ComboParticle(matchScene, particlePosition - new Vector2(4, MatchTile.TILE_SIZE / 2), "", GameSprite.Particles_ui_immune_indicator);
                matchScene.AddOverlay(comboParticle);
            }


            matchScene.Enemy.CurrentHealth = Math.Max(0, matchScene.Enemy.CurrentHealth - actualDamage);
            matchScene.GameViewModel.mapScene.MapViewModel.CurrentHealthBar.Value = matchScene.Enemy.CurrentHealth;

            string damageString = actualDamage.ToString();
            Vector2 damagePosition = new Vector2(matchScene.GameViewModel.mapScene.MapPanel.Position.X, matchScene.GameViewModel.mapScene.MapPanel.Position.Y) + new Vector2(matchScene.GameViewModel.mapScene.MapPanel.InnerBounds.Center.X + 16 - damageString.Length * 28, matchScene.GameViewModel.mapScene.MapPanel.InnerBounds.Bottom - 50);
            SceneObjects.Particles.DamageParticle damageParticle = new SceneObjects.Particles.DamageParticle(parentScene, damagePosition, damageString);
            damageParticle.OnTerminated += matchScene.GameViewModel.FinishTurn;
            parentScene.AddParticle(damageParticle);

            attackPentagram = null;

            TransitionController hurtController = new TransitionController(TransitionDirection.In, 1000, PriorityLevel.GameLevel);
            hurtController.UpdateTransition += new Action<float>(t => matchScene.GameViewModel.mapScene.ActorSprite.SpriteColor = Color.Lerp(Color.Red, Color.White, t));
            parentScene.AddController(hurtController);

            /*
            matchScene.GameViewModel.EnemyTurn.Value = true;
            if (matchScene.Enemy.CurrentHealth > 0) Task.Delay(1000).ContinueWith(t => matchScene.GameViewModel.BeginEnemyTurn());
            else Task.Delay(1000).ContinueWith(t => matchScene.GameViewModel.Victory());
            */
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (turnTimer > 0)
            {
                Vector2 cursorPosition = new Vector2((int)position.X + (SelectionX + 1) * MatchTile.TILE_SIZE, (int)position.Y + (SelectionY + 1) * MatchTile.TILE_SIZE - PushOffset + 2);
                cursorSprite?.Draw(spriteBatch, cursorPosition, camera, 0.32f);
            }

            for (int x = 0; x < TILE_COLUMNS; x++)
            {
                for (int y = 0; y < TILE_ROWS; y++)
                {
                    matchTiles[x, y]?.Draw(spriteBatch, camera);
                }
            }

            for (int x = 0; x < TILE_COLUMNS; x++)
            {
                upcomingTiles[x].Draw(spriteBatch, camera, new Color(96, 96, 96, 255));
            }
        }

        private void UpdateSwap(GameTime gameTime)
        {
            if (Swapping)
            {
                swapTimeLeft -= gameTime.ElapsedGameTime.Milliseconds;
                if (Swapping)
                {
                    Vector2 leftPosition = new Vector2(SelectionX * MatchTile.TILE_SIZE, SelectionY * MatchTile.TILE_SIZE);
                    Vector2 rightPosition = new Vector2((SelectionX + 1) * MatchTile.TILE_SIZE, SelectionY * MatchTile.TILE_SIZE);
                    float progress = (float)swapTimeLeft / SWAP_LENGTH;

                    MatchTile leftSelection = MatchTiles[SelectionX, SelectionY];
                    MatchTile rightSelection = MatchTiles[SelectionX + 1, SelectionY];

                    leftSelection?.SetSlidingPosition(Vector2.Lerp(leftPosition, rightPosition, 1.0f - progress) + position);
                    rightSelection?.SetSlidingPosition(Vector2.Lerp(leftPosition, rightPosition, progress) + position);

                    if (leftSelection != null) leftSelection.Chain = null;
                    if (rightSelection != null) rightSelection.Chain = null;
                }
                else
                {
                    MatchTile temp = MatchTiles[SelectionX, SelectionY];
                    MatchTiles[SelectionX, SelectionY] = MatchTiles[SelectionX + 1, SelectionY];
                    MatchTiles[SelectionX + 1, SelectionY] = temp;

                    MatchTile leftSelection = MatchTiles[SelectionX, SelectionY];
                    MatchTile rightSelection = MatchTiles[SelectionX + 1, SelectionY];

                    if (leftSelection != null)
                    {
                        leftSelection.SetRestingPosition(SelectionX, SelectionY, position);
                        if (!leftSelection.ReadyToFall) comboCandidates.Add(leftSelection);
                    }

                    if (rightSelection != null)
                    {
                        rightSelection.SetRestingPosition(SelectionX + 1, SelectionY, position);
                        if (!rightSelection.ReadyToFall) comboCandidates.Add(rightSelection);
                    }
                }
            }
        }

        public void Swap()
        {
            MatchTile leftSelection = MatchTiles[SelectionX, SelectionY];
            MatchTile rightSelection = MatchTiles[SelectionX + 1, SelectionY];
            if (leftSelection == null && rightSelection == null) return;

            Audio.PlaySound(GameSound.block_swap);
            swapTimeLeft = SWAP_LENGTH;

            if (!matchScene.GameViewModel.PlayerTurnCountdown) matchScene.GameViewModel.SwapAction();

            turnGracePeriod = 3000;

            if (turnTimer >= 15000) turnTimer = 15000 - 1;
        }

        private void UpdatePush(GameTime gameTime, float scale)
        {
            scale *= 12;

            pushCooldown -= (int)(gameTime.ElapsedGameTime.Milliseconds * scale);
            while (pushCooldown <= 0)
            {
                /*if (DefeatImminent())
                {

                    ((MatchScene)parentScene).EndMatch(this);
                }*/

                pushCooldown += PUSH_TICK;
                PushOffset++;
                if (PushOffset >= MatchTile.TILE_SIZE)
                {
                    PushRow();
                    rowsToPush--;
                }
            }
        }

        private void PushRow()
        {
            PushOffset = 0;

            for (int x = 0; x < TILE_COLUMNS; x++)
            {
                for (int y = 1; y < TILE_ROWS; y++)
                {
                    matchTiles[x, y - 1] = matchTiles[x, y];
                    matchTiles[x, y] = null;
                    matchTiles[x, y - 1]?.SetRestingPosition(x, y - 1, position);
                }

                matchTiles[x, TILE_ROWS - 1] = upcomingTiles[x];
                matchTiles[x, TILE_ROWS - 1].SetRestingPosition(x, TILE_ROWS - 1, position);
            }

            List<TileColor> colors = Enum.GetValues<TileColor>().ToList();
            for (int x = 0; x < TILE_COLUMNS; x++)
            {
                List<TileColor> availableColors = colors.FindAll(x => x != TileColor.Junk);
                if (x >= 2 && upcomingTiles[x - 2].TileColor == upcomingTiles[x - 1].TileColor)
                    availableColors = availableColors.FindAll(c => c != upcomingTiles[x - 1].TileColor);
                if (matchTiles[x, MatchBoard.TILE_ROWS - 1] != null && matchTiles[x, MatchBoard.TILE_ROWS - 2] != null && matchTiles[x, MatchBoard.TILE_ROWS - 1].TileColor == matchTiles[x, MatchBoard.TILE_ROWS - 2].TileColor)
                    availableColors = availableColors.FindAll(c => c != matchTiles[x, MatchBoard.TILE_ROWS - 1].TileColor);
                MatchTile matchTile = new MatchTile(matchScene, this, availableColors[Rng.RandomInt(0, availableColors.Count - 1)]);
                matchTile.SetRestingPosition(x, TILE_ROWS, position);
                matchTile.AppearingFinished();
                upcomingTiles[x] = matchTile;
            }

            for (int x = 0; x < TILE_COLUMNS; x++)
            {
                comboCandidates.Add(matchTiles[x, TILE_ROWS - 1]);
            }

            if (SelectionY > 1) SelectionY--;
        }

        public void DropTile(int x, int y)
        {
            matchTiles[x, y + 1] = matchTiles[x, y];
            matchTiles[x, y] = null;
            matchTiles[x, y + 1]?.SetRestingPosition(x, y + 1, position);
        }

        public Combo CheckForCombo(MatchTile matchTile)
        {
            if (matchTile == null || matchTile.Eliminating || matchTile.Falling || matchTile.Sliding || matchTile.TileColor == TileColor.Junk) return null;

            List<MatchTile> horizontalTiles = new List<MatchTile>();
            CheckComboLeft(matchTile.BoardX - 1, matchTile.BoardY, matchTile.TileColor, horizontalTiles);
            CheckComboRight(matchTile.BoardX + 1, matchTile.BoardY, matchTile.TileColor, horizontalTiles);

            List<MatchTile> verticalTiles = new List<MatchTile>();
            CheckComboUp(matchTile.BoardX, matchTile.BoardY - 1, matchTile.TileColor, verticalTiles);
            CheckComboDown(matchTile.BoardX, matchTile.BoardY + 1, matchTile.TileColor, verticalTiles);

            List<MatchTile> comboTiles = new List<MatchTile>();
            comboTiles.Add(matchTile);
            if (horizontalTiles.Count >= 2) comboTiles.AddRange(horizontalTiles);
            if (verticalTiles.Count >= 2) comboTiles.AddRange(verticalTiles);
            if (comboTiles.Count >= 3) return new Combo() { tiles = comboTiles };

            return null;
        }

        public void CheckComboLeft(int x, int y, TileColor color, List<MatchTile> matchingTiles)
        {
            if (x < 0) return;
            MatchTile matchTile = matchTiles[x, y];
            if (matchTile == null || matchTile.Eliminating || matchTile.Falling || matchTile.Sliding || matchTile.TileColor != color) return;
            matchingTiles.Add(matchTile);
            CheckComboLeft(x - 1, y, color, matchingTiles);
        }

        public void CheckComboRight(int x, int y, TileColor color, List<MatchTile> matchingTiles)
        {
            if (x > TILE_COLUMNS - 1) return;
            MatchTile matchTile = matchTiles[x, y];
            if (matchTile == null || matchTile.Eliminating || matchTile.Falling || matchTile.Sliding || matchTile.TileColor != color) return;
            matchingTiles.Add(matchTile);
            CheckComboRight(x + 1, y, color, matchingTiles);
        }

        public void CheckComboUp(int x, int y, TileColor color, List<MatchTile> matchingTiles)
        {
            if (y < 0) return;
            MatchTile matchTile = matchTiles[x, y];
            if (matchTile == null || matchTile.Eliminating || matchTile.Falling || matchTile.Sliding || matchTile.TileColor != color) return;
            matchingTiles.Add(matchTile);
            CheckComboUp(x, y - 1, color, matchingTiles);
        }

        public void CheckComboDown(int x, int y, TileColor color, List<MatchTile> matchingTiles)
        {
            if (y > TILE_ROWS - 1) return;
            MatchTile matchTile = matchTiles[x, y];
            if (matchTile == null || matchTile.Eliminating || matchTile.Falling || matchTile.Sliding || matchTile.TileColor != color) return;
            matchingTiles.Add(matchTile);
            CheckComboDown(x, y + 1, color, matchingTiles);
        }

        public bool DefeatImminent()
        {
            int ceilingRow = 5;

            if (PushOffset > 10) ceilingRow++;

            for (int i = 0; i < TILE_COLUMNS; i++)
            {
                if (matchTiles[i, ceilingRow] != null)
                {
                    return true;
                }
            }

            return false;
        }

        public void ChargeUp()
        {
            List<TileColor> colors = Enum.GetValues<TileColor>().ToList();
            for (int x = 0; x < TILE_COLUMNS; x++)
            {
                List<TileColor> availableColors = colors.FindAll(x => x != TileColor.Junk);
                if (x >= 2 && upcomingTiles[x - 2].TileColor == upcomingTiles[x - 1].TileColor)
                    availableColors = availableColors.FindAll(c => c != upcomingTiles[x - 1].TileColor);
                if (matchTiles[x, MatchBoard.TILE_ROWS - 1] != null && matchTiles[x, MatchBoard.TILE_ROWS - 2] != null && matchTiles[x, MatchBoard.TILE_ROWS - 1].TileColor == matchTiles[x, MatchBoard.TILE_ROWS - 2].TileColor)
                    availableColors = availableColors.FindAll(c => c != matchTiles[x, MatchBoard.TILE_ROWS - 1].TileColor);
                MatchTile matchTile = new MatchTile(matchScene, this, availableColors[Rng.RandomInt(0, availableColors.Count - 1)]);
                matchTile.SetRestingPosition(x, TILE_ROWS, position);
                matchTile.AppearingFinished();
                upcomingTiles[x] = matchTile;
            }

            rowsToPush = 3;
        }

        public int FindHighestColumn(int magnitude)
        {
            int bestColumnHeight = 0;
            int column = 0;
            for (int i = 0; i < TILE_COLUMNS; i++)
            {
                int columnHeight = TILE_ROWS;
                for (int y = TILE_ROWS - 1; y >= 0; y--) if (matchTiles[i, y] == null) { columnHeight = TILE_ROWS - y; break; }

                if (columnHeight > bestColumnHeight && columnHeight + magnitude < TILE_ROWS) { bestColumnHeight = columnHeight; column = i; }
            }

            return column;
        }

        public int HighestColumnSize()
        {
            int bestColumnHeight = 0;
            for (int i = 0; i < TILE_COLUMNS; i++)
            {
                int columnHeight = TILE_ROWS;
                for (int y = TILE_ROWS - 1; y >= 0; y--) if (matchTiles[i, y] == null) { columnHeight = TILE_ROWS - y; break; }

                if (columnHeight > bestColumnHeight) { bestColumnHeight = columnHeight; }
            }

            return bestColumnHeight;
        }

        public void Spike(TileColor tileColor, int magnitude)
        {
            int column = FindHighestColumn(magnitude);

            MatchTile matchTile = new MatchTile(matchScene, this, tileColor, 1);
            matchTile.SetRestingPosition(column, 0, position);
            matchTiles[column, 0] = matchTile;
            matchTile.AppearingTimeLeft = Rng.RandomInt(1, 300);

            for (int i = 1; i < magnitude; i++)
            {
                matchTile = new MatchTile(matchScene, this, TileColor.Junk, 1);
                matchTile.SetRestingPosition(column, i, position);
                matchTiles[column, i] = matchTile;
                matchTile.AppearingTimeLeft = Rng.RandomInt(1, 300);
            }
        }

        public void Neutralize(TileColor tileColor)
        {
            for (int y = 0; y < TILE_ROWS; y++)
            {
                for (int x = 0; x < TILE_COLUMNS; x++)
                {
                    MatchTile matchTile = matchTiles[x, y];
                    if (matchTile != null && matchTile.TileColor == tileColor)
                    {
                        matchTile = new MatchTile(matchScene, this, TileColor.Junk, 1);
                        matchTile.SetRestingPosition(x, y, position);
                        matchTiles[x, y] = matchTile;
                        matchTile.AppearingTimeLeft = 1;
                    }
                }
            }
        }

        public void Break(TileColor tileColor)
        {
            for (int y = 0; y < TILE_ROWS; y++)
            {
                for (int x = 0; x < TILE_COLUMNS; x++)
                {
                    MatchTile matchTile = matchTiles[x, y];
                    if (matchTile != null && matchTile.TileColor == tileColor)
                    {
                        matchTile.Break(new Combo());
                    }
                }
            }
        }

        public void DumpTiles(TileColor tileColor, int magnitude)
        {
            for (int i = 0; i < magnitude; i++)
            {
                int column = Rng.RandomInt(0, TILE_COLUMNS - 1);

                if (matchTiles[column, 0] == null)
                {
                    MatchTile matchTile = new MatchTile(matchScene, this, tileColor, 1);
                    matchTile.SetRestingPosition(column, 0, position);
                    matchTiles[column, 0] = matchTile;
                    matchTile.AppearingTimeLeft = Rng.RandomInt(1, 300);
                }
            }
        }

        public void SpawnJunk(int v)
        {
            int blocksLeft = v;
            List<TileColor> colors = Enum.GetValues<TileColor>().ToList();

            for (int y = TILE_ROWS - 1; y >= 1; y--)
            {
                for (int x = 0; x < TILE_COLUMNS; x++)
                {
                    if (matchTiles[x, y] == null)
                    {
                        MatchTile matchTile = new MatchTile(matchScene, this, TileColor.Junk);
                        matchTile.SetRestingPosition(x, y, position);
                        matchTiles[x, y] = matchTile;
                        matchTile.AppearingTimeLeft = Rng.RandomInt(1, 300);

                        blocksLeft--;
                        if (blocksLeft <= 0) return;
                    }
                }
            }
        }

        public int PushOffset { get; private set; }

        public MatchTile[,] MatchTiles { get => matchTiles; }
        public List<MatchTile> ComboCandidates { get => comboCandidates; }

        public int SelectionX { get; set; } = MatchBoard.TILE_COLUMNS / 2;
        public int SelectionY { get; set; } = MatchBoard.TILE_ROWS / 2;
        public bool Swapping { get => swapTimeLeft > 0.0f; }

        public Rectangle BoardBounds
        {
            get => new Rectangle((int)gamePanel.Position.X + gamePanel.InnerBounds.X, (int)gamePanel.Position.Y + gamePanel.InnerBounds.Y, gamePanel.InnerBounds.Width, gamePanel.InnerBounds.Height);
        }
        public bool Speedup { get; internal set; }
    }
}
