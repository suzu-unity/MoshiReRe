using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// uGUI Graphic を継承した5項目レーダーチャート描画コンポーネント。
/// StatusPage から SetValues() で値を渡す。
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class RadarChartRenderer : Graphic
{
    [Header("軸設定")]
    [SerializeField] private string[] labels = { "A", "B", "C", "D", "E" };

    [Header("値設定")]
    [SerializeField] private float[] values = { 2f, 4f, 6f, 1f, 5f };

    [Header("スケール")]
    [SerializeField] private float minValue = 0f;
    [SerializeField] private float maxValue = 10f;

    [Header("見た目")]
    [SerializeField] private float outerRadius = 100f;
    [SerializeField] private float lineThickness = 2f;
    [SerializeField] private Color chartColor = new Color(0.2f, 0.6f, 1f, 0.5f);
    [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private int gridLevels = 5;

    private const int AxisCount = 5;

    public void SetValues(float[] newValues)
    {
        if (newValues == null || newValues.Length != AxisCount) return;
        values = newValues;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (values == null || values.Length < AxisCount) return;

        // グリッド描画
        for (int level = 1; level <= gridLevels; level++)
        {
            float ratio = (float)level / gridLevels;
            DrawPolygon(vh, ratio, gridColor);
        }

        // データポリゴン描画
        DrawDataPolygon(vh);
    }

    private Vector2[] GetAxisPoints(float ratio)
    {
        var points = new Vector2[AxisCount];
        for (int i = 0; i < AxisCount; i++)
        {
            float angle = Mathf.PI / 2f + (2f * Mathf.PI / AxisCount) * i;
            points[i] = new Vector2(
                Mathf.Cos(angle) * outerRadius * ratio,
                Mathf.Sin(angle) * outerRadius * ratio
            );
        }
        return points;
    }

    private void DrawPolygon(VertexHelper vh, float ratio, Color col)
    {
        var pts = GetAxisPoints(ratio);
        for (int i = 0; i < AxisCount; i++)
        {
            int next = (i + 1) % AxisCount;
            DrawLine(vh, pts[i], pts[next], lineThickness, col);
        }
    }

    private void DrawDataPolygon(VertexHelper vh)
    {
        var pts = new Vector2[AxisCount];
        for (int i = 0; i < AxisCount; i++)
        {
            float normalized = Mathf.Clamp01((values[i] - minValue) / (maxValue - minValue));
            float angle = Mathf.PI / 2f + (2f * Mathf.PI / AxisCount) * i;
            pts[i] = new Vector2(
                Mathf.Cos(angle) * outerRadius * normalized,
                Mathf.Sin(angle) * outerRadius * normalized
            );
        }

        // 塗りつぶし三角形ファン
        int baseIdx = vh.currentVertCount;
        AddVert(vh, Vector2.zero, chartColor);
        for (int i = 0; i < AxisCount; i++)
            AddVert(vh, pts[i], chartColor);

        for (int i = 0; i < AxisCount; i++)
        {
            int curr = baseIdx + 1 + i;
            int next = baseIdx + 1 + (i + 1) % AxisCount;
            vh.AddTriangle(baseIdx, curr, next);
        }

        // 輪郭線
        Color outline = new Color(chartColor.r, chartColor.g, chartColor.b, 1f);
        for (int i = 0; i < AxisCount; i++)
            DrawLine(vh, pts[i], pts[(i + 1) % AxisCount], lineThickness, outline);
    }

    private void AddVert(VertexHelper vh, Vector2 pos, Color col)
    {
        var uiv = UIVertex.simpleVert;
        uiv.position = pos;
        uiv.color = col;
        vh.AddVert(uiv);
    }

    private void DrawLine(VertexHelper vh, Vector2 a, Vector2 b, float thickness, Color col)
    {
        Vector2 dir = (b - a).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x) * (thickness * 0.5f);

        int idx = vh.currentVertCount;
        AddVert(vh, a + perp, col);
        AddVert(vh, a - perp, col);
        AddVert(vh, b - perp, col);
        AddVert(vh, b + perp, col);
        vh.AddTriangle(idx, idx + 1, idx + 2);
        vh.AddTriangle(idx, idx + 2, idx + 3);
    }
}
