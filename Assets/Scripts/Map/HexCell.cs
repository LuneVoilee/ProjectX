using UnityEngine;

namespace Map
{
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

        public Color Color;

        public RectTransform UITransform;

        public int Height
        {
            get => m_Height;
            set
            {
                m_Height = value;
                Vector3 position = transform.localPosition;
                position.y = value * HexConfig.HeightStep;
                transform.localPosition = position;

                Vector3 uiPos = UITransform.localPosition;
                uiPos.z = m_Height * -HexConfig.HeightStep;
                UITransform.localPosition = uiPos;
            }
        }

        private int m_Height;

        [SerializeField] private HexCell[] neighbors;

        //NOTICE: 以下方法均没有检查邻居的存在性
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

        public HexEdgeType GetEdgeType(HexDirection direction)
        {
            return HexConfig.GetEdgeType(
                Height, neighbors[(int)direction].Height
            );
        }


        public HexEdgeType GetEdgeType(HexCell other)
        {
            return HexConfig.GetEdgeType(
                Height, other.Height
            );
        }
    }
}