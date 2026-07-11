using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class RadarChart : MaskableGraphic
{
    [SerializeField] private float radius = 100f;
    [SerializeField] private float maxValue = 10f;
    // The DRESS screen supplies its own radar grid. Drawing another filled pentagon
    // underneath the values makes the two status layers read as a blurry shape.
    [SerializeField] private Color backgroundColor = Color.clear;
    [SerializeField] private bool showGrid;
    [SerializeField] private Color gridColor = new Color(0.18f, 0.46f, 0.50f, 0.55f);
    [SerializeField] private float gridLineWidth = 1.5f;

    private readonly float[] values = { 1f, 1f, 1f, 1f, 1f };

    public void SetValues(float v1, float v2, float v3)
    {
        SetValues(v1, v2, 1f, 1f, v3);
    }

    public void SetValues(float v1, float v2, float v3, float v4, float v5)
    {
        values[0] = v1;
        values[1] = v2;
        values[2] = v3;
        values[3] = v4;
        values[4] = v5;
    }

    public void GenerateMesh()
    {
        SetVerticesDirty();
    }

    public void SetRadius(float value)
    {
        radius = Mathf.Max(1f, value);
    }

    public void SetMaxValue(float value)
    {
        maxValue = Mathf.Max(1f, value);
    }

    public void SetGridVisible(bool value)
    {
        showGrid = value;
    }

    public void SetBackgroundColor(Color value)
    {
        backgroundColor = value;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SetAllDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Vector2 center = Vector2.zero;
        const int axisCount = 5;

        if (showGrid)
        {
            for (int level = 1; level <= 3; level++)
            {
                var gridRadius = radius * level / 3f;
                for (int i = 0; i < axisCount; i++)
                {
                    var next = (i + 1) % axisCount;
                    AddLine(vh, PointAt(i, gridRadius), PointAt(next, gridRadius), gridColor, gridLineWidth);
                }
            }

            for (int i = 0; i < axisCount; i++)
                AddLine(vh, center, PointAt(i, radius), gridColor, gridLineWidth);
        }

        if (backgroundColor.a > 0.001f)
        {
            int bgCenter = vh.currentVertCount;
            vh.AddVert(center, backgroundColor, Vector2.zero);
            int bgStart = vh.currentVertCount;
            for (int i = 0; i < axisCount; i++)
                vh.AddVert(PointAt(i, radius), backgroundColor, Vector2.zero);

            for (int i = 0; i < axisCount; i++)
            {
                int next = (i + 1) % axisCount;
                vh.AddTriangle(bgCenter, bgStart + i, bgStart + next);
            }
        }

        int valCenter = vh.currentVertCount;
        vh.AddVert(center, color, Vector2.zero);

        int valStart = vh.currentVertCount;
        for (int i = 0; i < axisCount; i++)
        {
            float n = Mathf.Clamp01(values[i] / maxValue);
            vh.AddVert(PointAt(i, radius * n), color, Vector2.zero);
        }

        for (int i = 0; i < axisCount; i++)
        {
            int next = (i + 1) % axisCount;
            vh.AddTriangle(valCenter, valStart + i, valStart + next);
        }
    }

    private static Vector2 PointAt(int index, float distance)
    {
        const int axisCount = 5;
        float angle = Mathf.Deg2Rad * (90f - (360f / axisCount) * index);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
    }

    private static void AddLine(VertexHelper vh, Vector2 from, Vector2 to, Color color, float width)
    {
        var direction = (to - from).normalized;
        var offset = new Vector2(-direction.y, direction.x) * width * 0.5f;
        int start = vh.currentVertCount;
        vh.AddVert(from - offset, color, Vector2.zero);
        vh.AddVert(from + offset, color, Vector2.zero);
        vh.AddVert(to + offset, color, Vector2.zero);
        vh.AddVert(to - offset, color, Vector2.zero);
        vh.AddTriangle(start, start + 1, start + 2);
        vh.AddTriangle(start, start + 2, start + 3);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif
}
