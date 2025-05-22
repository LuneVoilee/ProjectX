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
            EdgeVertices e = new EdgeVertices(v1, v2);

            AddTriangleByEdge(center, e, cell.Color);

            if (dir <= HexDirection.SE)
            {
                AddBridgeAndCorner(cell, dir, e);
            }
        }
    }

    #region 梯田相关Bridge&&Corner

    private void AddBridgeAndCorner(HexCell cell, HexDirection dir, EdgeVertices e1)
    {
        if (cell.coordinates.X == 3 && cell.coordinates.Z == 3)
            Debug.Log("1");

        if (!cell.GetNeighbor(dir, out var neighbor))
            return;

        var bridge = HexUtil.GetBridge(dir);
        var v3 = e1.v1 + bridge;
        var v4 = e1.v4 + bridge;
        v3.y = v4.y = neighbor.Position.y;
        EdgeVertices e2 = new EdgeVertices(v3, v4);

        //梯田Bridge
        if (cell.GetEdgeType(dir) == HexEdgeType.Slope)
        {
            HandleSlope(e1, cell, e2, neighbor);
        }
        //斜面Bridge
        else
        {
            AddQuadByEdge(e1, cell.Color, e2, neighbor.Color);
        }


        if (dir > HexDirection.E || !cell.GetNeighbor(dir.Next(), out var nextNeighbor))
            return;
        var v5 = e1.v4 + HexUtil.GetBridge(dir.Next());
        v5.y = nextNeighbor.Position.y;

        var IsCellLowerOrEqualToNeighbor = cell.Height <= neighbor.Height;
        var IsCellLowerThanNextNeighbor = cell.Height < nextNeighbor.Height;
        var IsNeighborLowerOrEqualToNextNeighbor = neighbor.Height <= nextNeighbor.Height;

        //cell最矮
        if (IsCellLowerOrEqualToNeighbor && IsCellLowerThanNextNeighbor)
        {
            HandleCorner(e1.v4, cell, v4, neighbor, v5, nextNeighbor);
            return;
        }

        //NextNeighbor最矮(Cell<=Neighbor时)
        if (IsCellLowerOrEqualToNeighbor)
        {
            HandleCorner(v5, nextNeighbor, e1.v4, cell, v4, neighbor);
            return;
        }

        //Neighbor最矮
        if (IsNeighborLowerOrEqualToNextNeighbor)
        {
            HandleCorner(v4, neighbor, v5, nextNeighbor, e1.v4, cell);
            return;
        }

        //NextNeighbor最矮(Cell>Neighbor时)
        HandleCorner(v5, nextNeighbor, e1.v4, cell, v4, neighbor);
    }


    void HandleSlope(
        EdgeVertices begin, HexCell beginCell,
        EdgeVertices end, HexCell endCell
    )
    {
        EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
        Color c2 = HexUtil.TerraceLerp(beginCell.Color, endCell.Color, 1);

        AddQuadByEdge(begin, beginCell.Color, e2, c2);

        for (int i = 2; i < HexUtil.terraceSteps; i++)
        {
            EdgeVertices e1 = e2;
            Color c1 = c2;
            e2 = EdgeVertices.TerraceLerp(begin, end, i);
            c2 = HexUtil.TerraceLerp(beginCell.Color, endCell.Color, i);
            AddQuadByEdge(e1, c1, e2, c2);
        }

        AddQuadByEdge(e2, c2, end, endCell.Color);
    }

    void HandleCorner(
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
                //左右皆梯田
                HandleCase_TwoTerraces(
                    bottom, bottomCell, left, leftCell, right, rightCell
                );
            }

            else if (rightEdgeType == HexEdgeType.Flat)
            {
                //左梯田右平地
                //HandleCase_TwoTerraces处理插值时不分高低，所有可以把leftCell当作bottom用放在第一个
                HandleCase_TwoTerraces(
                    left, leftCell, right, rightCell, bottom, bottomCell
                );
            }

            else
            {
                //左梯田右悬崖
                HandleCase_OneTerracesAndOneCliff(
                    bottom, bottomCell, left, leftCell, right, rightCell
                );
            }
        }
        //同理
        else if (rightEdgeType == HexEdgeType.Slope)
        {
            if (leftEdgeType == HexEdgeType.Flat)
            {
                HandleCase_TwoTerraces(
                    right, rightCell, bottom, bottomCell, left, leftCell
                );
            }
            else
            {
                HandleCase_OneTerracesAndOneCliff(
                    bottom, bottomCell, right, rightCell, left, leftCell, false
                );
            }
        }
        //左右皆悬崖，左右之间有梯田
        else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            if (leftCell.Height < rightCell.Height)
            {
                HandleCase_OneTerracesAndOneCliff(
                    right, rightCell, left, leftCell, bottom, bottomCell, false
                );
            }
            else
            {
                HandleCase_OneTerracesAndOneCliff(
                    left, leftCell, right, rightCell, bottom, bottomCell
                );
            }
        }
        //三Cell之间没有梯田，用三角形正常处理
        else
        {
            AddTriangle(bottom, left, right);
            AddTriangleColor(bottomCell.Color, leftCell.Color, rightCell.Color);
        }
    }

    void HandleCase_TwoTerraces(
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

    void HandleCase_OneTerracesAndOneCliff(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell, bool isClockwise = true
    )
    {
        var b = Mathf.Abs(1f / (rightCell.Height - beginCell.Height));
        var boundary = Vector3.Lerp(Perturb(begin), Perturb(right), b);
        var boundaryColor = Color.Lerp(beginCell.Color, rightCell.Color, b);

        //处理底-左 (梯田)
        HandleCase_TerracesCorner(begin, beginCell, left, leftCell, boundary, boundaryColor, isClockwise);

        //处理左-右
        //左->右是梯田
        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            HandleCase_TerracesCorner(
                left, leftCell, right, rightCell, boundary, boundaryColor, isClockwise
            );
        }
        //左->右是悬崖
        else
        {
            AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary, isClockwise);
            AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor, isClockwise);
        }
    }

    private void HandleCase_TerracesCorner(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell,
        Vector3 boundary, Color boundaryColor, bool isClockwise)
    {
        var v2 = Perturb(HexUtil.TerraceLerp(begin, left, 1));
        var c2 = HexUtil.TerraceLerp(beginCell.Color, leftCell.Color, 1);

        AddTriangleUnperturbed(Perturb(begin), v2, boundary, isClockwise);
        AddTriangleColor(beginCell.Color, c2, boundaryColor, isClockwise);

        for (var i = 2; i < HexUtil.terraceSteps; i++)
        {
            Vector3 v1 = v2;
            Color c1 = c2;
            v2 = Perturb(HexUtil.TerraceLerp(begin, left, i));
            c2 = HexUtil.TerraceLerp(beginCell.Color, leftCell.Color, i);
            AddTriangleUnperturbed(v1, v2, boundary, isClockwise);
            AddTriangleColor(c1, c2, boundaryColor, isClockwise);
        }

        AddTriangleUnperturbed(v2, Perturb(left), boundary, isClockwise);
        AddTriangleColor(c2, leftCell.Color, boundaryColor, isClockwise);
    }

    #endregion


    #region 辅助

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

    void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3, bool isClockwise = true)
    {
        var vertexIndex = m_Vertices.Count;
        if (isClockwise)
        {
            m_Vertices.Add(v1);
            m_Vertices.Add(v2);
            m_Vertices.Add(v3);
        }
        else
        {
            m_Vertices.Add(v1);
            m_Vertices.Add(v3);
            m_Vertices.Add(v2);
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

    public struct EdgeVertices
    {
        public Vector3 v1, v2, v3, v4;

        public EdgeVertices(Vector3 v1, Vector3 v2)
        {
            this.v1 = v1;
            this.v2 = Vector3.Lerp(v1, v2, 1f / 3f);
            v3 = Vector3.Lerp(v1, v2, 2f / 3f);
            v4 = v2;
        }

        public static EdgeVertices TerraceLerp(EdgeVertices a, EdgeVertices b, int step)
        {
            EdgeVertices result;
            result.v1 = HexUtil.TerraceLerp(a.v1, b.v1, step);
            result.v2 = HexUtil.TerraceLerp(a.v2, b.v2, step);
            result.v3 = HexUtil.TerraceLerp(a.v3, b.v3, step);
            result.v4 = HexUtil.TerraceLerp(a.v4, b.v4, step);
            return result;
        }
    }

    void AddTriangleByEdge(Vector3 center, EdgeVertices edge, Color color)
    {
        AddTriangle(center, edge.v1, edge.v2);
        AddTriangleColor(color);
        AddTriangle(center, edge.v2, edge.v3);
        AddTriangleColor(color);
        AddTriangle(center, edge.v3, edge.v4);
        AddTriangleColor(color);
    }

    void AddQuadByEdge(
        EdgeVertices e1, Color c1,
        EdgeVertices e2, Color c2
    )
    {
        AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
        AddQuadColor(c1, c2);
        AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
        AddQuadColor(c1, c2);
        AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
        AddQuadColor(c1, c2);
    }

    #endregion

    #region Noise

    Vector3 Perturb(Vector3 position)
    {
        var sample = HexUtil.SampleNoise(position);
        sample = (sample * 2f - Vector4.one) * HexUtil.PerturbStrength;
        position.x += sample.x;
        //平整地面 //position.y += sample.y;
        position.z += sample.z;
        return position;
    }

    #endregion
}