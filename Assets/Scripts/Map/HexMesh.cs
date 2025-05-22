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

    private void Triangulate(HexCell cell)
    {
        for (var dir = HexDirection.NE; dir <= HexDirection.NW; dir++)
        {
            var center = cell.transform.localPosition;

            var v1 = center + HexConfig.GetFirstSolidVector(dir);
            var v2 = center + HexConfig.GetSecondSolidVector(dir);
            AddTriangle
            (
                center,
                v1,
                v2
            );
            AddTriangleColor(cell.color);

            if (dir <= HexDirection.SE)
            {
                AddBridge(cell, dir, v1, v2);
            }
        }
    }

    private void AddBridge(HexCell cell, HexDirection dir, Vector3 v1, Vector3 v2)
    {
        if (cell.coordinates.X == 3 && cell.coordinates.Z == 3)
            Debug.Log("1");

        if (!cell.GetNeighbor(dir, out var neighbor))
            return;

        var bridge = HexConfig.GetBridge(dir);
        var v3 = v1 + bridge;
        var v4 = v2 + bridge;
        AddQuad(v1, v2, v3, v4);
        //var bridgeColor = (cell.color + neighbor.color) * 0.5f;
        AddQuadColor(cell.color, neighbor.color);


        if (dir > HexDirection.E || !cell.GetNeighbor(dir.Next(), out var nextNeighbor))
            return;

        AddTriangle(v2, v4, v2 + HexConfig.GetBridge(dir.Next()));
        AddTriangleColor(cell.color, neighbor.color, nextNeighbor.color);
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

    private void AddTriangleColor(Color color)
    {
        m_Colors.Add(color);
        m_Colors.Add(color);
        m_Colors.Add(color);
    }

    private void AddTriangleColor(Color color1, Color color2, Color color3)
    {
        m_Colors.Add(color1);
        m_Colors.Add(color2);
        m_Colors.Add(color3);
    }

    void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        int vertexIndex = m_Vertices.Count;
        m_Vertices.Add(v1);
        m_Vertices.Add(v2);
        m_Vertices.Add(v3);
        m_Vertices.Add(v4);
        m_Triangles.Add(vertexIndex);
        m_Triangles.Add(vertexIndex + 2);
        m_Triangles.Add(vertexIndex + 1);
        m_Triangles.Add(vertexIndex + 1);
        m_Triangles.Add(vertexIndex + 2);
        m_Triangles.Add(vertexIndex + 3);
    }

    void AddQuadColor(Color c1, Color c2)
    {
        m_Colors.Add(c1);
        m_Colors.Add(c1);
        m_Colors.Add(c2);
        m_Colors.Add(c2);
    }
}