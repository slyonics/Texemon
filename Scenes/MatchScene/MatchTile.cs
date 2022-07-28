using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texemon.Scenes.MatchScene
{
    public enum TileColor
    {
        Blue,
        Cyan,
        Green,
        Red,
        Yellow,
        Junk
    }

    public class MatchTile
    {
        private static readonly Dictionary<string, Animation> BLOCK_ANIMATIONS = new Dictionary<string, Animation>()
        {
            { "Appearing", new Animation(0, 2, TILE_SIZE, TILE_SIZE, 6, 70) },
            { "Disappearing", new Animation(0, 3, TILE_SIZE, TILE_SIZE, 5, 120) },
            { "Idle", new Animation(0, 0, TILE_SIZE, TILE_SIZE, 1, 10000) },
            { "Glint", new Animation(0, 1, TILE_SIZE, TILE_SIZE, 5, new int[] { 100, 100, 100, 100, 2500 }) }
        };

        private static readonly Dictionary<TileColor, GameSprite> spritelookup = new Dictionary<TileColor, GameSprite>()
        {
            { TileColor.Blue, GameSprite.BlueBlock },
            { TileColor.Cyan, GameSprite.CyanBlock },
            { TileColor.Green, GameSprite.GreenBlock },
            { TileColor.Red, GameSprite.RedBlock },
            { TileColor.Yellow, GameSprite.YellowBlock },
            { TileColor.Junk, GameSprite.JunkBlock }
        };

        public const int TILE_SIZE = 40;

        private const float TILE_DEPTH = 0.8f;
        private const int FALL_TICK = 5;
        private const int ELIMINATION_LENGTH = 2000;

        private AnimatedSprite blockSprite;
        private Vector2 position;
        private Rectangle spriteBounds;
        private TileColor color;

        private int boardX;
        private int boardY;
        private MatchBoard matchBoard;

        private int fallOffset;
        private int fallCooldown = FALL_TICK;
        private int fallSpeed = FALL_TICK;

        private int eliminationTimeLeft;

        public int AppearingTimeLeft { get; set; } = 1;

        private int glintTime = Rng.RandomInt(0, 3000);

        public MatchTile(MatchScene iMatchScene, MatchBoard iMatchBoard, TileColor iColor, int iSpeed = FALL_TICK)
        {
            matchBoard = iMatchBoard;
            TileColor = iColor;

            fallSpeed = iSpeed;

        }

        public void AppearingFinished()
        {
            if (blockSprite == null)
            {
                AppearingTimeLeft = 0;
                blockSprite = new AnimatedSprite(AssetCache.SPRITES[spritelookup[TileColor]], BLOCK_ANIMATIONS);
            }

            blockSprite.PlayAnimation("Idle");
            blockSprite.Update(new GameTime(new TimeSpan(0), new TimeSpan(0, 0, 0, 0, Rng.RandomInt(0, 2500))));
        }

        private void GlintFinished()
        {
            glintTime = Rng.RandomInt(1000, 4000);
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            Draw(spriteBatch, camera, Color.White);
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera, Color color)
        {
            Vector2 runePosition = new Vector2((int)position.X, (int)position.Y - matchBoard.PushOffset - fallOffset);
            Vector2 blockPosition = runePosition + new Vector2(TILE_SIZE / 2, TILE_SIZE);

            if (Eliminating && eliminationTimeLeft > 600 && !Breaking)
            {
                if (eliminationTimeLeft / 100 % 2 == 0)
                {
                    if (blockSprite != null) blockSprite.SpriteColor = color;
                    blockSprite?.Draw(spriteBatch, blockPosition, null, TILE_DEPTH - 0.01f);
                }
            }
            else
            {
                if (blockSprite != null) blockSprite.SpriteColor = color;
                blockSprite?.Draw(spriteBatch, blockPosition, null, TILE_DEPTH - 0.01f);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (AppearingTimeLeft > 0)
            {
                AppearingTimeLeft -= gameTime.ElapsedGameTime.Milliseconds;
                if (AppearingTimeLeft <= 0)
                {
                    blockSprite = new AnimatedSprite(AssetCache.SPRITES[spritelookup[TileColor]], BLOCK_ANIMATIONS);
                    blockSprite.PlayAnimation("Appearing", AppearingFinished);
                }
            }
            else
            {
                if (!Eliminating && !Breaking && glintTime > 0)
                {
                    glintTime -= gameTime.ElapsedGameTime.Milliseconds;
                    if (glintTime <= 0)
                    {
                        blockSprite.PlayAnimation("Glint", GlintFinished);
                    }
                }

                blockSprite?.Update(gameTime);
            }
        }

        public void UpdateMovement(GameTime gameTime)
        {
            if (fallOffset > 0)
            {
                fallCooldown -= gameTime.ElapsedGameTime.Milliseconds;
                while (fallCooldown < 0)
                {
                    fallCooldown += fallSpeed;
                    fallOffset--;

                    if (fallOffset == 0)
                    {
                        if (ReadyToFall)
                        {
                            matchBoard.DropTile(boardX, boardY);
                            fallOffset = TILE_SIZE - 1;
                        }
                        else
                        {
                            StopFalling();
                        }
                    }
                }
            }
            else if (ReadyToFall)
            {
                if (boardY < MatchBoard.TILE_ROWS - 2)
                {
                    MatchTile tileBelow = matchBoard.MatchTiles[boardX, boardY + 2];
                    if (tileBelow != null) Chain = (Chain == null || (tileBelow.Chain != null && tileBelow.Chain.chain > Chain.chain)) ? tileBelow.Chain : Chain;
                }
                StartFalling();
            }
            else if (!Breaking) Chain = null;
        }

        public bool UpdateElimination(GameTime gameTime)
        {
            if (eliminationTimeLeft > 0)
            {
                int oldEliminationTime = eliminationTimeLeft;

                eliminationTimeLeft -= gameTime.ElapsedGameTime.Milliseconds;

                if (oldEliminationTime > 600 && eliminationTimeLeft <= 600) blockSprite.PlayAnimation("Disappearing");

                if (eliminationTimeLeft <= 0)
                {
                    return true;
                }
            }

            return false;
        }

        public void SetRestingPosition(int x, int y, Vector2 offset)
        {
            boardX = x;
            boardY = y;
            position = new Vector2(x * TILE_SIZE, y * TILE_SIZE) + offset;

            Sliding = false;
        }

        public void SetSlidingPosition(Vector2 newPosition)
        {
            position = newPosition;

            Sliding = true;
        }

        public void StartFalling()
        {
            matchBoard.DropTile(boardX, boardY);
            fallOffset = TILE_SIZE - 1;
            fallCooldown = fallSpeed;
        }

        public void StopFalling()
        {
            fallCooldown = fallSpeed = FALL_TICK;

            matchBoard.ComboCandidates.Add(this);
        }

        public bool Eliminate()
        {
            eliminationTimeLeft = ELIMINATION_LENGTH;

            bool playBreakSound = false;

            if (BoardX > 0)
            {
                MatchTile matchTile = matchBoard.MatchTiles[BoardX - 1, boardY];
                if (matchTile != null && matchTile.TileColor == TileColor.Junk && !matchTile.Breaking) if (matchTile.Break(Chain)) playBreakSound = true;
            }

            if (BoardX < MatchBoard.TILE_COLUMNS - 1)
            {
                MatchTile matchTile = matchBoard.MatchTiles[BoardX + 1, boardY];
                if (matchTile != null && matchTile.TileColor == TileColor.Junk && !matchTile.Breaking) if (matchTile.Break(Chain)) playBreakSound = true;
            }

            if (BoardY > 0)
            {
                MatchTile matchTile = matchBoard.MatchTiles[BoardX, boardY - 1];
                if (matchTile != null && matchTile.TileColor == TileColor.Junk && !matchTile.Breaking) if (matchTile.Break(Chain)) playBreakSound = true;
            }

            if (BoardY < MatchBoard.TILE_ROWS - 1)
            {
                MatchTile matchTile = matchBoard.MatchTiles[BoardX, boardY + 1];
                if (matchTile != null && matchTile.TileColor == TileColor.Junk && !matchTile.Breaking) if (matchTile.Break(Chain)) playBreakSound = true;
            }

            return playBreakSound;
        }

        public bool Break(MatchBoard.Combo chain)
        {
            if (blockSprite == null) return false;

            Breaking = true;
            blockSprite.PlayAnimation("Disappearing", FinishedBreaking);
            Chain = chain;

            if (BoardX > 0)
            {
                MatchTile matchTile = matchBoard.MatchTiles[BoardX - 1, boardY];
                if (matchTile != null && matchTile.TileColor == TileColor.Junk && !matchTile.Breaking) matchTile.Break(Chain);
            }

            if (BoardX < MatchBoard.TILE_COLUMNS - 1)
            {
                MatchTile matchTile = matchBoard.MatchTiles[BoardX + 1, boardY];
                if (matchTile != null && matchTile.TileColor == TileColor.Junk && !matchTile.Breaking) matchTile.Break(Chain);
            }

            if (BoardY > 0)
            {
                MatchTile matchTile = matchBoard.MatchTiles[BoardX, boardY - 1];
                if (matchTile != null && matchTile.TileColor == TileColor.Junk && !matchTile.Breaking) matchTile.Break(Chain);
            }

            if (BoardY < MatchBoard.TILE_ROWS - 1)
            {
                MatchTile matchTile = matchBoard.MatchTiles[BoardX, boardY + 1];
                if (matchTile != null && matchTile.TileColor == TileColor.Junk && !matchTile.Breaking) matchTile.Break(Chain);
            }

            return true;
        }

        private void FinishedBreaking()
        {
            eliminationTimeLeft = 1;
        }

        public TileColor TileColor
        {
            get => color;
            set
            {
                color = value;
                spriteBounds = new Rectangle((int)color * TILE_SIZE, 0, TILE_SIZE, TILE_SIZE);
            }
        }

        public int BoardX { get => boardX; }
        public int BoardY { get => boardY; }
        public Vector2 Position { get => position; }
        public bool ReadyToFall
        {
            get
            {
                return (fallOffset == 0 && boardY < MatchBoard.TILE_ROWS - 1 && matchBoard.MatchTiles[boardX, boardY + 1] == null &&
                        (!matchBoard.Swapping || (matchBoard.SelectionX != boardX && matchBoard.SelectionX + 1 != boardX) || matchBoard.SelectionY != boardY + 1));
            }
        }

        public bool Falling { get => fallOffset != 0; }
        public bool Sliding { get; private set; }
        public bool Eliminating { get => eliminationTimeLeft > 0; }
        public bool Appearing { get => AppearingTimeLeft > 0; }
        public bool CanSwap { get => !Eliminating && !Falling && !Sliding && !Breaking && !Appearing; }
        public MatchBoard.Combo Chain { get; set; } = null;
        public bool Breaking { get; private set; }
    }
}
