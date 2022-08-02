using System.IO;
using System.Collections.Generic;
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Panderling.Main;
using Panderling.Procedural;

using TiledSharp;

namespace Panderling.GameObjects.Maps
{
    public class TileSprite
    {
        public enum LayerType
        {
            Background,
            Obstacle,
            TallObstacle,
            Foreground
        }

        private const int DEFAULT_FRAME_LENGTH = 200;

        private static Dictionary<string, Texture2D> tilesetSprites = new Dictionary<string, Texture2D>();
        private static Dictionary<string, Rectangle[]> tilesetSources = new Dictionary<string, Rectangle[]>();

        private Tile parent;
        private LayerType layerType;
        private Texture2D sprite;
        private Rectangle[] sources;
        private Rectangle source;
        private Rectangle destination;
        private int cell;
        private int positionZ;
        private byte upperByteZ;
        private byte lowerByteZ;
        private float depth;
        private int height = -1;

        private Light light;
        private bool lightFlicker;

        private int frame;
        private int frameCount;
        private int[] frameOffset;
        private int frameLength;
        private int animationTime;

        private TmxTilesetTile tmxTilesetTile;

        public TileSprite(TmxLayer tmxLayer, TmxLayerTile tmxLayerTile, TmxTileset tmxTileset, Tile iParent, int x, int y, float iDepth, List<Light> lightList, LayerType iLayerType, int layerHeight)
        {
            string propertyString;

            parent = iParent;
            layerType = iLayerType;
            sprite = tilesetSprites[tmxTileset.Name];
            sources = tilesetSources[tmxTileset.Name];
            cell = tmxLayerTile.Gid - tmxTileset.FirstGid;
            source = sources[cell];
            destination = new Rectangle(x * Tile.TILE_SIZE + (int)tmxLayer.OffsetX, y * Tile.TILE_SIZE + (int)tmxLayer.OffsetY, Tile.TILE_SIZE, Tile.TILE_SIZE);
            frame = 0;

            depth = iDepth;

            height = layerHeight;
            tmxTileset.Tiles.TryGetValue(cell, out tmxTilesetTile);
            if (tmxTilesetTile != null)
            {
                if (tmxTilesetTile.Properties.TryGetValue("Height", out propertyString))
                {
                    height = int.Parse(propertyString);
                }

                if (tmxTilesetTile.Properties.TryGetValue("BlockSight", out propertyString)) parent.BlocksSight = true;

                if (tmxTilesetTile.Properties.TryGetValue("Random", out propertyString))
                {
                    string[] randomTokens = propertyString.Split(',');
                    string randomRange = randomTokens[(int)(Rng.GaussianDouble(1000000, 20) % randomTokens.Length)];
                    string[] rangeTokens = randomRange.Split('-');

                    cell = Rng.RandomInt(int.Parse(rangeTokens[0]), int.Parse(rangeTokens[1]));
                    source = sources[cell];
                }

                if (height != -1)
                {
                    if (layerType == LayerType.Obstacle) layerType = LayerType.TallObstacle;
                    if (height > 0 && height < Tile.TILE_SIZE / 2) height *= Tile.TILE_SIZE;
                }

                ParseLights(tmxTilesetTile, lightList);
                ParseAnimation(tmxTilesetTile);
            }

            SetDepthOrder();
        }

        private void ParseLights(TmxTilesetTile tmxTilesetTile, List<Light> lightList)
        {
            bool lightColorExists = tmxTilesetTile.Properties.ContainsKey("LightColor");
            bool lightIntensityExists = tmxTilesetTile.Properties.ContainsKey("LightIntensity");
            bool lightFlickerExists = tmxTilesetTile.Properties.ContainsKey("LightFlicker");

            if (!lightColorExists && !lightIntensityExists && !lightFlickerExists) return;

            int heightOffset = (height == -1) ? 0 : height * Tile.TILE_SIZE;
            light = new Light(new Vector2(destination.Center.X, destination.Bottom), destination.Bottom + heightOffset);

            if (lightIntensityExists)
            {
                light.Intensity = float.Parse(tmxTilesetTile.Properties["LightIntensity"]);
            }

            if (lightColorExists)
            {
                System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(tmxTilesetTile.Properties["LightColor"]);
                light.Color = new Color(color.R, color.G, color.B);
            }

            if (lightFlickerExists)
            {
                string[] tokens = tmxTilesetTile.Properties["LightFlicker"].Split(',');
                light.SetFlicker(float.Parse(tokens[0]), float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3]));
                lightFlicker = true;
            }
            
            lightList.Add(light);
        }

        private void ParseAnimation(TmxTilesetTile tmxTilesetTile)
        {
            string propertyString;
            if (tmxTilesetTile.Properties.TryGetValue("FrameOffsets", out propertyString))
            {
                string[] tokens = propertyString.Split(',');
                frameCount = tokens.Length;
                frameOffset = new int[frameCount];
                for (int i = 0; i < frameCount; i++)
                {
                    frameOffset[i] = int.Parse(tokens[i]);
                }

                if (tmxTilesetTile.Properties.TryGetValue("FrameLength", out propertyString)) frameLength = int.Parse(propertyString);
                else frameLength = DEFAULT_FRAME_LENGTH;
            }

            animationTime = frameLength;
        }

        private void SetDepthOrder()
        {
            switch (layerType)
            {
                case LayerType.Background:
                    if (height != -1)
                    {
                        positionZ = destination.Bottom + height;
                        upperByteZ = (byte)(positionZ >> 8);
                        lowerByteZ = (byte)positionZ;
                    }
                    break;

                case LayerType.Obstacle:
                    if (height == -1) positionZ = destination.Bottom;
                    else positionZ = destination.Bottom + height;
                    break;

                case LayerType.TallObstacle:
                    positionZ = destination.Bottom + height;
                    upperByteZ = (byte)(positionZ >> 8);
                    lowerByteZ = (byte)positionZ;
                    break;

                case LayerType.Foreground:
                    upperByteZ = 255;
                    lowerByteZ = 255;
                    break;
            }
        }

        public static void LoadContent(ContentManager contentManager)
        {
            string[] textures = Directory.GetFiles("Content\\Graphics\\Tiles");
            foreach (string textureName in textures)
            {
                string[] textureNameTokens = textureName.Split(new char[] { '.', '\\' });
                string tilesetName = textureNameTokens[textureNameTokens.Length - 2];

                Texture2D sprite = contentManager.Load<Texture2D>("Graphics//Tiles//" + tilesetName);
                int tileColumns = sprite.Width / Tile.TILE_SIZE;
                int tileRows = sprite.Height / Tile.TILE_SIZE;
                Rectangle[] sources = new Rectangle[tileColumns * tileRows];
                for (int y = 0; y < tileRows; y++)
                {
                    for (int x = 0; x < tileColumns; x++)
                    {
                        sources[x + y * tileColumns] = new Rectangle(x * Tile.TILE_SIZE, y * Tile.TILE_SIZE, Tile.TILE_SIZE, Tile.TILE_SIZE);
                    }
                }
                
                tilesetSprites.Add(tilesetName, sprite);
                tilesetSources.Add(tilesetName, sources);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (lightFlicker) light.Flicker(gameTime);

            if (frameCount > 1)
            {
                animationTime += gameTime.ElapsedGameTime.Milliseconds;
                while (animationTime >= frameLength)
                {
                    animationTime -= frameLength;
                    frame++;
                    if (frame >= frameCount) frame = 0;

                    source = sources[cell + frameOffset[frame]];
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            float spriteDepth = 1.0f;
            if (layerType == LayerType.Obstacle || layerType == LayerType.TallObstacle) spriteDepth = camera.GetDepth(positionZ) + depth;
            else spriteDepth = depth;

            spriteBatch.Draw(sprite, destination, source, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, spriteDepth);
        }

        public void DrawDepth(SpriteBatch spriteBatch, Camera camera)
        {
            float spriteDepth = 1.0f;
            if (height != -1)
            {
                if (layerType == LayerType.Background) spriteDepth = depth;
                else spriteDepth = camera.GetDepth(positionZ) + depth;
            }
            else spriteDepth = depth;

            Color depthData = new Color(upperByteZ, lowerByteZ, (byte)255, (byte)255);

            spriteBatch.Draw(sprite, destination, source, depthData, 0.0f, Vector2.Zero, SpriteEffects.None, spriteDepth);
        }

        public Light Light { get => light; }
        public TmxTilesetTile TilesetData { get => tmxTilesetTile; }
    }
}
