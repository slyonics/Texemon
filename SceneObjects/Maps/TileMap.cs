using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using TiledCS;

namespace Texemon.SceneObjects.Maps
{
    public class Tilemap : Entity
    {
        private class Tileset
        {
            public Tileset(TiledTileset iTiledTileset)
            {
                TiledTileset = iTiledTileset;
                SpriteAtlas = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Tiles_" + Path.GetFileNameWithoutExtension(TiledTileset.Image.source))];
            }

            public TiledTileset TiledTileset { get; private set; }
            public Texture2D SpriteAtlas { get; private set; }
        }

        private GameMap gameMap;
        private TiledMap tiledMap;
        private Dictionary<int, Tileset> tilesets = new Dictionary<int, Tileset>();

        private Tile[,] tiles;

        private NavNode[,] navMesh;

        public string Name { get => gameMap.ToString(); }

        public Tilemap(Scene iScene, GameMap iGameMap)
            : base(iScene, Vector2.Zero)
        {
            gameMap = iGameMap;

            tiledMap = new TiledMap();
            tiledMap.ParseXml(AssetCache.MAPS[gameMap]);
            foreach (TiledMapTileset tiledMapTileset in tiledMap.Tilesets)
            {
                TiledTileset tiledTileset = new TiledTileset();
                tiledTileset.ParseXml(AssetCache.MAPS[(GameMap)Enum.Parse(typeof(GameMap), "Tilesets_" + Path.GetFileNameWithoutExtension(tiledMapTileset.source))]);
                tilesets.Add(tiledMapTileset.firstgid, new Tileset(tiledTileset));
            }

            tiles = new Tile[Columns, Rows];
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    tiles[x, y] = new Tile(this, x, y);
                }
            }

            foreach (TiledGroup tiledGroup in tiledMap.Groups) LoadLayers(tiledGroup.layers, tiledGroup);
            LoadLayers(tiledMap.Layers, null);

            for (int y = Rows - 1; y >= 0; y--)
            {
                for (int x = 0; x < Columns; x++)
                {
                    tiles[x, y].AssignNeighbors();
                }
            }

            BuildNavMesh();
        }

        protected virtual void LoadLayers(TiledLayer[] tiledLayers, TiledGroup tiledGroup)
        {
            foreach (TiledLayer tiledLayer in tiledLayers)
            {
                switch (tiledLayer.type)
                {
                    case TiledLayerType.TileLayer: LoadTileLayer(tiledLayer, tiledGroup); break;
                    case TiledLayerType.ObjectLayer: LoadObjectLayer(tiledLayer, tiledGroup); break;
                    case TiledLayerType.ImageLayer: LoadImageLayer(tiledLayer, tiledGroup); break;
                }
            }
        }

        protected virtual void LoadTileLayer(TiledLayer tiledLayer, TiledGroup tiledGroup)
        {
            if (tiledGroup == null || tiledGroup.name == "Background")
            {
                int i = 0;
                for (int y = 0; y < Rows; y++)
                {
                    for (int x = 0; x < Columns; x++, i++)
                    {
                        int tileId = tiledLayer.data[i];
                        if (tileId == 0) continue;

                        TiledMapTileset tiledMapTileset = tiledMap.GetTiledMapTileset(tileId);
                        Tileset tileset = tilesets[tiledMapTileset.firstgid];
                        TiledTileset tiledTileset = tileset.TiledTileset;
                        TiledTile tilesetTile = tiledMap.GetTiledTile(tiledMapTileset, tiledTileset, tileId);
                        TiledSourceRect spriteSource = tiledMap.GetSourceRect(tiledMapTileset, tiledTileset, tileId);

                        tiles[x, y].ApplyBackgroundTile(tilesetTile, tiledLayer, new Rectangle(spriteSource.x, spriteSource.y, spriteSource.width, spriteSource.height), tileset.SpriteAtlas);
                    }
                }
            }
            else if (tiledGroup.name.Contains("Height"))
            {
                int height = int.Parse(tiledGroup.name.Remove(0, 6));

                int i = 0;
                for (int y = 0; y < Rows; y++)
                {
                    for (int x = 0; x < Columns; x++, i++)
                    {
                        int tileId = tiledLayer.data[i];
                        if (tileId == 0) continue;

                        TiledMapTileset tiledMapTileset = tiledMap.GetTiledMapTileset(tileId);
                        Tileset tileset = tilesets[tiledMapTileset.firstgid];
                        TiledTileset tiledTileset = tileset.TiledTileset;
                        TiledTile tilesetTile = tiledMap.GetTiledTile(tiledMapTileset, tiledTileset, tileId);
                        TiledSourceRect spriteSource = tiledMap.GetSourceRect(tiledMapTileset, tiledTileset, tileId);

                        tiles[x, y].ApplyEntityTile(tilesetTile, tiledLayer, new Rectangle(spriteSource.x, spriteSource.y, spriteSource.width, spriteSource.height), tileset.SpriteAtlas, height);
                    }
                }
            }
        }

        protected virtual void LoadObjectLayer(TiledLayer tiledLayer, TiledGroup tiledGroup)
        {
            ObjectData.Add(new Tuple<TiledLayer, TiledGroup>(tiledLayer, tiledGroup));
        }

        protected virtual void LoadImageLayer(TiledLayer tiledLayer, TiledGroup tiledGroup)
        {

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    tiles[x, y].Update(gameTime);
                }
            }
        }

        public void DrawBackground(SpriteBatch spriteBatch, Camera camera)
        {
            int startTileX = Math.Max((int)(camera.View.Left / tiledMap.TileWidth) - 1, 0);
            int startTileY = Math.Max((int)(camera.View.Top / tiledMap.TileHeight) - 1, 0);
            int endTileX = Math.Min((int)(camera.View.Right / tiledMap.TileWidth), Columns - 1);
            int endTileY = Math.Min((int)(camera.View.Bottom / tiledMap.TileHeight), Rows - 1);

            for (int y = startTileY; y <= endTileY; y++)
            {
                for (int x = startTileX; x <= endTileX; x++)
                {
                    tiles[x, y].DrawBackground(spriteBatch, camera);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            int startTileX = Math.Max((int)(camera.View.Left / tiledMap.TileWidth) - 1, 0);
            int startTileY = Math.Max((int)(camera.View.Top / tiledMap.TileHeight) - 1, 0);
            int endTileX = Math.Min((int)(camera.View.Right / tiledMap.TileWidth), Columns - 1);
            int endTileY = Math.Min((int)(camera.View.Bottom / tiledMap.TileHeight), Rows - 1);

            for (int y = startTileY; y <= endTileY; y++)
            {
                for (int x = startTileX; x <= endTileX; x++)
                {
                    tiles[x, y].Draw(spriteBatch, camera);
                }
            }
        }

        public override void DrawShader(SpriteBatch spriteBatch, Camera camera, Matrix matrix)
        {
            
        }

        public Tile GetTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Columns || y >= Rows) return null;
            return tiles[x, y];
        }

        public Tile GetTile(Vector2 position)
        {
            int tileX = (int)(position.X / TileWidth);
            int tileY = (int)(position.Y / TileHeight);

            if (tileX < 0 || tileY < 0 || tileX >= Columns || tileY >= Rows) return null;

            return tiles[tileX, tileY];
        }

        public NavNode GetNavNode(Vector2 position)
        {
            int nodeX = (int)(position.X / NavNode.NODE_SIZE) - 1;
            int nodeY = (int)(position.Y / NavNode.NODE_SIZE) - 1;
            if (nodeX < 0) nodeX = 0;
            if (nodeY < 0) nodeY = 0;
            if (nodeX > Columns * 2 - 2) nodeX = Columns * 2 - 2;
            if (nodeY > Rows * 2 - 2) nodeY = Rows * 2 - 2;

            return navMesh[nodeX, nodeY];
        }

        public NavNode GetNavNode(Actor actor)
        {
            int nodeX = (int)(actor.Center.X / NavNode.NODE_SIZE) - 1;
            int nodeY = (int)(actor.Bounds.Bottom / NavNode.NODE_SIZE) - 1;
            if (nodeX < 0) nodeX = 0;
            if (nodeY < 0) nodeY = 0;
            if (nodeX > Columns * 2 - 2) nodeX = Columns * 2 - 2;
            if (nodeY > Rows * 2 - 2) nodeY = Rows * 2 - 2;

            NavNode closestNode = navMesh[nodeX, nodeY];

            List<NavNode> nodeList = new List<NavNode>();
            nodeList.Add(closestNode);
            nodeList.AddRange(closestNode.NeighborList);

            IOrderedEnumerable<NavNode> sortedNodes = nodeList.OrderBy(x => Vector2.Distance(x.Center, new Vector2(actor.Center.X, actor.Bounds.Bottom)));
            return sortedNodes.FirstOrDefault(x => x.AccessibleFromActor(actor));
        }

        public NavNode GetNavNode(Actor seeker, Actor target)
        {
            NavNode targetNode = GetNavNode(target);
            if (targetNode == null) return null;
            if (targetNode.FitsActor(seeker)) return targetNode;

            List<NavNode> nodeList = new List<NavNode>();
            nodeList.Add(targetNode);
            nodeList.AddRange(targetNode.NeighborList);

            IOrderedEnumerable<NavNode> sortedNodes = nodeList.OrderBy(x => Vector2.Distance(x.Center, new Vector2(seeker.Center.X, seeker.Bounds.Bottom)));
            return sortedNodes.FirstOrDefault(x => x.FitsActor(seeker));
        }

        public NavNode GetNavNode(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Columns * 2 - 1 || y >= Rows * 2 - 1) return null;

            return navMesh[x, y];
        }

        private void BuildNavMesh()
        {
            navMesh = new NavNode[Columns * 2 - 1, Rows * 2 - 1];
            for (int y = 0; y < Rows * 2 - 1; y++)
            {
                for (int x = 0; x < Columns * 2 - 1; x++)
                {
                    navMesh[x, y] = new NavNode(this, x, y);
                }
            }

            for (int y = 0; y < Rows * 2 - 1; y++)
            {
                for (int x = 0; x < Columns * 2 - 1; x++)
                {
                    navMesh[x, y].AssignNeighbors(this);
                }
            }
        }

        public List<NavNode> GetPath(NavNode startNode, NavNode endNode, Actor actor, int searchLimit)
        {
            List<NavNode> processedNodes = new List<NavNode>();
            List<NavNode> unprocessedNodes = new List<NavNode> { startNode };
            Dictionary<NavNode, NavNode> cameFrom = new Dictionary<NavNode, NavNode>();
            Dictionary<NavNode, int> currentDistance = new Dictionary<NavNode, int>();
            Dictionary<NavNode, int> predictedDistance = new Dictionary<NavNode, int>();

            int searchCount = 0;

            currentDistance.Add(startNode, 0);
            predictedDistance.Add(startNode, (int)Vector2.Distance(startNode.Center, endNode.Center));

            while (unprocessedNodes.Count > 0 && searchCount < searchLimit)
            {
                searchCount++;

                // get the node with the lowest estimated cost to finish
                NavNode current = (from p in unprocessedNodes orderby predictedDistance[p] ascending select p).First();

                // if it is the finish, return the path
                if (current == endNode)
                {
                    // generate the found path
                    return ReconstructPath(cameFrom, endNode);
                }

                // move current node from open to closed
                unprocessedNodes.Remove(current);
                processedNodes.Add(current);

                foreach (NavNode neighbor in current.NeighborList)
                {
                    if (neighbor.AccessibleFromNode(current, actor))
                    {
                        int tempCurrentDistance = currentDistance[current] + (int)Vector2.Distance(current.Center, neighbor.Center);

                        // if we already know a faster way to this neighbor, use that route and ignore this one
                        if (currentDistance.ContainsKey(neighbor) && tempCurrentDistance >= currentDistance[neighbor]) continue;

                        // if we don't know a route to this neighbor, or if this is faster, store this route
                        if (!processedNodes.Contains(neighbor) || tempCurrentDistance < currentDistance[neighbor])
                        {
                            if (cameFrom.Keys.Contains(neighbor)) cameFrom[neighbor] = current;
                            else cameFrom.Add(neighbor, current);

                            currentDistance[neighbor] = tempCurrentDistance;
                            predictedDistance[neighbor] = currentDistance[neighbor] + (int)Vector2.Distance(neighbor.Center, endNode.Center);

                            if (!unprocessedNodes.Contains(neighbor)) unprocessedNodes.Add(neighbor);
                        }
                    }
                }
            }

            return null;
        }

        private static List<NavNode> ReconstructPath(Dictionary<NavNode, NavNode> cameFrom, NavNode current)
        {
            if (!cameFrom.Keys.Contains(current))
            {
                return new List<NavNode> { current };
            }

            List<NavNode> path = ReconstructPath(cameFrom, cameFrom[current]);
            path.Add(current);
            return path;
        }

        public TiledMap MapData { get => tiledMap; }
        public List<Tuple<TiledLayer, TiledGroup>> ObjectData { get; } = new List<Tuple<TiledLayer, TiledGroup>>();

        public int TileWidth { get => tiledMap.TileWidth; }
        public int TileHeight { get => tiledMap.TileHeight; }
        public int Width { get => tiledMap.Width * TileWidth; }
        public int Height { get => tiledMap.Height * TileHeight; }
        public int Columns { get => tiledMap.Width; }
        public int Rows { get => tiledMap.Height; }        
    }
}
