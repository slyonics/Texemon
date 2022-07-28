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

namespace Texemon.Scenes.MapScene
{
    public class MapRoom
    {
        public class RoomWall
        {
            public MapScene.Direction Orientation { get; set; }
            public VertexPositionTexture[] Quad { get; set; }
            public Vector4[] Lighting { get; set; } = new Vector4[] { new Vector4(1.0f), new Vector4(1.0f), new Vector4(1.0f), new Vector4(1.0f) };
            public Texture2D Texture { get; set; }
            public WallShader Shader { get; set; }

        }

        private const int ATLAS_ROWS = 16;
        private const int WALL_SPRITE_LENGTH = 128;
        private const int ATLAS_LENGTH = 2048;
        private const int WALL_HALF_LENGTH = 5;
        private const int CAM_HEIGHT = -1;
        private static readonly short[] INDICES = new short[] { 0, 2, 1, 2, 0, 3 };
        private static readonly Dictionary<MapScene.Direction, Vector3[]> VERTICES = new Dictionary<MapScene.Direction, Vector3[]>()
        {   {
                MapScene.Direction.North, new Vector3[] {
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH) }
            }, {
                MapScene.Direction.West, new Vector3[] {
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH) }
            }, {
                MapScene.Direction.East, new Vector3[] {
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH) }
            }, {
                MapScene.Direction.South, new Vector3[] {
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH) }
            }, {
                MapScene.Direction.Up, new Vector3[] {
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH) }
            }, {
                MapScene.Direction.Down, new Vector3[] {
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH) }
        } };

        private Texture2D minimapSprite = AssetCache.SPRITES[GameSprite.MiniMap];
        private static readonly Rectangle[] minimapSource = new Rectangle[] { new Rectangle(0, 0, 16, 16), new Rectangle(16, 0, 16, 16), new Rectangle(32, 0, 16, 16), new Rectangle(48, 0, 16, 16) };

        private MapRoom[,] mapRooms;
        public int RoomX { get; set; }
        public int RoomY { get; set; }
        public bool Blocked { get; set; }
        public string[] Script { get; set; }
        public string[] PreEnterScript { get; set; }
        public Dictionary<MapScene.Direction, string[]> ActivateScript { get; set; } = new Dictionary<MapScene.Direction, string[]>();

        //public WallShader WallEffect { get; private set; }
        private GraphicsDevice graphicsDevice = CrossPlatformGame.GameInstance.GraphicsDevice;

        private MapScene parentScene;
        private Matrix translationMatrix;

        private GameSprite defaultWall;

        private bool door = false;
        int waypointTile;

        private Dictionary<MapScene.Direction, RoomWall> wallList = new Dictionary<MapScene.Direction, RoomWall>();

        public int brightnessLevel = 0;
        private float[] lightVertices;

        public MapRoom(MapScene mapScene, MapRoom[,] iMapRooms, int x, int y, string wallSprite)
        {
            parentScene = mapScene;
            mapRooms = iMapRooms;
            RoomX = x;
            RoomY = y;
            defaultWall = (GameSprite)Enum.Parse(typeof(GameSprite), "Walls_" + wallSprite);
        }

        public void ApplyTile(TiledCS.TiledMap tiledMap, TiledCS.TiledTileset tiledTileset, int gid, string layerName)
        {
            TiledCS.TiledTile tileTemplate = tiledTileset.Tiles.FirstOrDefault(x => x.id == gid);

            if (tileTemplate != null)
            {
                TiledCS.TiledProperty doorProperty = tileTemplate.properties.FirstOrDefault(x => x.name == "Door");
                if (doorProperty != null)
                {
                    door = bool.Parse(doorProperty.value);
                }
                else door = false;
            }


            switch (layerName)
            {
                case "Walls":
                    Blocked = true;
                    if (RoomX > 0 && mapRooms[RoomX - 1, RoomY] != null && !mapRooms[RoomX - 1, RoomY].Blocked)
                        mapRooms[RoomX - 1, RoomY].ApplyWall(MapScene.Direction.East, AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Walls_" + Path.GetFileNameWithoutExtension(tiledTileset.Tiles[gid].image.source))]);

                    if (RoomX < mapRooms.GetLength(0) - 1 && mapRooms[RoomX + 1, RoomY] != null && !mapRooms[RoomX + 1, RoomY].Blocked)
                        mapRooms[RoomX + 1, RoomY].ApplyWall(MapScene.Direction.West, AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Walls_" + Path.GetFileNameWithoutExtension(tiledTileset.Tiles[gid].image.source))]);

                    if (RoomY > 0 && mapRooms[RoomX, RoomY - 1] != null && !mapRooms[RoomX, RoomY - 1].Blocked)
                        mapRooms[RoomX, RoomY - 1].ApplyWall(MapScene.Direction.South, AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Walls_" + Path.GetFileNameWithoutExtension(tiledTileset.Tiles[gid].image.source))]);

                    if (RoomY < mapRooms.GetLength(1) - 1 && mapRooms[RoomX, RoomY + 1] != null && !mapRooms[RoomX, RoomY + 1].Blocked)
                        mapRooms[RoomX, RoomY + 1].ApplyWall(MapScene.Direction.North, AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Walls_" + Path.GetFileNameWithoutExtension(tiledTileset.Tiles[gid].image.source))]);
                    break;

                case "Ceiling":
                    wallList.Add(MapScene.Direction.Up, new RoomWall() { Orientation = MapScene.Direction.Up, Texture = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Walls_" + Path.GetFileNameWithoutExtension(tiledTileset.Tiles[gid].image.source))] });
                    break;

                case "Floor":
                    wallList.Add(MapScene.Direction.Down, new RoomWall() { Orientation = MapScene.Direction.Down, Texture = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Walls_" + Path.GetFileNameWithoutExtension(tiledTileset.Tiles[gid].image.source))] });
                    break;
            }

            ResetMinimapIcon();
        }

        public void ApplyWall(MapScene.Direction direction, Texture2D texture2D)
        {
            if (wallList.TryGetValue(direction, out var wall))
            {
                wall.Texture = texture2D;
            }
            else
            {
                wallList.Add(direction, new RoomWall()
                {
                    Orientation = direction,
                    Texture = texture2D
                });
            }
        }

        public void SetVertices(int x, int y)
        {
            translationMatrix = Matrix.CreateTranslation(new Vector3(10 * (x), 0, 10 * (mapRooms.GetLength(1) - y)));

            foreach (KeyValuePair<MapScene.Direction, RoomWall> wall in wallList)
            {
                VertexPositionTexture[] quad = new VertexPositionTexture[4];
                quad[0] = new VertexPositionTexture(VERTICES[wall.Value.Orientation][0], new Vector2(0.0f, 0.0f));
                quad[1] = new VertexPositionTexture(VERTICES[wall.Value.Orientation][1], new Vector2(0.0f, 1.0f));
                quad[2] = new VertexPositionTexture(VERTICES[wall.Value.Orientation][2], new Vector2(1.0f, 1.0f));
                quad[3] = new VertexPositionTexture(VERTICES[wall.Value.Orientation][3], new Vector2(1.0f, 0.0f));
                wall.Value.Quad = quad;
            }

            BuildShader();
        }

        private void BuildShader()
        {
            foreach (KeyValuePair<MapScene.Direction, RoomWall> wall in wallList)
            {
                wall.Value.Shader = new WallShader(Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 2f, 472 / 332.0f, 0.7f, 10000.0f));
                wall.Value.Shader.World = translationMatrix;
                wall.Value.Shader.WallTexture = wall.Value.Texture;

                Vector4 brightness;

                switch (wall.Value.Orientation)
                {
                    case MapScene.Direction.Up: brightness = new Vector4(Brightness(lightVertices[2]), Brightness(lightVertices[3]), Brightness(lightVertices[0]), Brightness(lightVertices[1])); break;
                    case MapScene.Direction.North: brightness = new Vector4(Brightness(lightVertices[1]), Brightness(lightVertices[0]), Brightness(lightVertices[3]), Brightness(lightVertices[2])); break;
                    case MapScene.Direction.East: brightness = new Vector4(Brightness(lightVertices[3]), Brightness(lightVertices[1]), Brightness(lightVertices[2]), Brightness(lightVertices[0])); break;
                    case MapScene.Direction.West: brightness = new Vector4(Brightness(lightVertices[0]), Brightness(lightVertices[2]), Brightness(lightVertices[1]), Brightness(lightVertices[3])); break;
                    default: brightness = new Vector4(Brightness(lightVertices[0]), Brightness(lightVertices[1]), Brightness(lightVertices[2]), Brightness(lightVertices[3])); break;
                }

                wall.Value.Shader.Brightness = brightness;
            }
        }

        public float Brightness(float x) { return Math.Min(1.0f, Math.Max(x / 4.0f, parentScene.AmbientLight)); }

        public void BlendLighting()
        {
            lightVertices = new float[] { 0.25f, 0.25f, 0.25f, 0.25f };

            int[] neighborBrightness = new int[9];
            int i = 0;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (RoomX + x < 0 || RoomY + y < 0 || RoomX + x >= mapRooms.GetLength(0) || RoomY + y >= mapRooms.GetLength(1)) neighborBrightness[i] = brightnessLevel;
                    else
                    {
                        MapRoom mapRoom = mapRooms[RoomX + x, RoomY + y];
                        neighborBrightness[i] = mapRoom == null || mapRoom.Blocked ? brightnessLevel : mapRoom.brightnessLevel;
                    }
                    i++;
                }
            }

            List<MapRoom> nw = new List<MapRoom>() { this };
            if (Neighbors.Contains(this[MapScene.Direction.North])) { nw.Add(this[MapScene.Direction.North]); if (this[MapScene.Direction.North].Neighbors.Contains(this[MapScene.Direction.North][MapScene.Direction.West])) nw.Add(this[MapScene.Direction.North][MapScene.Direction.West]); }
            if (Neighbors.Contains(this[MapScene.Direction.West])) { nw.Add(this[MapScene.Direction.West]); nw.Add(this[MapScene.Direction.West]); if (this[MapScene.Direction.West].Neighbors.Contains(this[MapScene.Direction.West][MapScene.Direction.North])) nw.Add(this[MapScene.Direction.West][MapScene.Direction.North]); }
            lightVertices[0] = (float)nw.Distinct().Average(x => x.brightnessLevel);

            List<MapRoom> ne = new List<MapRoom>() { this };
            if (Neighbors.Contains(this[MapScene.Direction.North])) { ne.Add(this[MapScene.Direction.North]); if (this[MapScene.Direction.North].Neighbors.Contains(this[MapScene.Direction.North][MapScene.Direction.East])) ne.Add(this[MapScene.Direction.North][MapScene.Direction.East]); }
            if (Neighbors.Contains(this[MapScene.Direction.East])) { ne.Add(this[MapScene.Direction.East]); ne.Add(this[MapScene.Direction.East]); if (this[MapScene.Direction.East].Neighbors.Contains(this[MapScene.Direction.East][MapScene.Direction.North])) ne.Add(this[MapScene.Direction.East][MapScene.Direction.North]); }
            lightVertices[1] = (float)ne.Distinct().Average(x => x.brightnessLevel);

            List<MapRoom> sw = new List<MapRoom>() { this };
            if (Neighbors.Contains(this[MapScene.Direction.South])) { sw.Add(this[MapScene.Direction.South]); if (this[MapScene.Direction.South].Neighbors.Contains(this[MapScene.Direction.South][MapScene.Direction.West])) sw.Add(this[MapScene.Direction.South][MapScene.Direction.West]); }
            if (Neighbors.Contains(this[MapScene.Direction.West])) { sw.Add(this[MapScene.Direction.West]); sw.Add(this[MapScene.Direction.West]); if (this[MapScene.Direction.West].Neighbors.Contains(this[MapScene.Direction.West][MapScene.Direction.South])) sw.Add(this[MapScene.Direction.West][MapScene.Direction.South]); }
            lightVertices[2] = (float)sw.Distinct().Average(x => x.brightnessLevel);

            List<MapRoom> se = new List<MapRoom>() { this };
            if (Neighbors.Contains(this[MapScene.Direction.South])) { se.Add(this[MapScene.Direction.South]); if (this[MapScene.Direction.South].Neighbors.Contains(this[MapScene.Direction.South][MapScene.Direction.East])) se.Add(this[MapScene.Direction.South][MapScene.Direction.East]); }
            if (Neighbors.Contains(this[MapScene.Direction.East])) { se.Add(this[MapScene.Direction.East]); se.Add(this[MapScene.Direction.East]); if (this[MapScene.Direction.East].Neighbors.Contains(this[MapScene.Direction.East][MapScene.Direction.South])) se.Add(this[MapScene.Direction.East][MapScene.Direction.South]); }
            lightVertices[3] = (float)se.Distinct().Average(x => x.brightnessLevel);
        }

        public void Draw(Matrix viewMatrix)
        {
            if (Blocked) return;

            foreach (KeyValuePair<MapScene.Direction, RoomWall> wall in wallList)
            {
                DrawWall(wall.Value, viewMatrix);
            }
        }

        public void DrawWall(RoomWall wall, Matrix viewMatrix)
        {
            wall.Shader.View = viewMatrix;
            foreach (EffectPass pass in wall.Shader.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, wall.Quad, 0, 4, INDICES, 0, 2);
            }
        }

        public void DrawMinimap(SpriteBatch spriteBatch, Vector2 offset)
        {
            spriteBatch.Draw(minimapSprite, new Vector2(RoomX * 16, RoomY * 16) + offset, minimapSource[waypointTile], Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
        }

        public void ResetMinimapIcon()
        {
            waypointTile = Blocked ? 0 : 1;
            if (door) waypointTile = 2;
        }

        public void BuildNeighbors()
        {
            if (!wallList.ContainsKey(MapScene.Direction.West))
            {
                if (RoomX > 0 && mapRooms[RoomX - 1, RoomY] != null && !mapRooms[RoomX - 1, RoomY].Blocked) Neighbors.Add(mapRooms[RoomX - 1, RoomY]);
                else wallList.Add(MapScene.Direction.West, new RoomWall() { Orientation = MapScene.Direction.West, Texture = AssetCache.SPRITES[defaultWall] });
            }

            if (!wallList.ContainsKey(MapScene.Direction.East))
            {
                if (RoomX < mapRooms.GetLength(0) - 1 && mapRooms[RoomX + 1, RoomY] != null && !mapRooms[RoomX + 1, RoomY].Blocked) Neighbors.Add(mapRooms[RoomX + 1, RoomY]);
                else wallList.Add(MapScene.Direction.East, new RoomWall() { Orientation = MapScene.Direction.East, Texture = AssetCache.SPRITES[defaultWall] });
            }

            if (!wallList.ContainsKey(MapScene.Direction.North))
            {
                if (RoomY > 0 && mapRooms[RoomX, RoomY - 1] != null && !mapRooms[RoomX, RoomY - 1].Blocked) Neighbors.Add(mapRooms[RoomX, RoomY - 1]);
                else wallList.Add(MapScene.Direction.North, new RoomWall() { Orientation = MapScene.Direction.North, Texture = AssetCache.SPRITES[defaultWall] });
            }

            if (!wallList.ContainsKey(MapScene.Direction.South))
            {
                if (RoomY < mapRooms.GetLength(1) - 1 && mapRooms[RoomX, RoomY + 1] != null && !mapRooms[RoomX, RoomY + 1].Blocked) Neighbors.Add(mapRooms[RoomX, RoomY + 1]);
                else if (!wallList.ContainsKey(MapScene.Direction.South)) wallList.Add(MapScene.Direction.South, new RoomWall() { Orientation = MapScene.Direction.South, Texture = AssetCache.SPRITES[defaultWall] });
            }
        }

        public bool Activate(MapScene.Direction direction)
        {
            string[] script;
            if (ActivateScript.TryGetValue(direction, out script))
            {
                EventController eventController = new EventController(parentScene, script, this);
                parentScene.AddController(eventController);
                parentScene.ResetPathfinding();

                return true;
            }
            else return false;
        }

        public void ActivatePreScript()
        {
            if (PreEnterScript != null)
            {
                EventController eventController = new EventController(parentScene, PreEnterScript, this);
                parentScene.AddController(eventController);
                parentScene.ResetPathfinding();
            }
        }

        public void EnterRoom()
        {
            if (Script != null)
            {
                EventController eventController = new EventController(parentScene, Script, this);
                parentScene.AddController(eventController);
                parentScene.ResetPathfinding();
            }
            else
            {
                if (GameProfile.GetSaveData<bool>("AliensAttack"))
                {
                    int stepsRemaining = GameProfile.GetSaveData<int>("RandomBattle");
                    stepsRemaining--;
                    GameProfile.SetSaveData<int>("RandomBattle", stepsRemaining);

                    if (stepsRemaining < 0)
                    {
                        stepsRemaining = Rng.RandomInt(8, 14);
                        GameProfile.SetSaveData<int>("RandomBattle", stepsRemaining);

                        int alienPower = 1;
                        if (parentScene.roomX >= 5 || parentScene.MapFileName == "Class4" || parentScene.MapFileName == "Class5" || parentScene.MapFileName == "Class6" || parentScene.MapFileName == "Class7") alienPower = Rng.WeightedEntry<int>(new Dictionary<int, double>() { { 1, 3 }, { 2, 7 }, { 3, 2 } });

                        string[] script = new string[] { "Encounter Alien" + alienPower };

                        EventController eventController = new EventController(parentScene, script, mapRooms[parentScene.roomX, parentScene.roomY]);
                        parentScene.AddController(eventController);
                        parentScene.ResetPathfinding();
                    }
                }
            }
        }

        public void SetAsWaypoint()
        {
            waypointTile = 3;
        }

        public List<MapRoom> Neighbors { get; private set; } = new List<MapRoom>();

        public MapRoom this[MapScene.Direction key]
        {
            get
            {
                switch (key)
                {
                    case MapScene.Direction.North: return parentScene.GetRoom(RoomX, RoomY - 1);
                    case MapScene.Direction.East: return parentScene.GetRoom(RoomX + 1, RoomY);
                    case MapScene.Direction.South: return parentScene.GetRoom(RoomX, RoomY + 1);
                    case MapScene.Direction.West: return parentScene.GetRoom(RoomX - 1, RoomY);
                    default: return null;
                }
            }
        }
    }
}
