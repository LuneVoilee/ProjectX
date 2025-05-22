using Camera;
using Input;
using Map;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class MapEditor : MonoBehaviour
    {
        public Color[] colors = { Color.white, Color.yellow, Color.blue, Color.black };

        private Color m_ActiveColor;
        private int m_ActiveHeight;

        private CameraInputHandler m_CameraInputHandler;

        private void Awake()
        {
            SelectColor(0);
        }

        private void OnEnable()
        {
            if (KInput.MainCamera.gameObject.TryGetComponent(out m_CameraInputHandler))
            {
                m_CameraInputHandler.m_OnClickAction += EditCell;
            }
        }

        private void OnDisable()
        {
            if (m_CameraInputHandler)
            {
                m_CameraInputHandler.m_OnClickAction -= EditCell;
            }
        }

        private void EditCell(Vector3 pos)
        {
            var genSystem = HexMapGenerateSystem.Instance;
            if (!genSystem)
            {
                Debug.LogError("HexMapGenerateSystem not found");
                return;
            }

            var cell = genSystem.GetCell(pos);
            cell.Color = m_ActiveColor;
            cell.Height = m_ActiveHeight;
            genSystem.Refresh();
        }

        public void SelectColor(int index)
        {
            m_ActiveColor = colors[index];
        }

        public void SetHeight(float elevation)
        {
            m_ActiveHeight = (int)elevation;
        }
    }
}