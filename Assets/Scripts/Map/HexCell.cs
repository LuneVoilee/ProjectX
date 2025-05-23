#region

using UnityEngine;

#endregion

namespace Map
{
    public class HexCell : MonoBehaviour
    {
        public HexCoordinates coordinates;

        public RectTransform UITransform;

        public HexChunk Chunk;

        [SerializeField] private HexCell[] neighbors;


        private int m_Height = int.MinValue;

        public Color Color
        {
            get => m_Color;
            set
            {
                if (m_Color == value)
                {
                    return;
                }

                m_Color = value;
                Refresh();
            }
        }

        private Color m_Color;

        public int Height
        {
            get => m_Height;
            set
            {
                if (m_Height == value)
                {
                    return;
                }

                m_Height = value;
                var position = transform.localPosition;
                position.y = value * HexUtil.HeightStep;
                position.y +=
                    (HexUtil.SampleNoise(position).y * 2f - 1f) *
                    HexUtil.HeightPerturbStrength;
                transform.localPosition = position;

                var uiPos = UITransform.localPosition;
                uiPos.z = m_Height * -HexUtil.HeightStep;
                UITransform.localPosition = uiPos;

                Refresh();
            }
        }

        public Vector3 Position => transform.localPosition;

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
            return HexUtil.GetEdgeType(
                Height, neighbors[(int)direction].Height
            );
        }


        public HexEdgeType GetEdgeType(HexCell other)
        {
            return HexUtil.GetEdgeType(
                Height, other.Height
            );
        }

        private void Refresh()
        {
            if (Chunk)
            {
                Chunk.Refresh();
                foreach (var neighbor in neighbors)
                {
                    if (neighbor && neighbor.Chunk != Chunk)
                    {
                        neighbor.Chunk.Refresh();
                    }
                }
            }
        }
    }
}