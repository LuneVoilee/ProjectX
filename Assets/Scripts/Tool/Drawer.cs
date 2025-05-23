#region

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Tool
{
    public class Drawer : MonoBehaviour
    {
        private static Drawer m_Instance;

        public static Drawer Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    GameObject go = new GameObject("RayVisualizer");
                    m_Instance = go.AddComponent<Drawer>();
                }

                return m_Instance;
            }
        }

        private class RayInfo
        {
            public LineRenderer lineRenderer;
            public float duration;
            public float timer;
        }

        private readonly List<RayInfo> activeRays = new();

        public void Ray(Vector3 startPoint, Vector3 endPoint, Color color,
            float duration = 5f, float width = 5f)
        {
            var lineObj = new GameObject("DynamicRay");
            var lr = lineObj.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Unlit/Color")) { color = color };
            lr.positionCount = 2;
            lr.SetPositions(new[] { startPoint, endPoint });
            lr.startWidth = width;
            lr.endWidth = width;

            var rayInfo = new RayInfo
            {
                lineRenderer = lr,
                duration = duration,
                timer = 0
            };
            activeRays.Add(rayInfo);
        }

        private void LateUpdate()
        {
            for (var i = activeRays.Count - 1; i >= 0; i--)
            {
                RayInfo rayInfo = activeRays[i];
                rayInfo.timer += Time.deltaTime;

                if (rayInfo.timer >= rayInfo.duration)
                {
                    Destroy(rayInfo.lineRenderer.gameObject);
                    activeRays.RemoveAt(i);
                }
            }
        }
    }
}