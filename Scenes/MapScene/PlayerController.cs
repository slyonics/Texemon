using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Texemon.Scenes.MapScene
{
    public interface IInteractive
    {
        bool Activate(Player activator);
        Rectangle Bounds { get; }
        string Label { get; }
        Vector2 LabelPosition { get; }
    }

    public class PlayerController : Controller
    {
        private const int INPUT_BUFFER_AGE_LIMIT = 3000;

        private MapScene mapScene;
        private Player player;
        private PlayerProfile playerProfile;

        private TechniqueController techniqueController = null;
        private List<BufferedInput> bufferedInputList = new List<BufferedInput>();

        private IInteractive interactable;

        private WidgetMenuModel mainMenu;

        public PlayerController(MapScene iMapScene, Player iPlayer)
            : base(PriorityLevel.GameLevel)
        {
            mapScene = iMapScene;
            player = iPlayer;
            playerProfile = player.PlayerProfile;

            InteractionView interactionView = new InteractionView(mapScene, this);
            mapScene.AddView(interactionView);
        }

        public override void PreUpdate(GameTime gameTime)
        {
            if (player.Flinching || player.Dead) return;
            if (player.ControllerList.Exists(x => x is PettingController)) return;

            AgeBufferedInputs(gameTime);

            Vector2 movement = Vector2.Zero;
            PlayerInput playerInput = Input.GetPlayerInput(playerProfile.PlayerNumber);
            if (playerInput != null && CrossPlatformGame.SceneTransition == null)
            {
                movement = new Vector2(playerInput.AxisX, -playerInput.AxisY);
                if (movement.Length() < Input.THUMBSTICK_DEADZONE_THRESHOLD)
                {
                    if (playerInput.CommandDown(Command.Left)) movement.X -= 1.0f;
                    if (playerInput.CommandDown(Command.Right)) movement.X += 1.0f;
                    if (playerInput.CommandDown(Command.Up)) movement.Y -= 1.0f;
                    if (playerInput.CommandDown(Command.Down)) movement.Y += 1.0f;
                }

                if (playerInput.CommandDown(Command.Menu)) LaunchMenu();
                else if (!mapScene.GameMap.Peaceful)
                {
                    for (Command command = Command.Technique1; command <= Command.Technique4; command++)
                    {
                        if (playerInput.CommandPressed(command))
                        {
                            TechniqueData technique = playerProfile.Techniques[command];

                            bufferedInputList.RemoveAll(x => x.techniqueData == technique);
                            bufferedInputList.Add(new BufferedInput(technique, movement));

                            if (techniqueController == null)
                            {
                                techniqueController = new TechniqueController(mapScene, player, technique, movement);
                                mapScene.AddController(techniqueController);
                                bufferedInputList.Clear();
                            }
                        }
                    }
                }
            }

            if (techniqueController != null) CancelTechniques(gameTime);
            if (techniqueController == null)
            {
                if (playerInput.CommandPressed(Command.Interact) && interactable != null)
                {
                    if (interactable.Activate(player)) return;
                }

                MovePlayer(movement);
            }
        }

        public override void PostUpdate(GameTime gameTime)
        {
            List<IInteractive> interactableList = new List<IInteractive>(mapScene.VillagerList);
            interactableList.AddRange(mapScene.FollowerList.FindAll(x => x.Interactive));
            interactableList.AddRange(mapScene.EventList.FindAll(x => x.Interactive));

            IOrderedEnumerable<IInteractive> sortedInteractableList = interactableList.OrderBy(x => player.Distance(x.Bounds));
            Rectangle interactionZone = player.Bounds;
            switch (player.Orientation)
            {
                case Orientation.Up: interactionZone.Y -= (int)(player.BoundingBox.Height * 1.5f); break;
                case Orientation.Right: interactionZone.X += (int)(player.BoundingBox.Width * 1.5f); break;
                case Orientation.Down: interactionZone.Y += (int)(player.BoundingBox.Height * 1.5f); break;
                case Orientation.Left: interactionZone.X -= (int)(player.BoundingBox.Width * 1.5f); break;
            }

            interactable = sortedInteractableList.FirstOrDefault(x => x.Bounds.Intersects(interactionZone));
        }

        private void AgeBufferedInputs(GameTime gameTime)
        {
            foreach (BufferedInput bufferedInput in bufferedInputList) bufferedInput.age += gameTime.ElapsedGameTime.Milliseconds;
            bufferedInputList.RemoveAll(x => x.age > INPUT_BUFFER_AGE_LIMIT);
        }

        private void CancelTechniques(GameTime gameTime)
        {
            BufferedInput cancellingInput = techniqueController.ProcessBufferedInput(bufferedInputList);
            if (cancellingInput == null)
            {
                techniqueController.PreUpdate(gameTime);
                if (techniqueController.Terminated) techniqueController = null;
            }
            else
            {
                bufferedInputList.Clear();

                techniqueController.Terminate();
                if (cancellingInput.techniqueData == null) techniqueController = null;
                else
                {
                    techniqueController = new TechniqueController(mapScene, player, cancellingInput.techniqueData, cancellingInput.techniqueMovement);
                    mapScene.AddController(techniqueController);
                }
            }
        }

        private void MovePlayer(Vector2 movement)
        {
            if (movement.Length() < Input.THUMBSTICK_DEADZONE_THRESHOLD) player.Idle();
            else
            {
                movement.Normalize();
                player.Walk(movement, Player.WALKING_SPEED);
            }
        }

        private void LaunchMenu()
        {
            mainMenu = new WidgetMenuModel(new Rectangle(-50, -40, 100, 42), true);
            mainMenu.AddWidget(new Button(mainMenu.FlowPosition, "Resume Game", ResumeGame));
            mainMenu.AddWidget(new Button(mainMenu.FlowPosition, "Options", ResumeGame));
            mainMenu.AddWidget(new Button(mainMenu.FlowPosition, "Quit to Title", QuitToTitle));

            mapScene.AddMenu(mainMenu);
        }

        private void ResumeGame()
        {
            mainMenu.Terminate();
        }

        private void QuitToTitle()
        {
            FadeTransition transitionOut = new FadeTransition(Color.Black, Transition.TransitionState.Out, 600);
            CrossPlatformGame.LoadScene(typeof(TitleScene), transitionOut);
        }

        public Player Player { get => player; }
        public PlayerNumber PlayerNumber { get => player.PlayerProfile.PlayerNumber; }
        public IInteractive Interactable { get => interactable; }
    }
}
