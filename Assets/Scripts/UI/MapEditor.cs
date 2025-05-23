#region

using Camera;
using Input;
using Map;
using UnityEngine;

#endregion

namespace UI
{
    public class MapEditor : MonoBehaviour
    {
        public Color[] colors = { Color.white, Color.yellow, Color.blue, Color.black };

        private Color m_ActiveColor;
        private int m_ActiveHeight;
        private int m_BrushSize;

        private CameraInputHandler m_CameraInputHandler;

        private void Awake()
        {
            SelectColor(0);
        }

        private void OnEnable()
        {
            if (KInput.MainCamera.gameObject.TryGetComponent(out m_CameraInputHandler))
            {
                m_CameraInputHandler.OnClickAction += EditCells;
            }
        }

        private void OnDisable()
        {
            if (m_CameraInputHandler)
            {
                m_CameraInputHandler.OnClickAction -= EditCells;
            }
        }

        private void EditCells(Vector3 pos)
        {
            var genSystem = HexMapGenerateSystem.Instance;
            if (!genSystem)
            {
                Debug.LogError("HexMapGenerateSystem not found");
                return;
            }

            var cell = genSystem.GetCell(pos);

            EditCells(cell);
        }

        private void EditCells(HexCell center)
        {
            var centerX = center.coordinates.X;
            var centerZ = center.coordinates.Z;

            for (int r = 0, z = centerZ - m_BrushSize; z <= centerZ; z++, r++)
            {
                for (var x = centerX - r; x <= centerX + m_BrushSize; x++)
                {
                    EditCell(new HexCoordinates(x, z));
                }
            }

            for (int r = 0, z = centerZ + m_BrushSize; z > centerZ; z--, r++)
            {
                for (var x = centerX - m_BrushSize; x <= centerX + r; x++)
                {
                    EditCell(new HexCoordinates(x, z));
                }
            }
        }

        private void EditCell(HexCoordinates coordinates)
        {
            var genSystem = HexMapGenerateSystem.Instance;
            if (!genSystem)
            {
                Debug.LogError("HexMapGenerateSystem not found");
                return;
            }

            var cell = genSystem.GetCell(coordinates);

            if (cell)
            {
                cell.Color = m_ActiveColor;
                cell.Height = m_ActiveHeight;
            }
        }

        public void SelectColor(int index)
        {
            m_ActiveColor = colors[index];
        }

        public void SetHeight(float elevation)
        {
            m_ActiveHeight = (int)elevation;
        }

        public void SetBrushSize(float size)
        {
            m_BrushSize = (int)size;
        }
    }
}