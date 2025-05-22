using System;
using UnityEngine;

namespace Map
{
    public static class HexUtil
    {
        #region HexCell

        public const float OuterRadius = 10f;

        public const float InnerRadius = OuterRadius * 0.866025404f;

        public const float SolidFactor = 0.8f;

        public const float BlendFactor = 1f - SolidFactor;


        private static readonly Vector3[] VerticesDir =
        {
            new(0f, 0f, OuterRadius),
            new(InnerRadius, 0f, 0.5f * OuterRadius),
            new(InnerRadius, 0f, -0.5f * OuterRadius),
            new(0f, 0f, -OuterRadius),
            new(-InnerRadius, 0f, -0.5f * OuterRadius),
            new(-InnerRadius, 0f, 0.5f * OuterRadius),
            new(0f, 0f, OuterRadius)
        };

        public const float HeightPerturbStrength = 1.5f;

        public static Vector3 GetFirstVector(HexDirection direction)
        {
            return VerticesDir[(int)direction];
        }

        public static Vector3 GetFirstSolidVector(HexDirection direction)
        {
            return GetFirstVector(direction) * SolidFactor;
        }

        public static Vector3 GetSecondVector(HexDirection direction)
        {
            return VerticesDir[(int)direction + 1];
        }

        public static Vector3 GetSecondSolidVector(HexDirection direction)
        {
            return GetSecondVector(direction) * SolidFactor;
        }

        public static Vector3 GetBridge(HexDirection direction)
        {
            return (VerticesDir[(int)direction] + VerticesDir[(int)direction + 1]) * BlendFactor;
        }

        #endregion


        #region Height

        public const float HeightStep = 5f;

        public const int EnableSlopeHeight = 1;
        public const int terracesPerSlope = 2;
        public const int terraceSteps = terracesPerSlope * 2 + 1;
        public const float horizontalTerraceStepSize = 1f / terraceSteps;
        public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);

        public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
        {
            var h = step * horizontalTerraceStepSize;
            a.x += (b.x - a.x) * h;
            a.z += (b.z - a.z) * h;

            var v = (step + 1) / 2 * verticalTerraceStepSize;
            a.y += (b.y - a.y) * v;
            return a;
        }

        public static Color TerraceLerp(Color a, Color b, int step)
        {
            float h = step * horizontalTerraceStepSize;
            return Color.Lerp(a, b, h);
        }

        public static HexEdgeType GetEdgeType(int height1, int height2)
        {
            if (height1 == height2)
            {
                return HexEdgeType.Flat;
            }

            if (Math.Abs(height1 - height2) <= EnableSlopeHeight)
            {
                return HexEdgeType.Slope;
            }

            return HexEdgeType.Cliff;
        }

        #endregion


        #region Noise

        public const float PerturbStrength = 4f;

        public const float NoiseScale = 0.003f;
        public static Texture2D NoiseSource;

        public static Vector4 SampleNoise(Vector3 position)
        {
            return NoiseSource.GetPixelBilinear(
                position.x * NoiseScale,
                position.z * NoiseScale
            );
        }

        #endregion
    }
}