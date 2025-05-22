using UnityEngine;

namespace Map
{
    public static class HexConfig
    {
        public const float outerRadius = 10f;

        public const float innerRadius = outerRadius * 0.866025404f;

        public const float solidFactor = 0.75f;

        public const float blendFactor = 1f - solidFactor;

        private static readonly Vector3[] VerticesDir =
        {
            new(0f, 0f, outerRadius),
            new(innerRadius, 0f, 0.5f * outerRadius),
            new(innerRadius, 0f, -0.5f * outerRadius),
            new(0f, 0f, -outerRadius),
            new(-innerRadius, 0f, -0.5f * outerRadius),
            new(-innerRadius, 0f, 0.5f * outerRadius),
            new(0f, 0f, outerRadius)
        };

        public static Vector3 GetFirstVector(HexDirection direction)
        {
            return VerticesDir[(int)direction];
        }

        public static Vector3 GetFirstSolidVector(HexDirection direction)
        {
            return GetFirstVector(direction) * solidFactor;
        }

        public static Vector3 GetSecondVector(HexDirection direction)
        {
            return VerticesDir[(int)direction + 1];
        }

        public static Vector3 GetSecondSolidVector(HexDirection direction)
        {
            return GetSecondVector(direction) * solidFactor;
        }

        public static Vector3 GetBridge(HexDirection direction)
        {
            return (VerticesDir[(int)direction] + VerticesDir[(int)direction + 1]) * blendFactor;
        }
    }
}