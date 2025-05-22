using Camera;
using Input;
using Map;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{

    public class MapEditor : MonoBehaviour {

        public Color[] colors =  {Color.white, Color.yellow, Color.blue, Color.black };
        
        private Color activeColor;
        private CameraInputHandler m_CameraInputHandler ;

        private void Awake () {
            SelectColor(0);
        }
        private void OnEnable()
        {
            if (KInput.MainCamera.gameObject.TryGetComponent(out m_CameraInputHandler))
            {
                m_CameraInputHandler.m_OnClickAction += ApplyColor;
            }
        }

        private void OnDisable()
        {
            if (m_CameraInputHandler)
            {
                m_CameraInputHandler.m_OnClickAction -= ApplyColor;
            }
        }
        
        private void ApplyColor(Vector3 pos)
        {
            var genSystem = HexMapGenerateSystem.Instance;
            if (!genSystem)
            {
                Debug.LogError("HexMapGenerateSystem not found");
                return;
            }
            
            genSystem.ColorCell(pos,activeColor);
        }

        public void SelectColor (int index) {
            activeColor = colors[index];
        }
    }
}