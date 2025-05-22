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
        public static HexMapGenerateSystem Instance;

        public int Width = 6;
        public int Height = 6;

        public HexCell CellPrefab;
        public Text InfoLabelPrefab;
        public Color defaultColor = Color.white;
        public Texture2D NoiseSource;

        private HexCell[] m_Cells;
        private HexMesh m_HexMesh;
        private Canvas m_InfoCanvas;

        //在 OnEnable 事件方法中重新分配纹理。该方法将在重新编译后被调用。
        private void OnEnable()
        {
            HexUtil.NoiseSource = NoiseSource;
        }

        private void OnDisable()
        {
            HexUtil.NoiseSource = null;
        }

        private void Awake()
        {
            m_InfoCanvas = GetComponentInChildren<Canvas>();
            m_HexMesh = GetComponentInChildren<HexMesh>();

            m_Cells = new HexCell[Height * Width];

            HexUtil.NoiseSource = NoiseSource;

            for (int z = 0, i = 0; z < Height; z++)
            {
                for (var x = 0; x < Width; x++)
                {
                    CreateCell(x, z, i++);
                }
            }

            Instance = this;
        }

        private void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            m_HexMesh.Triangulate(m_Cells);
        }

        private void CreateCell(int x, int z, int i)
        {
            Vector3 position;
            position.x = (x + z * 0.5f - z / 2) * (HexUtil.InnerRadius * 2f);
            position.y = 0f;
            position.z = z * (HexUtil.OuterRadius * 1.5f);

            var cell = m_Cells[i] = Instantiate(CellPrefab);
            cell.transform.SetParent(transform, false);
            cell.transform.localPosition = position;
            cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
            cell.Color = defaultColor;

            SetNeighbors(x, z, i, cell);

            var label = Instantiate(InfoLabelPrefab, m_InfoCanvas.transform, false);

            label.rectTransform.anchoredPosition =
                new Vector2(position.x, position.z);
            label.text = cell.coordinates.ToStringOnSeparateLines();
            cell.UITransform = label.rectTransform;

            //应用扰动
            cell.Height = 0;
        }

        private void SetNeighbors(int x, int z, int i, HexCell cell)
        {
            if (x > 0)
            {
                cell.SetNeighbor(HexDirection.W, m_Cells[i - 1]);
            }

            if (z > 0)
            {
                if ((z & 1) == 0)
                {
                    cell.SetNeighbor(HexDirection.SE, m_Cells[i - Width]);
                    if (x > 0)
                    {
                        cell.SetNeighbor(HexDirection.SW, m_Cells[i - Width - 1]);
                    }
                }
                else
                {
                    cell.SetNeighbor(HexDirection.SW, m_Cells[i - Width]);
                    if (x < Width - 1)
                    {
                        cell.SetNeighbor(HexDirection.SE, m_Cells[i - Width + 1]);
                    }
                }
            }
        }

        public HexCell GetCell(Vector3 position)
        {
            position = transform.InverseTransformPoint(position);

            var coordinates = HexCoordinates.FromPosition(position);
            var index = coordinates.X + coordinates.Z * Width + coordinates.Z / 2;
            return m_Cells[index];
        }
    }
}