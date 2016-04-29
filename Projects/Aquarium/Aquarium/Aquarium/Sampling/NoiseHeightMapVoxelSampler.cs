using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forever.Voxel.World;
using Microsoft.Xna.Framework;
using LibNoise;
using Forever.Voxel;

namespace Aquarium.Sampling
{
    class NoiseHeightMapVoxelSampler : IVoxelSampler
    {
        public float MinHeight { get; set; }
        public float MaxHeight { get; set; }

        public Color StartColor { get; set; }
        public Color EndColor { get; set; }

        IModule Noise { get; set; }

        IModule Noise_ColorR { get; set; }
        IModule Noise_ColorG { get; set; }
        IModule Noise_ColorB { get; set; }
        IModule Noise_ColorA { get; set; }

        public NoiseHeightMapVoxelSampler()
        {
            NoiseQuality quality = NoiseQuality.High;
            int seed = 1;
            int octaves = 3;
            double frequency = 0.001;
            double lacunarity = 1.5;
            double persistence = 0.6;

            var module = new Perlin();
            module.Frequency = frequency;
            module.NoiseQuality = quality;
            module.Seed = seed;
            module.OctaveCount = octaves;
            module.Lacunarity = lacunarity;
            module.Persistence = persistence;

            Noise = module;
            MinHeight = 0;
            MaxHeight = 1;
            StartColor = Color.Black;
            EndColor = Color.White;

            double colorFreq = 0.025;
            double colorLac = 1;
            int colorOctaves = 2;
            double colorPersist = 0.2;

            Noise_ColorA = new Perlin() { 
                Seed = 1,
                Frequency = colorFreq,
                Lacunarity = colorLac,
                OctaveCount = colorOctaves,
                Persistence = colorPersist,
                NoiseQuality = NoiseQuality.Low
            };
            Noise_ColorB = new Perlin()
            {
                Seed = 2,
                Frequency = colorFreq,
                Lacunarity = colorLac,
                OctaveCount = colorOctaves,
                Persistence = colorPersist,
                NoiseQuality = NoiseQuality.Low
            };
            Noise_ColorG = new Perlin()
            {
                Seed = 3,
                Frequency = colorFreq,
                Lacunarity = colorLac,
                OctaveCount = colorOctaves,
                Persistence = colorPersist,
                NoiseQuality = NoiseQuality.Low
            };
            Noise_ColorR = new Perlin()
            {
                Seed = 4,
                Frequency = colorFreq,
                Lacunarity = colorLac,
                OctaveCount = colorOctaves,
                Persistence = colorPersist,
                NoiseQuality = NoiseQuality.Low
            };
        }

        public Voxel GetSample(Vector3 samplePoint, float voxelScale)
        {
            return GetSample(samplePoint.X, samplePoint.Y, samplePoint.Z, voxelScale);
        }

        public Voxel GetSample(float x, float y, float z, float scale)
        {
            float height = GetSurfaceHeight(x, z);
            bool active = y < height;

            x *= scale;
            y *= scale;
            z *= scale;

            return new Voxel
            {
                Material = new Material(GetColor(x, height, z)),
                State = active ? VoxelState.Active : VoxelState.Inactive
            };
        }

        Color GetColor(float x, float y, float z)
        {
            var s = StartColor.ToVector4();
            var e = EndColor.ToVector4();

            float rNoise = SmoothNoise(Noise_ColorR, x, y, z);
            float gNoise = SmoothNoise(Noise_ColorG, x, y, z);
            float bNoise = SmoothNoise(Noise_ColorB, x, y, z);
            float aNoise = SmoothNoise(Noise_ColorA, x, y, z);

            return new Color(
                Microsoft.Xna.Framework.MathHelper.Lerp(s.X, e.X, rNoise),
                Microsoft.Xna.Framework.MathHelper.Lerp(s.Y, e.Y, gNoise),
                Microsoft.Xna.Framework.MathHelper.Lerp(s.Z, e.Z, bNoise),
                Microsoft.Xna.Framework.MathHelper.Lerp(s.W, e.W, aNoise)
                );
        }

        float SmoothNoise(float x, float y)
        {
            return (float)(Noise.GetValue((double)x, (double)y, 0));
        }

        float SmoothNoise(IModule noise, float x, float y, float z)
        {
            return (float)(noise.GetValue((double)x, (double)y, (double)z));
        }

        public float GetSurfaceHeight(float x, float z)
        {

            float n = SmoothNoise(x, z);
            return MinHeight + (n * (MaxHeight - MinHeight));
        }
    }
}
