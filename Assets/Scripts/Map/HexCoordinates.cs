#region

using System;
using UnityEngine;

#endregion

namespace Map
{
    [Serializable]
    public struct HexCoordinates
    {
        public int X => m_X;
        public int Y => -X - Z;
        public int Z => m_Z;

        [SerializeField] private int m_X, m_Z;

        public HexCoordinates(int x, int z)
        {
            m_X = x;
            m_Z = z;
        }

        public override string ToString()
        {
            return "(" + X + ", " + Y + ", " + Z + ")";
        }

        public string ToStringOnSeparateLines()
        {
            return X + "\n" + Y + "\n" + Z;
        }

        public static HexCoordinates FromCellOffset(int x, int z)
        {
            return new HexCoordinates(x - z / 2, z);
        }
        
        public static (int x, int z) ToCellOffset(int x, int z)
        {
            return (x + z / 2, z);
        }

        public static HexCoordinates FromCellPosition(Vector3 position)
        {
            var x = position.x / (HexUtil.InnerRadius * 2f);
            var y = -x;

            var offset = position.z / (HexUtil.OuterRadius * 3f);
            x -= offset;
            y -= offset;

            var iX = Mathf.RoundToInt(x);
            var iY = Mathf.RoundToInt(y);
            var iZ = Mathf.RoundToInt(-x - y);

            if (iX + iY + iZ == 0)
            {
                return new HexCoordinates(iX, iZ);
            }

            var dX = Mathf.Abs(x - iX);
            var dY = Mathf.Abs(y - iY);
            var dZ = Mathf.Abs(-x - y - iZ);

            if (dX > dY && dX > dZ)
            {
                iX = -iY - iZ;
            }
            else if (dZ > dY)
            {
                iZ = -iX - iY;
            }

            return new HexCoordinates(iX, iZ);
        }
    }
}