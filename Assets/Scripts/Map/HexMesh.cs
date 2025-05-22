using System;
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
            var center = cell.Position;

            var v1 = center + HexUtil.GetFirstSolidVector(dir);
            var v2 = center + HexUtil.GetSecondSolidVector(dir);
            AddTriangle
            (
                center,
                v1,
                v2
            );
            AddTriangleColor(cell.Color);

            if (dir <= HexDirection.SE)
            {
                AddBridgeAndCorner(cell, dir, v1, v2);
            }
        }
    }

    #region 梯田相关Bridge&&Corner

    private void AddBridgeAndCorner(HexCell cell, HexDirection dir, Vector3 v1, Vector3 v2)
    {
        if (cell.coordinates.X == 3 && cell.coordinates.Z == 3)
            Debug.Log("1");

        if (!cell.GetNeighbor(dir, out var neighbor))
            return;

        var bridge = HexUtil.GetBridge(dir);
        var v3 = v1 + bridge;
        var v4 = v2 + bridge;
        v3.y = v4.y = neighbor.Position.y;

        if (cell.GetEdgeType(dir) == HexEdgeType.Slope)
        {
            TriangulateSlopeBridge(v1, v2, cell, v3, v4, neighbor);
        }
        else
        {
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(cell.Color, neighbor.Color);
        }


        if (dir > HexDirection.E || !cell.GetNeighbor(dir.Next(), out var nextNeighbor))
            return;
        var v5 = v2 + HexUtil.GetBridge(dir.Next());
        v5.y = nextNeighbor.Position.y;

        if (cell.Height <= neighbor.Height)
        {
            if (cell.Height < nextNeighbor.Height)
            {
                TriangulateCorner(v2, cell, v4, neighbor, v5, nextNeighbor);
            }
            else
            {
                TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
            }
        }
        else if (neighbor.Height <= nextNeighbor.Height)
        {
            TriangulateCorner(v4, neighbor, v5, nextNeighbor, v2, cell);
        }
        else
        {
            TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
        }
    }


    void TriangulateSlopeBridge(
        Vector3 beginLeft, Vector3 beginRight, HexCell beginCell,
        Vector3 endLeft, Vector3 endRight, HexCell endCell
    )
    {
        Vector3 v3 = HexUtil.TerraceLerp(beginLeft, endLeft, 1);
        Vector3 v4 = HexUtil.TerraceLerp(beginRight, endRight, 1);
        Color c2 = HexUtil.TerraceLerp(beginCell.Color, endCell.Color, 1);

        AddQuad(beginLeft, beginRight, v3, v4);
        AddQuadColor(beginCell.Color, c2);

        for (var i = 2; i < HexUtil.terraceSteps; i++)
        {
            var v1 = v3;
            var v2 = v4;
            var c1 = c2;
            v3 = HexUtil.TerraceLerp(beginLeft, endLeft, i);
            v4 = HexUtil.TerraceLerp(beginRight, endRight, i);
            c2 = HexUtil.TerraceLerp(beginCell.Color, endCell.Color, i);
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2);
        }

        AddQuad(v3, v4, endLeft, endRight);
        AddQuadColor(c2, endCell.Color);
    }

    void TriangulateCorner(
        Vector3 bottom, HexCell bottomCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
    )
    {
        var leftEdgeType = bottomCell.GetEdgeType(leftCell);
        var rightEdgeType = bottomCell.GetEdgeType(rightCell);

        if (leftEdgeType == HexEdgeType.Slope)
        {
            if (rightEdgeType == HexEdgeType.Slope)
            {
                TriangulateCornerTerraces(
                    bottom, bottomCell, left, leftCell, right, rightCell
                );
            }

            else if (rightEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(
                    left, leftCell, right, rightCell, bottom, bottomCell
                );
            }

            else
            {
                TriangulateCornerTerracesCliff(
                    bottom, bottomCell, left, leftCell, right, rightCell
                );
            }
        }
        else if (rightEdgeType == HexEdgeType.Slope)
        {
            if (leftEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(
                    right, rightCell, bottom, bottomCell, left, leftCell
                );
            }
            else
            {
                TriangulateCornerTerracesCliff(
                    bottom, bottomCell, right, rightCell, left, leftCell, false
                );
            }
        }
        //左右皆悬崖，左右之间有梯田
        else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            if (leftCell.Height < rightCell.Height)
            {
                TriangulateCornerTerracesCliff(
                    right, rightCell, bottom, bottomCell, left, leftCell, false
                );
            }
            else
            {
                TriangulateCornerTerracesCliff(
                    left, leftCell, right, rightCell, bottom, bottomCell
                );
            }
        }
        else
        {
            AddTriangle(bottom, left, right);
            AddTriangleColor(bottomCell.Color, leftCell.Color, rightCell.Color);
        }
    }

    void TriangulateCornerTerraces(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
    )
    {
        Vector3 v3 = HexUtil.TerraceLerp(begin, left, 1);
        Vector3 v4 = HexUtil.TerraceLerp(begin, right, 1);
        Color c3 = HexUtil.TerraceLerp(beginCell.Color, leftCell.Color, 1);
        Color c4 = HexUtil.TerraceLerp(beginCell.Color, rightCell.Color, 1);

        AddTriangle(begin, v3, v4);
        AddTriangleColor(beginCell.Color, c3, c4);

        for (var i = 2; i < HexUtil.terraceSteps; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c3;
            Color c2 = c4;
            v3 = HexUtil.TerraceLerp(begin, left, i);
            v4 = HexUtil.TerraceLerp(begin, right, i);
            c3 = HexUtil.TerraceLerp(beginCell.Color, leftCell.Color, i);
            c4 = HexUtil.TerraceLerp(beginCell.Color, rightCell.Color, i);
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2, c3, c4);
        }

        AddQuad(v3, v4, left, right);
        AddQuadColor(c3, c4, leftCell.Color, rightCell.Color);
    }

    void TriangulateCornerTerracesCliff(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell, bool isClockwise = true
    )
    {
        var b = Mathf.Abs(1f / (rightCell.Height - beginCell.Height));
        var boundary = Vector3.Lerp(begin, right, b);
        var boundaryColor = Color.Lerp(beginCell.Color, rightCell.Color, b);

        //处理底-悬崖
        HandleCliffDetail(begin, beginCell, left, leftCell, boundary, boundaryColor, isClockwise);

        //处理左-悬崖
        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            HandleCliffDetail(
                left, leftCell, right, rightCell, boundary, boundaryColor, isClockwise
            );
        }
        else
        {
            AddTriangle(left, right, boundary, isClockwise);
            AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor, isClockwise);
        }
    }

    private void HandleCliffDetail(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell,
        Vector3 boundary, Color boundaryColor, bool isClockwise)
    {
        var v2 = HexUtil.TerraceLerp(begin, left, 1);
        var c2 = HexUtil.TerraceLerp(beginCell.Color, leftCell.Color, 1);

        AddTriangle(begin, v2, boundary, isClockwise);
        AddTriangleColor(beginCell.Color, c2, boundaryColor, isClockwise);

        for (var i = 2; i < HexUtil.terraceSteps; i++)
        {
            Vector3 v1 = v2;
            Color c1 = c2;
            v2 = HexUtil.TerraceLerp(begin, left, i);
            c2 = HexUtil.TerraceLerp(beginCell.Color, leftCell.Color, i);
            AddTriangle(v1, v2, boundary, isClockwise);
            AddTriangleColor(c1, c2, boundaryColor, isClockwise);
        }

        AddTriangle(v2, left, boundary, isClockwise);
        AddTriangleColor(c2, leftCell.Color, boundaryColor, isClockwise);
    }

    #endregion


    #region 辅助方法

    private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3, bool isClockwise = true)
    {
        var vertexIndex = m_Vertices.Count;
        if (isClockwise)
        {
            m_Vertices.Add(Perturb(v1));
            m_Vertices.Add(Perturb(v2));
            m_Vertices.Add(Perturb(v3));
        }
        else
        {
            m_Vertices.Add(Perturb(v1));
            m_Vertices.Add(Perturb(v3));
            m_Vertices.Add(Perturb(v2));
        }

        m_Triangles.Add(vertexIndex);
        m_Triangles.Add(vertexIndex + 1);
        m_Triangles.Add(vertexIndex + 2);
    }

    private void AddTriangleColor(Color color)
    {
        AddTriangleColor(color, color, color);
    }

    private void AddTriangleColor(Color color1, Color color2, Color color3, bool isClockwise = true)
    {
        if (isClockwise)
        {
            m_Colors.Add(color1);
            m_Colors.Add(color2);
            m_Colors.Add(color3);
        }
        else
        {
            m_Colors.Add(color1);
            m_Colors.Add(color3);
            m_Colors.Add(color2);
        }
    }

    void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        int vertexIndex = m_Vertices.Count;
        m_Vertices.Add(Perturb(v1));
        m_Vertices.Add(Perturb(v2));
        m_Vertices.Add(Perturb(v3));
        m_Vertices.Add(Perturb(v4));

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

    void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
    {
        m_Colors.Add(c1);
        m_Colors.Add(c2);
        m_Colors.Add(c3);
        m_Colors.Add(c4);
    }

    #endregion

    #region Noise

    Vector3 Perturb(Vector3 position)
    {
        var sample = HexUtil.SampleNoise(position);
        sample = (sample * 2f - Vector4.one) * HexUtil.PerturbStrength;
        position.x += sample.x;
        position.y += sample.y;
        position.z += sample.z;
        return position;
    }

    #endregion
}