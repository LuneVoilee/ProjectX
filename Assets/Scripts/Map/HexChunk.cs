#region

using UnityEngine;

#endregion

namespace Map
{
    public class HexChunk : MonoBehaviour
    {
        private HexCell[] m_Cells;
        private Canvas m_GridCanvas;
        private HexMesh m_HexMesh;

        private void Awake()
        {
            m_GridCanvas = GetComponentInChildren<Canvas>();
            m_HexMesh = GetComponentInChildren<HexMesh>();

            m_Cells = new HexCell[HexUtil.ChunkSizeX * HexUtil.ChunkSizeZ];
        }

        public void AddCell(int index, HexCell cell)
        {
            m_Cells[index] = cell;
            cell.transform.SetParent(transform, false);
            cell.UITransform.SetParent(m_GridCanvas.transform, false);
            cell.Chunk = this;
        }

        public void Refresh()
        {
            enabled = true;
        }

        private void LateUpdate()
        {
            m_HexMesh.Triangulate(m_Cells);
            enabled = false;
        }

        public void ShowLabel(bool isVisible)
        {
            m_GridCanvas.gameObject.SetActive(isVisible);
        }
    }
}