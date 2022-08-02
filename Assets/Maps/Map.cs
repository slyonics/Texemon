using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Panderling.GameObjects.Actors;
using Panderling.Main;
using Panderling.Procedural;

using TiledSharp;

namespace Panderling.GameObjects.Maps
{
    public class EntranceZone
    {
        public Rectangle zone;
        public string mapName;
        public Orientation orientation;

        public EntranceZone(Rectangle initialZone, string initialMapName, Orientation initialOrientation)
        {
            zone = initialZone;
            mapName = initialMapName;
            orientation = initialOrientation;
        }
    }

    public class ExitZone
    {
        public Rectangle zone;
        public string mapName;

        public ExitZone(Rectangle initialZone, string initialMapName)
        {
            zone = initialZone;
            mapName = initialMapName;
        }
    }

    public class Map
    {
        /// <summary>
        /// Immutable class for holding coordinate transform constants.  Bulkier than a 2D
        /// array of ints, but it's self-formatting if you want to log it while debugging.
        /// </summary>
        private class OctantTransform
        {
            public int xx { get; private set; }
            public int xy { get; private set; }
            public int yx { get; private set; }
            public int yy { get; private set; }

            public OctantTransform(int xx, int xy, int yx, int yy)
            {
                this.xx = xx;
                this.xy = xy;
                this.yx = yx;
                this.yy = yy;
            }

            public override string ToString()
            {
                // consider formatting in constructor to reduce garbage
                return string.Format("[OctantTransform {0,2:D} {1,2:D} {2,2:D} {3,2:D}]",
                    xx, xy, yx, yy);
            }
        }

        private static OctantTransform[] s_octantTransform =
        {
            new OctantTransform( 1,  0,  0,  1 ),   // 0 E-NE
            new OctantTransform( 0,  1,  1,  0 ),   // 1 NE-N
            new OctantTransform( 0, -1,  1,  0 ),   // 2 N-NW
            new OctantTransform(-1,  0,  0,  1 ),   // 3 NW-W
            new OctantTransform(-1,  0,  0, -1 ),   // 4 W-SW
            new OctantTransform( 0, -1, -1,  0 ),   // 5 SW-S
            new OctantTransform( 0,  1, -1,  0 ),   // 6 S-SE
            new OctantTransform( 1,  0,  0, -1 ),   // 7 SE-E
        };

        List<int> visibleOctants = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };
        List<Tile> visibleTiles = new List<Tile>();

        private Tile[,] tiles;
        private NavNode[,] navMesh;

        private Weather weather;
        private List<Light> lightList = new List<Light>();

        private List<EntranceZone> entranceList = new List<EntranceZone>();
        private List<ExitZone> exitList = new List<ExitZone>();

        private bool peaceful;

        public Map(int width, int height)
        {
            tiles = new Tile[width, height];
        }

        public void AddTilesFromTmx(TmxMap tmxMap, string mapName, int insertX, int insertY)
        {
            int i = 0;
            for (int y = insertY; y < insertY + tmxMap.Height; y++)
            {
                for (int x = insertX; x < insertX + tmxMap.Width; x++)
                {
                    tiles[x, y] = new Tile(this, tmxMap, x, y, i);
                    i++;
                }
            }

            if (tmxMap.Properties.ContainsKey("Peaceful")) peaceful = bool.Parse(tmxMap.Properties["Peaceful"]);

            foreach (TmxObject tmxObject in tmxMap.ObjectGroups["Travel Zones"].Objects)
            {
                Rectangle zone = new Rectangle((int)tmxObject.X + insertX * Tile.TILE_SIZE, (int)tmxObject.Y + insertY * Tile.TILE_SIZE, (int)tmxObject.Width, (int)tmxObject.Height);
                Orientation orientation = (Orientation)Enum.Parse(typeof(Orientation), tmxObject.Properties["Orientation"]);

                if (tmxObject.Name == "Start") entranceList.Add(new EntranceZone(zone, tmxObject.Name, orientation));
                else
                {
                    Rectangle entranceZone = zone;
                    switch (orientation)
                    {
                        case Orientation.Up: entranceZone.Y -= entranceZone.Height; break;
                        case Orientation.Right: entranceZone.X += entranceZone.Width; break;
                        case Orientation.Down: entranceZone.Y += entranceZone.Height; break;
                        case Orientation.Left: entranceZone.X -= entranceZone.Width; break;
                    }

                    string entranceName = tmxObject.Name;
                    string exitName = tmxObject.Name;
                    switch (tmxObject.Name)
                    {
                        case "NextFloor":
                            entranceName = exitName = mapName.Split('-')[0] + "-" + (int.Parse(mapName.Split('-')[1]) + 1);
                            break;

                        case "PreviousFloor":
                            entranceName = exitName = mapName.Split('-')[0] + "-" + (int.Parse(mapName.Split('-')[1]) - 1);
                            break;
                    }

                    entranceList.Add(new EntranceZone(entranceZone, entranceName, orientation));
                    exitList.Add(new ExitZone(zone, exitName));
                }                
            }
        }

        public void FinishMap(int seed)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    if (tiles[x, y] == null) tiles[x, y] = new Tile(this, null, x, y, -1);
                }
            }

            Rng.Seed(seed);

            for (int y = MapHeight - 1; y >= 0; y--)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    tiles[x, y].AssignNeighbors();
                    tiles[x, y].AssignLayers();
                }
            }

            Rng.Seed();

            BuildNavMesh();
        }

        private void BuildNavMesh()
        {
            navMesh = new NavNode[MapWidth * 2 - 1, MapHeight * 2 - 1];
            for (int y = 0; y < MapHeight * 2 - 1; y++)
            {
                for (int x = 0; x < MapWidth * 2 - 1; x++)
                {
                    navMesh[x, y] = new NavNode(this, x, y);
                }
            }

            for (int y = 0; y < MapHeight * 2 - 1; y++)
            {
                for (int x = 0; x < MapWidth * 2 - 1; x++)
                {
                    navMesh[x, y].AssignNeighbors(this);
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            weather.Update(gameTime);

            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    tiles[x, y].Update(gameTime);
                    tiles[x, y].HiddenToPlayer = true;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            int startTileX = Math.Max((int)(camera.View.Left / Tile.TILE_SIZE) - 1, 0);
            int startTileY = Math.Max((int)(camera.View.Top / Tile.TILE_SIZE - 1), 0);
            int endTileX = Math.Min((int)(camera.View.Right / Tile.TILE_SIZE), MapWidth - 1);
            int endTileY = Math.Min((int)(camera.View.Bottom / Tile.TILE_SIZE), MapHeight - 1);

            for (int y = startTileY; y <= endTileY; y++)
            {
                for (int x = startTileX; x <= endTileX; x++)
                {
                    tiles[x, y].Draw(spriteBatch, camera);
                }
            }
        }

        public void DrawDepth(SpriteBatch spriteBatch, Camera camera)
        {
            int startTileX = Math.Max((int)(camera.View.Left / Tile.TILE_SIZE) - 1, 0);
            int startTileY = Math.Max((int)(camera.View.Top / Tile.TILE_SIZE - 1), 0);
            int endTileX = Math.Min((int)(camera.View.Right / Tile.TILE_SIZE), MapWidth - 1);
            int endTileY = Math.Min((int)(camera.View.Bottom / Tile.TILE_SIZE), MapHeight - 1);

            for (int y = startTileY; y <= endTileY; y++)
            {
                for (int x = startTileX; x <= endTileX; x++)
                {
                    tiles[x, y].DrawDepth(spriteBatch, camera);
                }
            }
        }

        public Tile GetTile(Vector2 position)
        {
            int tileX = (int)(position.X / Tile.TILE_SIZE);
            int tileY = (int)(position.Y / Tile.TILE_SIZE);

            if (tileX < 0 || tileY < 0 || tileX >= MapWidth || tileY >= MapHeight) return null;

            return tiles[tileX, tileY];
        }

        public Tile GetTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= MapWidth || y >= MapHeight) return null;

            return tiles[x, y];
        }

        public Tile GetClearTileNear(Vector2 position)
        {
            return GetClearTileNear((int)(position.X / Tile.TILE_SIZE), (int)(position.Y / Tile.TILE_SIZE));
        }

        public Tile GetClearTileNear(int x, int y)
        {
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x >= MapWidth) x = MapWidth - 1;
            if (y >= MapHeight) y = MapHeight - 1;

            Queue<Tile> unprocessedTiles = new Queue<Tile>();
            List<Tile> processedTiles = new List<Tile>();
            unprocessedTiles.Enqueue(GetTile(x, y));

            while (unprocessedTiles.Count > 0)
            {
                Tile candidateTile = unprocessedTiles.Dequeue();
                if (candidateTile.ColliderList.Count == 0) return candidateTile;

                processedTiles.Add(candidateTile);
                foreach (Tile tile in candidateTile.NeighborList)
                {
                    if (!processedTiles.Contains(tile) && !unprocessedTiles.Contains(tile)) unprocessedTiles.Enqueue(tile);
                }
            }

            return null;
        }

        public NavNode GetNavNode(Vector2 position)
        {
            int nodeX = (int)(position.X / NavNode.NODE_SIZE) - 1;
            int nodeY = (int)(position.Y / NavNode.NODE_SIZE) - 1;
            if (nodeX < 0) nodeX = 0;
            if (nodeY < 0) nodeY = 0;
            if (nodeX > MapWidth * 2 - 2) nodeX = MapWidth * 2 - 2;
            if (nodeY > MapHeight * 2 - 2) nodeY = MapHeight * 2 - 2;

            return navMesh[nodeX, nodeY];
        }

        public NavNode GetNavNode(Actor actor)
        {
            int nodeX = (int)(actor.Center.X / NavNode.NODE_SIZE) - 1;
            int nodeY = (int)(actor.Bounds.Bottom / NavNode.NODE_SIZE) - 1;
            if (nodeX < 0) nodeX = 0;
            if (nodeY < 0) nodeY = 0;
            if (nodeX > MapWidth * 2 - 2) nodeX = MapWidth * 2 - 2;
            if (nodeY > MapHeight * 2 - 2) nodeY = MapHeight * 2 - 2;

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
            if (targetNode.FitsActor(seeker)) return targetNode;

            List<NavNode> nodeList = new List<NavNode>();
            nodeList.Add(targetNode);
            nodeList.AddRange(targetNode.NeighborList);

            IOrderedEnumerable<NavNode> sortedNodes = nodeList.OrderBy(x => Vector2.Distance(x.Center, new Vector2(seeker.Center.X, seeker.Bounds.Bottom)));
            return sortedNodes.FirstOrDefault(x => x.FitsActor(seeker));
        }

        public NavNode GetNavNode(int x, int y)
        {
            if (x < 0 || y < 0 || x >= MapWidth * 2 - 1 || y >= MapHeight * 2 - 1) return null;

            return navMesh[x, y];
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

        public void CalculateFieldOfView(Tile sourceTile, float viewRadius)
        {
            sourceTile.HiddenToPlayer = false;
            for (int txidx = 0; txidx < s_octantTransform.Length; txidx++)
            {
                CastLight(sourceTile, viewRadius, 1, 1.0f, 0.0f, s_octantTransform[txidx]);
            }
        }

        /// <summary>
        /// Recursively casts light into cells.  Operates on a single octant.
        /// Adapted from source code at http://www.roguebasin.com/index.php?title=FOV_using_recursive_shadowcasting by Fadden
        /// </summary>
        /// <param name="sourceTile">The player's current tile after moving.</param>
        /// <param name="viewRadius">The view radius; can be a fractional value.</param>
        /// <param name="startColumn">Current column; pass 1 as initial value.</param>
        /// <param name="leftViewSlope">Slope of the left (upper) view edge; pass 1.0 as
        ///   the initial value.</param>
        /// <param name="rightViewSlope">Slope of the right (lower) view edge; pass 0.0 as
        ///   the initial value.</param>
        /// <param name="txfrm">Coordinate multipliers for the octant transform.</param>
        ///
        /// Maximum recursion depth is (Ceiling(viewRadius)).
        private void CastLight(Tile sourceTile, float viewRadius, int startColumn, float leftViewSlope, float rightViewSlope, OctantTransform txfrm)
        {
            // Used for distance test.
            float viewRadiusSquared = viewRadius * viewRadius;
            int viewCeiling = (int)Math.Ceiling(viewRadius);

            // Set true if the previous cell we encountered was blocked.
            bool prevWasBlocked = false;

            // As an optimization, when scanning past a block we keep track of the
            // rightmost corner (bottom-right) of the last one seen.  If the next cell
            // is empty, we can use this instead of having to compute the top-right corner
            // of the empty cell.
            float savedRightSlope = -1;

            // Outer loop: walk across each column, stopping when we reach the visibility limit.
            for (int currentCol = startColumn; currentCol <= viewCeiling; currentCol++)
            {
                int xc = currentCol;

                // Inner loop: walk down the current column.  We start at the top, where X==Y.
                //
                // TODO: we waste time walking across the entire column when the view area
                //   is narrow.  Experiment with computing the possible range of cells from
                //   the slopes, and iterate over that instead.
                for (int yc = currentCol; yc >= 0; yc--)
                {
                    // Translate local coordinates to grid coordinates.  For the various octants
                    // we need to invert one or both values, or swap X for Y.
                    int gridX = sourceTile.TileX + xc * txfrm.xx + yc * txfrm.xy;
                    int gridY = sourceTile.TileY + xc * txfrm.yx + yc * txfrm.yy;

                    // Range-check the values.  This lets us avoid the slope division for blocks
                    // that are outside the grid.
                    //
                    // Note that, while we will stop at a solid column of blocks, we do always
                    // start at the top of the column, which may be outside the grid if we're (say)
                    // checking the first octant while positioned at the north edge of the map.
                    if (gridX < 0 || gridX >= MapWidth || gridY < 0 || gridY >= MapHeight)
                    {
                        continue;
                    }

                    // Compute slopes to corners of current block.  We use the top-left and
                    // bottom-right corners.  If we were iterating through a quadrant, rather than
                    // an octant, we'd need to flip the corners we used when we hit the midpoint.
                    //
                    // Note these values will be outside the view angles for the blocks at the
                    // ends -- left value > 1, right value < 0.
                    float leftBlockSlope = (yc + 0.5f) / (xc - 0.5f);
                    float rightBlockSlope = (yc - 0.5f) / (xc + 0.5f);

                    // Check to see if the block is outside our view area.  Note that we allow
                    // a "corner hit" to make the block visible.  Changing the tests to >= / <=
                    // will reduce the number of cells visible through a corner (from a 3-wide
                    // swath to a single diagonal line), and affect how far you can see past a block
                    // as you approach it.  This is mostly a matter of personal preference.
                    if (rightBlockSlope > leftViewSlope)
                    {
                        // Block is above the left edge of our view area; skip.
                        continue;
                    }
                    else if (leftBlockSlope < rightViewSlope)
                    {
                        // Block is below the right edge of our view area; we're done.
                        break;
                    }

                    // This cell is visible, given infinite vision range.  If it's also within
                    // our finite vision range, light it up.
                    //
                    // To avoid having a single lit cell poking out N/S/E/W, use a fractional
                    // viewRadius, e.g. 8.5.
                    //
                    // TODO: we're testing the middle of the cell for visibility.  If we tested
                    //  the bottom-left corner, we could say definitively that no part of the
                    //  cell is visible, and reduce the view area as if it were a wall.  This
                    //  could reduce iteration at the corners.
                    float distanceSquared = xc * xc + yc * yc;
                    if (distanceSquared <= viewRadiusSquared)
                    {
                        tiles[gridX, gridY].HiddenToPlayer = false;
                        visibleTiles.Add(tiles[gridX, gridY]);
                    }

                    bool curBlocked = tiles[gridX, gridY].BlocksSight;

                    if (prevWasBlocked)
                    {
                        if (curBlocked)
                        {
                            // Still traversing a column of walls.
                            savedRightSlope = rightBlockSlope;
                        }
                        else
                        {
                            // Found the end of the column of walls.  Set the left edge of our
                            // view area to the right corner of the last wall we saw.
                            prevWasBlocked = false;
                            leftViewSlope = savedRightSlope;
                        }
                    }
                    else
                    {
                        if (curBlocked)
                        {
                            // Found a wall.  Split the view area, recursively pursuing the
                            // part to the left.  The leftmost corner of the wall we just found
                            // becomes the right boundary of the view area.
                            //
                            // If this is the first block in the column, the slope of the top-left
                            // corner will be greater than the initial view slope (1.0).  Handle
                            // that here.
                            if (leftBlockSlope <= leftViewSlope)
                            {
                                CastLight(sourceTile, viewRadius, currentCol + 1, leftViewSlope, leftBlockSlope, txfrm);
                            }

                            // Once that's done, we keep searching to the right (down the column),
                            // looking for another opening.
                            prevWasBlocked = true;
                            savedRightSlope = rightBlockSlope;
                        }
                    }
                }

                // Open areas are handled recursively, with the function continuing to search to
                // the right (down the column).  If we reach the bottom of the column without
                // finding an open cell, then the area defined by our view area is completely
                // obstructed, and we can stop working.
                if (prevWasBlocked)
                {
                    break;
                }
            }
        }

        public EntranceZone GetEntrance(string name)
        {
            return entranceList.FirstOrDefault(x => x.mapName == name);
        }

        public string GetExit(List<Player> playerList)
        {
            foreach (ExitZone exitZone in exitList)
            {
                if (playerList.Exists(x => exitZone.zone.Contains(x.Position))) return exitZone.mapName;
            }

            return "";
        }

        public int MapWidth { get => tiles.GetLength(0); }
        public int MapHeight { get => tiles.GetLength(1); }
        public List<Light> LightList { get => lightList; }
        public Weather Weather { set => weather = value; get => weather; }
        public bool Peaceful { get => peaceful; }
    }
}
