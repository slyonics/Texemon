using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texemon.Scenes.MatchScene
{
    public class PlayerController : Controller
    {
        private MatchBoard matchBoard;
        private MatchScene matchScene;

        private int cursorSoundCooldown = 0;
        public PlayerController(MatchScene iMatchScene, MatchBoard iMatchBoard)
            : base(PriorityLevel.GameLevel)
        {
            matchScene = iMatchScene;
            matchBoard = iMatchBoard;
        }

        public override void PreUpdate(GameTime gameTime)
        {
            if (matchBoard.turnTimer <= 0) return;

            if (SelectionY < 6) SelectionY = 6;

            InputFrame inputFrame = Input.CurrentInput;

            if (cursorSoundCooldown > 0) cursorSoundCooldown -= gameTime.ElapsedGameTime.Milliseconds;

            if (!matchBoard.Swapping)
            {
                bool newSelection = false;

                if ((Math.Abs(Input.DeltaMouseGame.X) > 0 || Math.Abs(Input.DeltaMouseGame.Y) > 0) && matchBoard.BoardBounds.Contains(Input.MousePosition))
                {
                    int scale = CrossPlatformGame.Scale;
                    int oldX = SelectionX;
                    int oldY = SelectionY;

                    SelectionX = (int)(((Input.MousePosition.X / scale - (matchBoard.BoardBounds.Left) / scale - MatchTile.TILE_SIZE) + MatchTile.TILE_SIZE / 2) / MatchTile.TILE_SIZE);

                    Console.WriteLine(matchBoard.BoardBounds.Left);

                    SelectionY = (int)((Input.MousePosition.Y / scale - matchBoard.BoardBounds.Top + matchBoard.PushOffset - 10) / MatchTile.TILE_SIZE + 6);
                    if (SelectionX < 0) SelectionX = 0;
                    if (SelectionX >= MatchBoard.TILE_COLUMNS - 1) SelectionX = MatchBoard.TILE_COLUMNS - 2;
                    if (SelectionY < 6)
                    {
                        SelectionX = oldX;
                        SelectionY = 6;
                    }
                    if (SelectionY >= MatchBoard.TILE_ROWS)
                    {
                        SelectionX = oldX;
                        SelectionY = MatchBoard.TILE_ROWS - 1;
                    }

                    if (oldX != SelectionX || oldY != SelectionY)
                    {
                        if (cursorSoundCooldown <= 0) { Audio.PlaySound(GameSound.move_selection_cursor); cursorSoundCooldown = 50; }
                        newSelection = true;
                    }

                    if (Input.LeftMouseClicked && matchBoard.BoardBounds.Contains(Input.MousePosition))
                    {
                        if ((LeftSelection == null || LeftSelection.CanSwap) && (RightSelection == null || RightSelection.CanSwap))
                            matchBoard.Swap();
                    }
                }
                else if (Input.LeftMouseClicked && matchBoard.BoardBounds.Contains(Input.MousePosition))
                {
                    if ((LeftSelection == null || LeftSelection.CanSwap) && (RightSelection == null || RightSelection.CanSwap))
                        matchBoard.Swap();
                }
                else if (!newSelection && inputFrame.CommandPressed(Command.Confirm))
                {
                    if ((LeftSelection == null || LeftSelection.CanSwap) && (RightSelection == null || RightSelection.CanSwap))
                        matchBoard.Swap();
                }
                else if (!newSelection && inputFrame.CommandPressed(Command.Left)) { CursorLeft(); repeatCooldown = 250; }
                else if (!newSelection && inputFrame.CommandPressed(Command.Right)) { CursorRight(); repeatCooldown = 250; }
                else if (!newSelection && inputFrame.CommandPressed(Command.Down)) { CursorDown(); repeatCooldown = 250; }
                else if (!newSelection && inputFrame.CommandPressed(Command.Up)) { CursorUp(); repeatCooldown = 250; }
                else if (!newSelection && inputFrame.CommandDown(Command.Left))
                {
                    if (repeatCooldown > 0) repeatCooldown -= gameTime.ElapsedGameTime.Milliseconds;
                    else { CursorLeft(); repeatCooldown = 30; }
                }
                else if (!newSelection && inputFrame.CommandDown(Command.Right))
                {
                    if (repeatCooldown > 0) repeatCooldown -= gameTime.ElapsedGameTime.Milliseconds;
                    else { CursorRight(); repeatCooldown = 30; }
                }
                else if (!newSelection && inputFrame.CommandDown(Command.Down))
                {
                    if (repeatCooldown > 0) repeatCooldown -= gameTime.ElapsedGameTime.Milliseconds;
                    else { CursorDown(); repeatCooldown = 30; }
                }
                else if (!newSelection && inputFrame.CommandDown(Command.Up))
                {
                    if (repeatCooldown > 0) repeatCooldown -= gameTime.ElapsedGameTime.Milliseconds;
                    else { CursorUp(); repeatCooldown = 30; }
                }
            }

            matchBoard.Speedup = inputFrame.CommandDown(Command.Speedup);
        }

        int repeatCooldown = 100;

        public void CursorUp()
        {
            if (SelectionY == 6) return;
            SelectionY--;
            Audio.PlaySound(GameSound.move_selection_cursor);
        }

        public void CursorRight()
        {
            if (SelectionX == MatchBoard.TILE_COLUMNS - 2) return;
            SelectionX++;
            Audio.PlaySound(GameSound.move_selection_cursor);
        }

        public void CursorDown()
        {
            if (SelectionY == MatchBoard.TILE_ROWS - 1) return;
            SelectionY++;
            Audio.PlaySound(GameSound.move_selection_cursor);
        }

        public void CursorLeft()
        {
            if (SelectionX == 0) return;
            SelectionX--;
            Audio.PlaySound(GameSound.move_selection_cursor);
        }

        public int SelectionX { get => matchBoard.SelectionX; set => matchBoard.SelectionX = value; }
        public int SelectionY { get => matchBoard.SelectionY; set => matchBoard.SelectionY = value; }
        public MatchTile LeftSelection { get => matchBoard.MatchTiles[SelectionX, SelectionY]; }
        public MatchTile RightSelection { get => matchBoard.MatchTiles[SelectionX + 1, SelectionY]; }
    }
}
