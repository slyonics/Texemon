using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texemon.Models;
using Texemon.SceneObjects.Controllers;
using Texemon.SceneObjects.Widgets;
using TiledCS;

namespace Texemon.Scenes.MapScene
{
    public class MapScene : Scene
    {
        private const int WALL_LENGTH = 128;

        public enum Direction
        {
            North, East, South, West, Up, Down
        }

        public string MapName { get; private set; }
        public float AmbientLight { get; private set; }

        private MapViewModel mapViewModel;

        private MovementController movementController;

        private float cameraX = 0.0f;
        private float cameraPosX = 0.0f;
        private float cameraPosZ = 0.0f;

        private MapRoom[,] mapRooms = new MapRoom[8, 8];
        public int roomX = -1;
        public int roomY = -1;
        private Direction direction;

        private Texture2D minimapSprite = AssetCache.SPRITES[GameSprite.YouAreHere];
        private static readonly Rectangle[] minimapSource = new Rectangle[] { new Rectangle(0, 0, 16, 16), new Rectangle(16, 0, 16, 16), new Rectangle(32, 0, 16, 16), new Rectangle(48, 0, 16, 16) };

        public AnimatedSprite ActorSprite { get => MapViewModel.MapActor.Value; }
        public Panel MapPanel { get; set; }

        private int bumpCooldown;

        public MapRoom GetRoom(int x, int y)
        {
            if (x < 0 || y < 0 || x >= mapRooms.GetLength(0) || y >= mapRooms.GetLength(1)) return null;
            return mapRooms[x, y];
        }

        public MapScene(string iMapName)
        {
            MapName = iMapName;

            mapViewModel = AddView(new MapViewModel(this, GameView.MapScene_MapView));
            MapPanel = mapViewModel.GetWidget<Panel>("MapPanel");

            LoadMap(MapName);

            mapRooms[roomX, roomY].EnterRoom();

            movementController = AddController(new MovementController(this));
        }

        public MapScene(string iMapName, string spawnName)
        {
            MapName = iMapName;

            mapViewModel = AddView(new MapViewModel(this, GameView.MapScene_MapView));
            MapPanel = mapViewModel.GetWidget<Panel>("MapPanel");

            LoadMap(MapName, spawnName);

            cameraX = (float)(Math.PI * (int)direction / 2.0f);

            mapRooms[roomX, roomY].EnterRoom();

            movementController = AddController(new MovementController(this));
        }

        public MapScene(string iMapName, int spawnX, int spawnY, Direction iDirection)
        {
            MapName = iMapName;

            mapViewModel = AddView<MapViewModel>(new MapViewModel(this, GameView.MapScene_MapView));
            MapPanel = mapViewModel.GetWidget<SceneObjects.Widgets.Panel>("MapPanel");

            LoadMap(MapName);

            roomX = spawnX;
            roomY = spawnY;
            direction = iDirection;
            cameraX = (float)(Math.PI * (int)direction / 2.0f);

            mapRooms[roomX, roomY].EnterRoom();

            movementController = AddController(new MovementController(this));
        }

        public void ResetPathfinding()
        {
            movementController?.Path.Clear();
        }

        public void SaveData()
        {
            GameProfile.SetSaveData<string>("LastMap", MapFileName);
            GameProfile.SetSaveData<int>("LastRoomX", roomX);
            GameProfile.SetSaveData<int>("LastRoomY", roomY);
            GameProfile.SetSaveData<Direction>("LastDirection", direction);

            GameProfile.SaveState();
        }



        private void LoadMap(string mapName, string spawnName = "Default")
        {
            TiledMap tiledMap = new TiledMap();
            tiledMap.ParseXml(AssetCache.MAPS[(GameMap)Enum.Parse(typeof(GameMap), mapName)]);

            TiledTileset tiledTileset = new TiledTileset();
            tiledTileset.ParseXml(AssetCache.MAPS[(GameMap)Enum.Parse(typeof(GameMap), Path.GetFileNameWithoutExtension(tiledMap.Tilesets[0].source))]);

            MapFileName = mapName;

            string wallSprite = "";
            TiledProperty wallProperty = tiledMap.Properties.FirstOrDefault(x => x.name == "Wall");
            if (wallProperty != null)
            {
                wallSprite = wallProperty.value;
            }

            TiledProperty nameProperty = tiledMap.Properties.FirstOrDefault(x => x.name == "Name");
            if (nameProperty != null)
            {
                MapViewModel.MapName.Value = nameProperty.value;
                MapName = nameProperty.value;
            }

            TiledProperty ambientProperty = tiledMap.Properties.FirstOrDefault(x => x.name == "AmbientLight");
            if (ambientProperty != null)
            {
                AmbientLight = float.Parse(ambientProperty.value);
            }

            MapWidth = tiledMap.Width;
            MapHeight = tiledMap.Height;
            mapRooms = new MapRoom[MapWidth, MapHeight];

            foreach (TiledLayer tiledLayer in tiledMap.Layers)
            {
                if (tiledLayer.type == TiledLayerType.TileLayer)
                {
                    int x = 0, y = 0;

                    foreach (int tileGID in tiledLayer.data)
                    {
                        int tile = tileGID - tiledMap.Tilesets[0].firstgid;
                        if (tile >= 0)
                        {
                            if (mapRooms[x, y] == null) mapRooms[x, y] = new MapRoom(this, mapRooms, x, y, wallSprite);
                            mapRooms[x, y].ApplyTile(tiledMap, tiledTileset, tile, tiledLayer.name);
                        }

                        x++;
                        if (x >= MapWidth) { x = 0; y++; }
                    }
                }
            }

            foreach (TiledLayer tiledLayer in tiledMap.Layers)
            {
                void LoadWallObject(TiledObject tiledObject)
                {
                    int centerX = (int)(tiledObject.x + tiledObject.width / 2);
                    int centerY = (int)(tiledObject.y - tiledObject.height / 2);
                    int wallGID = tiledObject.gid - tiledMap.Tilesets[0].firstgid;

                    if (centerX % WALL_LENGTH > WALL_LENGTH / 3)
                    {
                        MapRoom northRoom = mapRooms[centerX / WALL_LENGTH, (int)Math.Round((float)centerY / WALL_LENGTH) - 1];
                        MapRoom southRoom = mapRooms[centerX / WALL_LENGTH, (int)Math.Round((float)centerY / WALL_LENGTH)];
                        Texture2D texture = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Walls_" + Path.GetFileNameWithoutExtension(tiledTileset.Tiles[wallGID].image.source))];
                        if (northRoom != null && !northRoom.Blocked) northRoom.ApplyWall(Direction.South, texture);
                        if (southRoom != null && !southRoom.Blocked) southRoom.ApplyWall(Direction.North, texture);
                    }
                    else
                    {
                        MapRoom westRoom = mapRooms[(int)Math.Round((float)centerX / WALL_LENGTH) - 1, centerY / WALL_LENGTH];
                        MapRoom eastRoom = mapRooms[(int)Math.Round((float)centerX / WALL_LENGTH), centerY / WALL_LENGTH];
                        Texture2D texture = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Walls_" + Path.GetFileNameWithoutExtension(tiledTileset.Tiles[wallGID].image.source))];
                        if (westRoom != null && !westRoom.Blocked) westRoom.ApplyWall(Direction.East, texture);
                        if (eastRoom != null && !eastRoom.Blocked) eastRoom.ApplyWall(Direction.West, texture);
                    }
                }

                if (tiledLayer.type == TiledLayerType.ObjectLayer)
                {
                    if (tiledLayer.name == "Spawns" && roomX < 0 && roomY < 0)
                    {
                        TiledObject spawn = tiledLayer.objects.FirstOrDefault(x => x.name == spawnName);
                        roomX = (int)(spawn.x / WALL_LENGTH);
                        roomY = (int)(spawn.y / WALL_LENGTH);
                        direction = (Direction)Enum.Parse(typeof(Direction), spawn.properties.First(x => x.name == "Direction").value);
                        cameraX = (float)(Math.PI * (int)direction / 2.0f);
                    }

                    foreach (TiledObject tiledObject in tiledLayer.objects)
                    {
                        switch (tiledLayer.name)
                        {
                            case "Events": LoadEvent(tiledObject); break;
                            case "Walls": LoadWallObject(tiledObject); break;
                        }

                        void LoadEvent(TiledObject tiledObject)
                        {
                            if (tiledObject.type != null)
                            {
                                if (tiledObject.type == "Enter")
                                {
                                    TiledProperty disableProperty = tiledObject.properties.FirstOrDefault(x => x.name == "DisableIf");
                                    if (disableProperty == null || !GameProfile.GetSaveData<bool>(disableProperty.value))
                                    {
                                        mapRooms[(int)(tiledObject.x / WALL_LENGTH), (int)(tiledObject.y / WALL_LENGTH)].Script = tiledObject.properties.First(x => x.name == "Script").value.Split('\n');
                                    }
                                }
                                else if (tiledObject.type == "BeforeEnter")
                                {
                                    TiledProperty disableProperty = tiledObject.properties.FirstOrDefault(x => x.name == "DisableIf");
                                    if (disableProperty == null || !GameProfile.GetSaveData<bool>(disableProperty.value))
                                    {
                                        mapRooms[(int)(tiledObject.x / WALL_LENGTH), (int)(tiledObject.y / WALL_LENGTH)].PreEnterScript = tiledObject.properties.First(x => x.name == "Script").value.Split('\n');
                                    }
                                }
                                else if (tiledObject.type == "ActivateNorth")
                                {
                                    mapRooms[(int)(tiledObject.x / WALL_LENGTH), (int)(tiledObject.y / WALL_LENGTH) + 1].ActivateScript[Direction.North] = tiledObject.properties.First(x => x.name == "Script").value.Split('\n');
                                }
                                else if (tiledObject.type == "ActivateSouth")
                                {
                                    mapRooms[(int)(tiledObject.x / WALL_LENGTH), (int)(tiledObject.y / WALL_LENGTH) - 1].ActivateScript[Direction.South] = tiledObject.properties.First(x => x.name == "Script").value.Split('\n');
                                }
                                else if (tiledObject.type == "ActivateEast")
                                {
                                    mapRooms[(int)(tiledObject.x / WALL_LENGTH) - 1, (int)(tiledObject.y / WALL_LENGTH)].ActivateScript[Direction.East] = tiledObject.properties.First(x => x.name == "Script").value.Split('\n');
                                }
                                else if (tiledObject.type == "ActivateWest")
                                {
                                    mapRooms[(int)(tiledObject.x / WALL_LENGTH) + 1, (int)(tiledObject.y / WALL_LENGTH)].ActivateScript[Direction.West] = tiledObject.properties.First(x => x.name == "Script").value.Split('\n');
                                }
                            }
                        }
                    }
                }
            }

            for (int x = 0; x < MapWidth; x++)
            {
                for (int y = 0; y < MapHeight; y++)
                {
                    if (mapRooms[x, y] != null) mapRooms[x, y].BuildNeighbors();
                }
            }


            TiledLayer lightLayer = tiledMap.Layers.FirstOrDefault(x => x.type == TiledLayerType.ObjectLayer && x.name == "Lighting");
            if (lightLayer != null)
            {
                foreach (TiledObject tiledObject in lightLayer.objects) Lighting(tiledObject);
            }

            for (int x = 0; x < MapWidth; x++)
            {
                for (int y = 0; y < MapHeight; y++)
                {
                    if (mapRooms[x, y] != null) mapRooms[x, y].SetVertices(x, y);
                }
            }

            if (MapName == "School Foyer")
            {
                if (GameProfile.GetSaveData<bool>("AliensAttack") == false)
                {
                    mapRooms[2, 1].SetAsWaypoint();
                }
                else if (GameProfile.GetSaveData<bool>("AliensAttack") == true && GameProfile.GetSaveData<bool>("SaveAbby") == false)
                {
                    mapRooms[4, 1].SetAsWaypoint();
                }
                else if (GameProfile.GetSaveData<bool>("AliensAttack") == true && GameProfile.GetSaveData<bool>("SaveAbby") == true && GameProfile.GetSaveData<bool>("SaveFriends") == false)
                {
                    mapRooms[6, 2].SetAsWaypoint();
                }
                else
                {
                    mapRooms[4, 9].SetAsWaypoint();
                }
            }
        }

        void Lighting(TiledObject tiledObject)
        {
            int startX = (int)(tiledObject.x / WALL_LENGTH);
            int startY = (int)(tiledObject.y / WALL_LENGTH);
            int width = (int)(tiledObject.width / WALL_LENGTH);
            int height = (int)(tiledObject.height / WALL_LENGTH);

            int fullBrightness = 0;
            int attenuatedBrightness = 4;
            MapRoom originRoom = mapRooms[startX + width / 2, startY + height / 2];
            List<MapRoom> visitedRooms = new List<MapRoom>();
            List<MapRoom> roomsToVisit = new List<MapRoom>() { originRoom };
            List<MapRoom> nextRooms = new List<MapRoom>();
            while (attenuatedBrightness > 0)
            {
                visitedRooms.AddRange(roomsToVisit);
                foreach (MapRoom room in roomsToVisit)
                {
                    room.brightnessLevel += attenuatedBrightness;
                    nextRooms.AddRange(room.Neighbors.FindAll(x => !x.Blocked && !visitedRooms.Contains(x) && !nextRooms.Contains(x)));
                }

                roomsToVisit = nextRooms;
                nextRooms = new List<MapRoom>();

                if (fullBrightness > 0) fullBrightness--;
                else attenuatedBrightness--;
            }

            for (int x = 0; x < mapRooms.GetLength(0); x++)
            {
                for (int y = 0; y < mapRooms.GetLength(1); y++)
                {
                    MapRoom mapRoom = mapRooms[x, y];
                    mapRoom?.BlendLighting();
                }
            }
        }

        public override void BeginScene()
        {
            sceneStarted = true;

            Audio.PlayMusic(GameMusic.SMP_DUN);
        }

        public void SetWaypoint(int x, int y)
        {
            mapRooms[x, y].SetAsWaypoint();
        }

        public override void Update(GameTime gameTime, PriorityLevel priorityLevel = PriorityLevel.GameLevel)
        {
            if (Input.CurrentInput.CommandPressed(Command.Up) ||
                Input.CurrentInput.CommandPressed(Command.Down) ||
                Input.CurrentInput.CommandPressed(Command.Right) ||
                Input.CurrentInput.CommandPressed(Command.Left))
                ResetPathfinding();

            base.Update(gameTime, priorityLevel);

            if (bumpCooldown > 0) bumpCooldown -= gameTime.ElapsedGameTime.Milliseconds;
        }

        public void TurnLeft()
        {
            TransitionController transitionController = new TransitionController(TransitionDirection.Out, 300, PriorityLevel.TransitionLevel);
            AddController(transitionController);

            transitionController.UpdateTransition += new Action<float>(t => cameraX = MathHelper.Lerp(((float)(Math.PI * ((int)direction - 1) / 2.0f)), (float)(Math.PI * (int)direction / 2.0f), t));
            transitionController.FinishTransition += new Action<TransitionDirection>(t =>
            {
                if (direction == Direction.North) direction = Direction.West; else direction--;
                cameraX = (float)(Math.PI * (int)direction / 2.0f);
            });
        }

        public void TurnRight()
        {
            TransitionController transitionController = new TransitionController(TransitionDirection.In, 300, PriorityLevel.TransitionLevel);
            AddController(transitionController);

            transitionController.UpdateTransition += new Action<float>(t => cameraX = MathHelper.Lerp(((float)(Math.PI * (int)direction / 2.0f)), (float)(Math.PI * ((int)direction + 1) / 2.0f), t));
            transitionController.FinishTransition += new Action<TransitionDirection>(t =>
            {
                if (direction == Direction.West) direction = Direction.North; else direction++;
                cameraX = (float)(Math.PI * (int)direction / 2.0f);
            });
        }

        public void MoveForward()
        {
            SaveData();

            TransitionController transitionController;

            switch (direction)
            {
                case Direction.North:
                    if (roomY > 0 && mapRooms[roomX, roomY - 1] != null && !mapRooms[roomX, roomY - 1].Blocked && mapRooms[roomX, roomY].Neighbors.Contains(mapRooms[roomX, roomY - 1]))
                    {
                        if (mapRooms[roomX, roomY - 1].PreEnterScript != null) { mapRooms[roomX, roomY - 1].ActivatePreScript(); return; }

                        transitionController = new TransitionController(TransitionDirection.In, 300, PriorityLevel.TransitionLevel);
                        AddController(transitionController);
                        transitionController.UpdateTransition += new Action<float>(t => cameraPosZ = MathHelper.Lerp(0, 10, t));
                        transitionController.FinishTransition += new Action<TransitionDirection>(t => { cameraPosZ = 0; roomY--; mapRooms[roomX, roomY].EnterRoom(); });
                    }
                    else if (!Activate()) { WallBump(); return; }
                    break;
                case Direction.East:
                    if (roomX < mapRooms.GetLength(0) - 1 && mapRooms[roomX + 1, roomY] != null && !mapRooms[roomX + 1, roomY].Blocked && mapRooms[roomX, roomY].Neighbors.Contains(mapRooms[roomX + 1, roomY]))
                    {
                        if (mapRooms[roomX + 1, roomY].PreEnterScript != null) { mapRooms[roomX + 1, roomY].ActivatePreScript(); return; }

                        transitionController = new TransitionController(TransitionDirection.In, 300, PriorityLevel.TransitionLevel);
                        AddController(transitionController);
                        transitionController.UpdateTransition += new Action<float>(t => cameraPosX = MathHelper.Lerp(0, 10, t));
                        transitionController.FinishTransition += new Action<TransitionDirection>(t => { cameraPosX = 0; roomX++; mapRooms[roomX, roomY].EnterRoom(); });
                    }
                    else if (!Activate()) { WallBump(); return; }
                    break;
                case Direction.South:
                    if (roomY < mapRooms.GetLength(1) - 1 && mapRooms[roomX, roomY + 1] != null && !mapRooms[roomX, roomY + 1].Blocked && mapRooms[roomX, roomY].Neighbors.Contains(mapRooms[roomX, roomY + 1]))
                    {
                        if (mapRooms[roomX, roomY + 1].PreEnterScript != null) { mapRooms[roomX, roomY + 1].ActivatePreScript(); return; }

                        transitionController = new TransitionController(TransitionDirection.Out, 300, PriorityLevel.TransitionLevel);
                        AddController(transitionController);
                        transitionController.UpdateTransition += new Action<float>(t => cameraPosZ = MathHelper.Lerp(-10, 0, t));
                        transitionController.FinishTransition += new Action<TransitionDirection>(t => { cameraPosZ = 0; roomY++; mapRooms[roomX, roomY].EnterRoom(); });
                    }
                    else if (!Activate()) { WallBump(); return; }
                    break;
                case Direction.West:
                    if (roomX > 0 && mapRooms[roomX - 1, roomY] != null && !mapRooms[roomX - 1, roomY].Blocked && mapRooms[roomX, roomY].Neighbors.Contains(mapRooms[roomX - 1, roomY]))
                    {
                        if (mapRooms[roomX - 1, roomY].PreEnterScript != null) { mapRooms[roomX - 1, roomY].ActivatePreScript(); return; }

                        transitionController = new TransitionController(TransitionDirection.Out, 300, PriorityLevel.TransitionLevel);
                        AddController(transitionController);
                        transitionController.UpdateTransition += new Action<float>(t => cameraPosX = MathHelper.Lerp(-10, 0, t));
                        transitionController.FinishTransition += new Action<TransitionDirection>(t => { cameraPosX = 0; roomX--; mapRooms[roomX, roomY].EnterRoom(); });
                    }
                    else if (!Activate()) { WallBump(); return; }
                    break;
            }

            mapViewModel.SetActor("Actors_Blank");
        }

        public void MoveTo(MapRoom destinationRoom)
        {
            Direction requiredDirection;
            if (destinationRoom.RoomX > roomX) requiredDirection = Direction.East;
            else if (destinationRoom.RoomX < roomX) requiredDirection = Direction.West;
            else if (destinationRoom.RoomY > roomY) requiredDirection = Direction.South;
            else requiredDirection = Direction.North;

            if (requiredDirection == direction) MoveForward();
            else
            {
                if (requiredDirection == direction + 1 || (requiredDirection == Direction.North && direction == Direction.West)) TurnRight();
                else TurnLeft();
            }
        }

        private void WallBump()
        {
            if (bumpCooldown <= 0)
            {
                Audio.PlaySound(GameSound.wall_bump);
                bumpCooldown = 350;
            }
        }

        public override void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, RenderTarget2D pixelRender, RenderTarget2D compositeRender)
        {
            graphicsDevice.SetRenderTarget(CrossPlatformGame.GameInstance.mapRender);
            DrawMap(graphicsDevice);

            graphicsDevice.SetRenderTarget(CrossPlatformGame.GameInstance.minimapRender);
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, null);
            DrawMiniMap(spriteBatch);
            spriteBatch.End();

            base.Draw(graphicsDevice, spriteBatch, pixelRender, compositeRender);
        }

        private void DrawMap(GraphicsDevice graphicsDevice)
        {
            Panel mapPanel = mapViewModel.GetWidget<Panel>("MapPanel");
            if (!mapPanel.Transitioning)
            {
                Rectangle mapBounds = mapPanel.InnerBounds;
                mapBounds.X += (int)mapPanel.Position.X;
                mapBounds.Y += (int)mapPanel.Position.Y;

                graphicsDevice.Clear(new Color(0.1f, 0.1f, 0.1f));
                Vector3 cameraUp = new Vector3(0, -1, 0);
                Vector3 cameraPos = new Vector3(cameraPosX + 10 * roomX, 0, cameraPosZ + 10 * (mapRooms.GetLength(1) - roomY));
                Matrix viewMatrix = Matrix.CreateLookAt(cameraPos, cameraPos + Vector3.Transform(new Vector3(0, 0, 1), Matrix.CreateRotationY(cameraX)), cameraUp);

                for (int x = 0; x < mapRooms.GetLength(0); x++)
                {
                    for (int y = 0; y < mapRooms.GetLength(1); y++)
                    {
                        mapRooms[x, y]?.Draw(viewMatrix);
                    }
                }
            }
        }

        private void DrawMiniMap(SpriteBatch spriteBatch)
        {
            Panel miniMapPanel = mapViewModel.GetWidget<Panel>("MiniMapPanel");
            if (!miniMapPanel.Transitioning && miniMapPanel.Visible)
            {
                Vector2 offset = new Vector2((CrossPlatformGame.GameInstance.minimapRender.Width - (mapRooms.GetLength(0) * 16)) / 2, (CrossPlatformGame.GameInstance.minimapRender.Height - (mapRooms.GetLength(1) * 16)) / 2);
                for (int x = 0; x < mapRooms.GetLength(0); x++)
                {
                    for (int y = 0; y < mapRooms.GetLength(1); y++)
                    {
                        MapRoom mapRoom = mapRooms[x, y];
                        mapRoom?.DrawMinimap(spriteBatch, offset);
                    }
                }

                spriteBatch.Draw(minimapSprite, new Vector2(roomX * 16, roomY * 16) + offset, minimapSource[(int)direction], Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);

            }
        }

        public bool Activate()
        {
            return mapRooms[roomX, roomY].Activate(direction);
        }

        public void MiniMapClick(Vector2 clickPosition)
        {
            if (PriorityLevel != PriorityLevel.GameLevel || controllerList.Any(x => x.Any(y => y is EventController))) return;

            Panel miniMapPanel = mapViewModel.GetWidget<Panel>("MiniMapPanel");
            clickPosition -= new Vector2((miniMapPanel.InnerBounds.Width - (mapRooms.GetLength(0) * 16 * CrossPlatformGame.Scale)) / 2, (miniMapPanel.InnerBounds.Height - (mapRooms.GetLength(1) * 16 * CrossPlatformGame.Scale)) / 2);
            clickPosition /= CrossPlatformGame.Scale;
            int newRoomX = (int)clickPosition.X / 16;
            int newRoomY = (int)clickPosition.Y / 16;

            if (newRoomX >= 0 && newRoomY >= 0 && newRoomX < mapRooms.GetLength(0) && newRoomY < mapRooms.GetLength(1))
            {
                if (mapRooms[newRoomX, newRoomY] != null && !mapRooms[newRoomX, newRoomY].Blocked)
                {
                    movementController.Path = GetPath(mapRooms[roomX, roomY], mapRooms[newRoomX, newRoomY]);
                }
            }
        }


        public List<MapRoom> GetPath(MapRoom startTile, MapRoom endTile)
        {
            List<MapRoom> processedTiles = new List<MapRoom>();
            List<MapRoom> unprocessedTiles = new List<MapRoom> { startTile };
            Dictionary<MapRoom, MapRoom> cameFrom = new Dictionary<MapRoom, MapRoom>();
            Dictionary<MapRoom, int> currentDistance = new Dictionary<MapRoom, int>();
            Dictionary<MapRoom, int> predictedDistance = new Dictionary<MapRoom, int>();

            currentDistance.Add(startTile, 0);
            predictedDistance.Add(startTile, Distance(startTile, endTile));

            while (unprocessedTiles.Count > 0)
            {
                // get the node with the lowest estimated cost to finish
                MapRoom current = (from p in unprocessedTiles orderby predictedDistance[p] ascending select p).First();

                // if it is the finish, return the path
                if (current.RoomX == endTile.RoomX && current.RoomY == endTile.RoomY)
                {
                    // generate the found path
                    return ReconstructPath(cameFrom, endTile);
                }

                // move current node from open to closed
                unprocessedTiles.Remove(current);
                processedTiles.Add(current);

                foreach (MapRoom neighbor in current.Neighbors)
                {
                    int tempCurrentDistance = currentDistance[current] + Distance(neighbor, endTile);

                    // if we already know a faster way to this neighbor, use that route and ignore this one
                    if (processedTiles.Contains(neighbor) && tempCurrentDistance >= currentDistance[neighbor]) continue;

                    // if we don't know a route to this neighbor, or if this is faster, store this route
                    if (!processedTiles.Contains(neighbor) || tempCurrentDistance < currentDistance[neighbor])
                    {
                        if (cameFrom.Keys.Contains(neighbor)) cameFrom[neighbor] = current;
                        else cameFrom.Add(neighbor, current);

                        currentDistance[neighbor] = tempCurrentDistance;
                        predictedDistance[neighbor] = currentDistance[neighbor] + Distance(neighbor, endTile);

                        if (!unprocessedTiles.Contains(neighbor)) unprocessedTiles.Add(neighbor);
                    }
                }
            }

            return null;
        }

        private int Distance(MapRoom room1, MapRoom room2)
        {
            return Math.Abs(room1.RoomX - room2.RoomX) + Math.Abs(room1.RoomY - room2.RoomY);
        }

        private static List<MapRoom> ReconstructPath(Dictionary<MapRoom, MapRoom> cameFrom, MapRoom current)
        {
            if (!cameFrom.Keys.Contains(current))
            {
                return new List<MapRoom> { current };
            }

            List<MapRoom> path = ReconstructPath(cameFrom, cameFrom[current]);
            path.Add(current);
            return path;
        }

        public MapViewModel MapViewModel { get => mapViewModel; }

        public int MapWidth { get; set; }
        public int MapHeight { get; set; }

        public string MapFileName { get; set; }

        public Color RoomLighting
        {
            get
            {
                MapRoom currentRoom = mapRooms[roomX, roomY];
                Color color = Color.White;
                color.R = color.G = color.B = (byte)(currentRoom.Brightness(currentRoom.brightnessLevel) * 255);
                return color;
            }
        }
    }
}
