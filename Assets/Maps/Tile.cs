using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Panderling.GameObjects.Actors;
using Panderling.Main;

using TiledSharp;

namespace Panderling.GameObjects.Maps
{
    public class Tile
    {
        public const int TILE_SIZE = 16;

        private const float BACKGROUND_START_DEPTH = (Camera.MAXIMUM_ENTITY_DEPTH + 1.0f) / 2;
        private const float FOREGROUND_START_DEPTH = (Camera.MINIMUM_ENTITY_DEPTH + 0.0f) / 2;
        private const int MAXIMUM_OBSTACLE_LAYERS = 6;

        private TmxMap tmxMap;
        private Map map;

        private int tileX;
        private int tileY;
        private int tileI;
        private Vector2 center;

        private List<TileSprite> tileSpriteList = new List<TileSprite>();
        private List<Tile> neighborList = new List<Tile>();
        private List<Rectangle> colliderList = new List<Rectangle>();

        private bool blocksSight;
        private bool visibleToPlayer;

        public Tile(Map iMap, TmxMap iTmxMap, int x, int y, int i)
        {
            map = iMap;
            tmxMap = iTmxMap;
            tileX = x;
            tileY = y;
            tileI = i;
            center = new Vector2(x * TILE_SIZE + TILE_SIZE / 2, y * TILE_SIZE + TILE_SIZE / 2);
        }

        public Tile(Map iMap, int x, int y, int i)
        {
            map = iMap;
            tmxMap = null;
            tileX = x;
            tileY = y;
            tileI = i;
            center = new Vector2(x * TILE_SIZE + TILE_SIZE / 2, y * TILE_SIZE + TILE_SIZE / 2);
        }

        public void AssignNeighbors()
        {
            if (tileX > 0) neighborList.Add(map.GetTile(tileX - 1, tileY));
            if (tileY > 0) neighborList.Add(map.GetTile(tileX, tileY - 1));
            if (tileY < map.MapHeight - 1) neighborList.Add(map.GetTile(tileX, tileY + 1));
            if (tileX < map.MapWidth - 1) neighborList.Add(map.GetTile(tileX + 1, tileY));

            if (tileX > 0 && tileY > 0) neighborList.Add(map.GetTile(tileX - 1, tileY - 1));
            if (tileX > 0 && tileY < map.MapHeight - 1) neighborList.Add(map.GetTile(tileX - 1, tileY + 1));
            if (tileX < map.MapWidth - 1 && tileY > 0) neighborList.Add(map.GetTile(tileX + 1, tileY - 1));
            if (tileX < map.MapWidth - 1 && tileY < map.MapHeight - 1) neighborList.Add(map.GetTile(tileX + 1, tileY + 1));
        }

        public void AssignLayers()
        {
            if (tmxMap == null) return;

            foreach (TmxGroup tmxGroup in tmxMap.Groups)
            {
                float depth = 0.0f;
                float depthOffset = -0.01f;
                TileSprite.LayerType tileLayerType = TileSprite.LayerType.Background;
                switch (tmxGroup.Name)
                {
                    case "Background":
                        tileLayerType = TileSprite.LayerType.Background;
                        depth = BACKGROUND_START_DEPTH;
                        break;

                    case "Foreground":
                        tileLayerType = TileSprite.LayerType.Foreground;
                        depth = FOREGROUND_START_DEPTH;
                        break;

                    case "Obstacles":
                        tileLayerType = TileSprite.LayerType.Obstacle;
                        depthOffset = -(Camera.MAXIMUM_ENTITY_DEPTH - Camera.MINIMUM_ENTITY_DEPTH) / (CrossPlatformGame.ScreenHeight + Camera.LARGEST_ENTITY_SIZE) / MAXIMUM_OBSTACLE_LAYERS;
                        break;
                }

                List<TmxLayer> layerList = GetAllLayers(tmxGroup);
                foreach (TmxLayer tmxLayer in layerList)
                {
                    string propertyString;
                    int layerHeight = -1;
                    if (tmxLayer.Properties.TryGetValue("Height", out propertyString))
                    {
                        layerHeight = int.Parse(propertyString);
                    }

                    TmxLayerTile tmxLayerTile = tmxLayer.Tiles[tileI];
                    if (tmxLayerTile.Gid == 0) continue;

                    TmxTileset tmxTileset = tmxMap.Tilesets.LastOrDefault(tileset => tileset.FirstGid <= tmxLayerTile.Gid);
                    TileSprite tileSprite = new TileSprite(tmxLayer, tmxLayerTile, tmxTileset, this, tileX, tileY, depth, map.LightList, tileLayerType, layerHeight);
                    tileSpriteList.Add(tileSprite);

                    if (tileSprite.TilesetData != null) AddColliders(tmxLayer, tileSprite.TilesetData.ObjectGroups);

                    depth += depthOffset;
                }
            }

            tmxMap = null;
        }

        private List<TmxLayer> GetAllLayers(TmxGroup tmxGroup)
        {
            List<TmxLayer> result = new List<TmxLayer>();

            foreach (TmxLayer tmxLayer in tmxGroup.Layers) result.Add(tmxLayer);
            foreach (TmxGroup childGroup in tmxGroup.Groups) result.AddRange(GetAllLayers(childGroup));            

            return result;
        }

        private void AddColliders(TmxLayer tmxLayer, TmxList<TmxObjectGroup> tmxObjectGroupList)
        {
            foreach (TmxObjectGroup tmxObjectGroup in tmxObjectGroupList)
            {
                foreach (TmxObject tmxObject in tmxObjectGroup.Objects)
                {
                    int colliderLeft = tileX * TILE_SIZE + (int)tmxObject.X + (int)tmxLayer.OffsetX;
                    int colliderTop = tileY * TILE_SIZE + (int)tmxObject.Y + (int)tmxLayer.OffsetY;
                    int colliderRight = colliderLeft + (int)tmxObject.Width;
                    int colliderBottom = colliderTop + (int)tmxObject.Height;

                    Rectangle fullCollider = new Rectangle(colliderLeft, colliderTop, colliderRight - colliderLeft, colliderBottom - colliderTop);
                    Rectangle tileBounds;
                    Rectangle collider;
                    foreach (Tile neighbor in neighborList)
                    {
                        tileBounds = new Rectangle(neighbor.TileX * TILE_SIZE, neighbor.TileY * TILE_SIZE, TILE_SIZE, TILE_SIZE);
                        collider = Rectangle.Intersect(fullCollider, tileBounds);
                        if (!collider.IsEmpty) neighbor.ColliderList.Add(collider);
                    }

                    tileBounds = new Rectangle(tileX * TILE_SIZE, tileY * TILE_SIZE, TILE_SIZE, TILE_SIZE);
                    collider = Rectangle.Intersect(fullCollider, tileBounds);
                    if (!collider.IsEmpty) colliderList.Add(collider);
                }
            }
        }
       
        public void Update(GameTime gameTime)
        {
            foreach (TileSprite tileLayer in tileSpriteList) tileLayer.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            foreach (TileSprite tileLayer in tileSpriteList) tileLayer.Draw(spriteBatch, camera);
        }

        public void DrawDepth(SpriteBatch spriteBatch, Camera camera)
        {
            foreach (TileSprite tileLayer in tileSpriteList) tileLayer.DrawDepth(spriteBatch, camera);
        }

        public bool AccessibleFrom(Tile originTile, List<Rectangle> entityColliders)
        {
            List<Rectangle> potentialColliders = new List<Rectangle>();
            potentialColliders.AddRange(colliderList);
            potentialColliders.AddRange(originTile.ColliderList);
            if (entityColliders != null) potentialColliders.AddRange(entityColliders);

            Rectangle startRectangle = new Rectangle((int)this.Center.X - 3, (int)this.Center.Y - 3, 6, 6);
            Rectangle endRectangle = new Rectangle((int)originTile.Center.X - 3, (int)originTile.Center.Y - 3, 6, 6);
            Rectangle pathRectangle = Rectangle.Union(startRectangle, endRectangle);

            foreach (Rectangle collider in potentialColliders)
            {
                if (collider.Intersects(pathRectangle)) return false;
            }

            return true;
        }

        public int TileX { get => tileX; }
        public int TileY { get => tileY; }
        public Vector2 Center { get => center; }
        public List<Tile> NeighborList { get => neighborList; }
        public List<Rectangle> ColliderList { get => colliderList; }
        public bool BlocksSight { set => blocksSight = value; get => blocksSight; }
        public bool HiddenToPlayer { set => visibleToPlayer = value; get => visibleToPlayer; }
    }
}
