using UnityEngine;

namespace Map
{
    public static class HexConfig
    {
        public const float outerRadius = 10f;

        public const float innerRadius = outerRadius * 0.866025404f;

        public static readonly Vector3[] VerticesDir =
        {
            new(0f, 0f, outerRadius),
            new(innerRadius, 0f, 0.5f * outerRadius),
            new(innerRadius, 0f, -0.5f * outerRadius),
            new(0f, 0f, -outerRadius),
            new(-innerRadius, 0f, -0.5f * outerRadius),
            new(-innerRadius, 0f, 0.5f * outerRadius),
            new(0f, 0f, outerRadius)
        };
    }
}