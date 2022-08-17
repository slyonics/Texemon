﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Texemon.Main
{
    public enum GameFont
    {
        PersonifiedUXRegular,
        Tooltip,
        BigTooltip,
        Title,
        Dialogue,
        BitPotion,
        Irisdot21
    }

    public static class Text
    {
        public class GameFontData
        {
            public string fontFile;
            public int fontSize;
            public int fontHeight;
        }

        private const float TEXT_DEPTH = 0.1f;

        public static readonly Dictionary<GameFont, GameFontData> FONT_DATA = new Dictionary<GameFont, GameFontData>()
        {
            { GameFont.Title, new GameFontData() { fontFile = "PersonifiedUXRegular", fontSize = 96, fontHeight = 96 } },
            { GameFont.PersonifiedUXRegular, new GameFontData() { fontFile = "PersonifiedUXRegular", fontSize = 32, fontHeight = 40 } },
            { GameFont.Dialogue, new GameFontData() { fontFile = "Futuradot-H10", fontSize = 10, fontHeight = 8 } },
            // { GameFont.Dialogue, new GameFontData() { fontFile = "BitPotionExt", fontSize = 24, fontHeight = 16 } },
            { GameFont.Tooltip, new GameFontData() { fontFile = "Futuradot-H10", fontSize = 10, fontHeight = 8 } },
            { GameFont.BitPotion, new GameFontData() { fontFile = "BitPotionExt", fontSize = 14, fontHeight = 10 } },
            { GameFont.Irisdot21, new GameFontData() { fontFile = "Irisdot-H21-Condensed", fontSize = 18, fontHeight = 14 } },
        };

        public static readonly Dictionary<GameFont, SpriteFont> GAME_FONTS = new Dictionary<GameFont, SpriteFont>();

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            Dictionary<string, byte[]> rawFonts = new Dictionary<string, byte[]>();
            List<Tuple<byte[], byte[]>> assetData = AssetCache.LoadAssetData("Fonts.jam");
            foreach (Tuple<byte[], byte[]> asset in assetData)
            {
                string fontName = Encoding.ASCII.GetString(asset.Item1);
                rawFonts.Add(fontName, asset.Item2);
            }

            foreach (KeyValuePair<GameFont, GameFontData> fontEntry in FONT_DATA)
            {
                TtfFontBakerResult bakedFont = TtfFontBaker.Bake(rawFonts[fontEntry.Value.fontFile], fontEntry.Value.fontSize, 1024, 1024, new[] { CharacterRange.BasicLatin });
                GAME_FONTS.Add(fontEntry.Key, bakedFont.CreateSpriteFont(graphicsDevice));
            }
        }

        public static void DrawText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, int row = 0)
        {
            Vector2 offset = new Vector2(0, -row * GetStringHeight(font));
            spriteBatch.DrawString(GAME_FONTS[font], text, position, Color.White, 0.0f, offset, 1.0f, SpriteEffects.None, TEXT_DEPTH);
        }

        public static void DrawText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, float depth, int row = 0)
        {
            Vector2 offset = new Vector2(0, -row * GetStringHeight(font));
            spriteBatch.DrawString(GAME_FONTS[font], text, position, Color.White, 0.0f, offset, 1.0f, SpriteEffects.None, depth);
        }

        public static void DrawText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, Color color, int row = 0)
        {
            Vector2 offset = new Vector2(0, -row * GetStringHeight(font));
            spriteBatch.DrawString(GAME_FONTS[font], text, position, color, 0.0f, offset, 1.0f, SpriteEffects.None, TEXT_DEPTH);
        }

        public static void DrawText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, Color color, float depth, int row = 0)
        {
            Vector2 offset = new Vector2(0, -row * GetStringHeight(font));
            spriteBatch.DrawString(GAME_FONTS[font], text, position, color, 0.0f, offset, 1.0f, SpriteEffects.None, depth);
        }

        public static void DrawCenteredText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, int row = 0)
        {
            Vector2 offset = new Vector2(GetStringLength(font, text) / 2, -row * GetStringHeight(font) + GetStringHeight(font) / 2);
            spriteBatch.DrawString(GAME_FONTS[font], text, position, Color.White, 0.0f, offset, 1.0f, SpriteEffects.None, TEXT_DEPTH);
        }

        public static void DrawCenteredText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, Color color, int row = 0)
        {
            Vector2 offset = new Vector2(GetStringLength(font, text) / 2, -row * GetStringHeight(font) + GetStringHeight(font) / 2);
            spriteBatch.DrawString(GAME_FONTS[font], text, position, color, 0.0f, offset, 1.0f, SpriteEffects.None, TEXT_DEPTH);
        }

        public static void DrawCenteredText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, Color color, float depth, int row = 0)
        {
            Vector2 offset = new Vector2(GetStringLength(font, text) / 2, -row * GetStringHeight(font) + GetStringHeight(font) / 2);
            spriteBatch.DrawString(GAME_FONTS[font], text, position, color, 0.0f, offset, 1.0f, SpriteEffects.None, depth);
        }


        public static void DrawCenteredText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, float depth, int row = 0)
        {
            Vector2 offset = new Vector2(GetStringLength(font, text) / 2, -row * GetStringHeight(font) + GetStringHeight(font) / 2);
            spriteBatch.DrawString(GAME_FONTS[font], text, position, Color.White, 0.0f, offset, 1.0f, SpriteEffects.None, depth);
        }

        public static void DrawCenteredText(SpriteBatch spriteBatch, Vector2 position, GameFont font, string text, float depth, Color color, int row = 0)
        {
            Vector2 offset = new Vector2(GetStringLength(font, text) / 2, -row * GetStringHeight(font) + GetStringHeight(font) / 2);
            spriteBatch.DrawString(GAME_FONTS[font], text, position, color, 0.0f, offset, 1.0f, SpriteEffects.None, depth);
        }

        public static int GetStringLength(GameFont font, string text)
        {
            return (int)GAME_FONTS[font].MeasureString(text).X;
        }

        public static int GetStringHeight(GameFont font)
        {
            return FONT_DATA[font].fontHeight;
        }
    }
}
