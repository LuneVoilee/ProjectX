using System;
using Camera;
using Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Map
{
    public class HexMapGenerateSystem : MonoBehaviour
    {
        public int Width = 6;
        public int Height = 6;

        public HexCell CellPrefab;
        public Text InfoLabelPrefab;

        public Color defaultColor = Color.white;
        public Color touchedColor = Color.magenta;

        private HexCell[] m_Cells;
        private HexMesh m_HexMesh;
        private Canvas m_InfoCanvas;
        private CameraInputHandler m_CameraInputHandler;

        private void Awake()
        {
            m_InfoCanvas = GetComponentInChildren<Canvas>();
            m_HexMesh = GetComponentInChildren<HexMesh>();

            m_Cells = new HexCell[Height * Width];

            for (int z = 0, i = 0; z < Height; z++)
            {
                for (var x = 0; x < Width; x++)
                {
                    CreateCell(x, z, i++);
                }
            }
        }

        private void OnEnable()
        {
            if (KInput.MainCamera.gameObject.TryGetComponent(out m_CameraInputHandler))
            {
                m_CameraInputHandler.m_OnClickAction += TouchCell;
            }
        }

        private void OnDisable()
        {
            if (m_CameraInputHandler)
            {
                m_CameraInputHandler.m_OnClickAction -= TouchCell;
            }
        }

        private void Start()
        {
            m_HexMesh.Triangulate(m_Cells);
        }

        private void CreateCell(int x, int z, int i)
        {
            Vector3 position;
            position.x = (x + z * 0.5f - z / 2) * (HexConfig.innerRadius * 2f);
            position.y = 0f;
            position.z = z * (HexConfig.outerRadius * 1.5f);

            var cell = m_Cells[i] = Instantiate(CellPrefab);
            cell.transform.SetParent(transform, false);
            cell.transform.localPosition = position;
            cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
            cell.color = defaultColor;

            var label = Instantiate(InfoLabelPrefab, m_InfoCanvas.transform, false);

            label.rectTransform.anchoredPosition =
                new Vector2(position.x, position.z);
            label.text = cell.coordinates.ToStringOnSeparateLines();
        }

        void TouchCell(Vector3 position)
        {
            position = transform.InverseTransformPoint(position);

            var coordinates = HexCoordinates.FromPosition(position);
            var index = coordinates.X + coordinates.Z * Width + coordinates.Z / 2;
            var cell = m_Cells[index];
            cell.color = touchedColor;

            Debug.Log("touch:" + cell.coordinates);
            m_HexMesh.Triangulate(m_Cells);
        }
    }
}