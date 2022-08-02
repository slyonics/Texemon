using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

using Panderling.Main;
using Panderling.Procedural;

namespace Panderling.GameObjects.Maps
{
    public class Light
    {
        public const float DEFAULT_INTENSITY = 64.0f;
        public const float DEFAULT_FLICKER_SIZE = 1.0f;
        public const float DEFAULT_FLICKER_MEAN = 100.0f;
        public const float DEFAULT_FLICKER_STD = 100.0f;
        public const float DEFAULT_FLICKER_SPEED = 20000.0f;

        private Vector2 position;
        private float positionZ;
        private Color color = Color.White;
        private float intensity = 64.0f;

        private float lightFlicker;
        private float flickerSize = 1.0f;
        private float flickerMean = 100.0f;
        private float flickerStd = 100.0f;
        private float flickerSpeed = 20000.0f;
        
        public Light(Vector2 iPosition, float iPositionZ)
        {
            position = iPosition;
            positionZ = iPositionZ;
        }

        public void Flicker(GameTime gameTime, float mean, float std, float speed, float size)
        {
            lightFlicker += (float)Math.Abs(Rng.GaussianDouble(mean, std)) * gameTime.ElapsedGameTime.Milliseconds / speed;
            flickerSize = size;
        }

        public void Flicker(GameTime gameTime)
        {
            lightFlicker += (float)Math.Abs(Rng.GaussianDouble(flickerMean, flickerStd)) * gameTime.ElapsedGameTime.Milliseconds / flickerSpeed;
        }

        public void SetFlicker(float mean, float std, float speed, float size)
        {
            flickerMean = mean;
            flickerStd = std;
            flickerSpeed = speed;
            flickerSize = size;
        }

        public Vector2 Position { set => position = value; get => position; }
        public float PositionZ { set => positionZ = value; get => positionZ; }
        public float Intensity { set => intensity = value; get => intensity + (float)Math.Sin(lightFlicker) * flickerSize; }
        public Color Color { set => color = value; get => color; }
    }
}
