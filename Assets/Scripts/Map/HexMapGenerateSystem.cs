#region

using Camera;
using Input;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Map
{
    public class HexMapGenerateSystem : MonoBehaviour
    {
        public static HexMapGenerateSystem Instance;

        public HexCell CellPrefab;
        public HexChunk ChunkPrefab;
        public Text InfoLabelPrefab;
        public Color defaultColor = Color.white;
        public Texture2D NoiseSource;

        public int ChunkCountX = 4;
        public int ChunkCountZ = 3;

        private int m_CellCountX = 6;
        private int m_CellCountZ = 6;
        private HexCell[] m_Cells;

        private HexChunk[] m_Chunks;
        private CameraInputHandler m_CameraInputHandler;

        private bool isLabelVisible;

        private void Awake()
        {
            m_CellCountX = HexUtil.ChunkSizeX * ChunkCountX;
            m_CellCountZ = HexUtil.ChunkSizeZ * ChunkCountZ;
            m_Cells = new HexCell[m_CellCountZ * m_CellCountX];


            HexUtil.NoiseSource = NoiseSource;

            CreateChunks();
            CreateCells();

            Instance = this;
        }

        //在 OnEnable 事件方法中重新分配纹理。该方法将在重新编译后被调用。
        private void OnEnable()
        {
            HexUtil.NoiseSource = NoiseSource;

            if (KInput.MainCamera.gameObject.TryGetComponent(out m_CameraInputHandler))
            {
                m_CameraInputHandler.OnShowAction += SwitchLabelVisible;
            }
        }

        private void OnDisable()
        {
            HexUtil.NoiseSource = null;

            if (m_CameraInputHandler)
            {
                m_CameraInputHandler.OnShowAction -= SwitchLabelVisible;
            }
        }

        private void SwitchLabelVisible()
        {
            isLabelVisible = !isLabelVisible;

            ShowLabel(isLabelVisible);
        }

        private void ShowLabel(bool isVisible)
        {
            foreach (var chunk in m_Chunks)
            {
                chunk.ShowLabel(isVisible);
            }
        }

        private void CreateChunks()
        {
            m_Chunks = new HexChunk[ChunkCountX * ChunkCountZ];

            for (int z = 0, i = 0; z < ChunkCountZ; z++)
            {
                for (var x = 0; x < ChunkCountX; x++)
                {
                    var chunk = m_Chunks[i++] = Instantiate(ChunkPrefab);
                    chunk.transform.SetParent(transform);
                }
            }

            ShowLabel(false);
        }

        private void CreateCells()
        {
            for (int z = 0, i = 0; z < m_CellCountZ; z++)
            {
                for (var x = 0; x < m_CellCountX; x++)
                {
                    CreateCell(x, z, i++);
                }
            }
        }

        private void CreateCell(int x, int z, int i)
        {
            Vector3 position;
            position.x = (x + z * 0.5f - z / 2) * (HexUtil.InnerRadius * 2f);
            position.y = 0f;
            position.z = z * (HexUtil.OuterRadius * 1.5f);

            var cell = m_Cells[i] = Instantiate(CellPrefab);
            cell.transform.localPosition = position;
            cell.coordinates = HexCoordinates.FromCellOffset(x, z);
            cell.Color = defaultColor;

            SetNeighbors(x, z, i, cell);

            var label = Instantiate(InfoLabelPrefab);
            label.rectTransform.anchoredPosition =
                new Vector2(position.x, position.z);
            label.text = cell.coordinates.ToStringOnSeparateLines();
            cell.UITransform = label.rectTransform;

            //应用扰动
            cell.Height = 0;

            AddCellToChunk(x, z, cell);
        }

        private void AddCellToChunk(int x, int z, HexCell cell)
        {
            var chunkX = x / HexUtil.ChunkSizeX;
            var chunkZ = z / HexUtil.ChunkSizeZ;

            var chunk = m_Chunks[chunkX + chunkZ * ChunkCountX];
            var localX = x - chunkX * HexUtil.ChunkSizeX;
            var localZ = z - chunkZ * HexUtil.ChunkSizeZ;
            chunk.AddCell(localX + localZ * HexUtil.ChunkSizeX, cell);
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
                    cell.SetNeighbor(HexDirection.SE, m_Cells[i - m_CellCountX]);
                    if (x > 0)
                    {
                        cell.SetNeighbor(HexDirection.SW, m_Cells[i - m_CellCountX - 1]);
                    }
                }
                else
                {
                    cell.SetNeighbor(HexDirection.SW, m_Cells[i - m_CellCountX]);
                    if (x < m_CellCountX - 1)
                    {
                        cell.SetNeighbor(HexDirection.SE, m_Cells[i - m_CellCountX + 1]);
                    }
                }
            }
        }


        public HexCell GetCell(HexCoordinates coordinates)
        {
            var (x, z) = HexCoordinates.ToCellOffset(coordinates.X, coordinates.Z);

            var index = x + z * m_CellCountX;

            return m_Cells[index];
        }

        public HexCell GetCell(Vector3 position)
        {
            position = transform.InverseTransformPoint(position);
            var coordinates = HexCoordinates.FromCellPosition(position);

            return GetCell(coordinates);
        }
    }
}