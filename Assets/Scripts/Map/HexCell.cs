using UnityEngine;

namespace Map
{
    public enum HexDirection
    {
        //东北
        NE,

        //东
        E,

        //东南
        SE,

        //西南
        SW,

        //西
        W,

        //西北
        NW
    }

    public static class HexDirectionExtensions
    {
        public static HexDirection Opposite(this HexDirection direction)
        {
            return (int)direction < 3 ? (direction + 3) : (direction - 3);
        }

        public static HexDirection Previous(this HexDirection direction)
        {
            return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
        }

        public static HexDirection Next(this HexDirection direction)
        {
            return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
        }
    }

    public class HexCell : MonoBehaviour
    {
        public HexCoordinates coordinates;

        public Color color;

        [SerializeField] private HexCell[] neighbors;

        public HexCell GetNeighbor(HexDirection direction)
        {
            return neighbors[(int)direction];
        }

        public bool GetNeighbor(HexDirection direction, out HexCell cell)
        {
            cell = neighbors[(int)direction];
            return cell;
        }

        public void SetNeighbor(HexDirection direction, HexCell neighbor)
        {
            neighbors[(int)direction] = neighbor;
            neighbor.neighbors[(int)direction.Opposite()] = this;
        }
    }
}