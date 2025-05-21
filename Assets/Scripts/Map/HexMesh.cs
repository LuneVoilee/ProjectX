using System.Collections.Generic;
using Map;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    private Mesh m_Mesh;
    private List<int> m_Triangles;
    private List<Vector3> m_Vertices;
    private MeshCollider m_MeshCollider;
    private List<Color> m_Colors;

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = m_Mesh = new Mesh();
        m_MeshCollider = gameObject.AddComponent<MeshCollider>();

        m_Mesh.name = "Hex Mesh";
        m_Vertices = new List<Vector3>();
        m_Triangles = new List<int>();
        m_Colors = new List<Color>();
    }

    public void Triangulate(HexCell[] cells)
    {
        m_Mesh.Clear();
        m_Vertices.Clear();
        m_Triangles.Clear();
        m_Colors.Clear();

        foreach (var t in cells)
        {
            Triangulate(t);
        }

        m_Mesh.vertices = m_Vertices.ToArray();
        m_Mesh.triangles = m_Triangles.ToArray();
        m_Mesh.colors = m_Colors.ToArray();

        m_Mesh.RecalculateNormals();

        m_MeshCollider.sharedMesh = m_Mesh;
    }

    public void Triangulate(HexCell cell)
    {
        var center = cell.transform.localPosition;
        for (var i = 0; i < 6; i++)
        {
            AddTriangle
            (
                center,
                center + HexConfig.VerticesDir[i],
                center + HexConfig.VerticesDir[i + 1]
            );

            AddTriangleColor(cell.color);
        }
    }

    private void AddTriangleColor(Color color)
    {
        m_Colors.Add(color);
        m_Colors.Add(color);
        m_Colors.Add(color);
    }

    private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        var vertexIndex = m_Vertices.Count;
        m_Vertices.Add(v1);
        m_Vertices.Add(v2);
        m_Vertices.Add(v3);
        m_Triangles.Add(vertexIndex);
        m_Triangles.Add(vertexIndex + 1);
        m_Triangles.Add(vertexIndex + 2);
    }
}