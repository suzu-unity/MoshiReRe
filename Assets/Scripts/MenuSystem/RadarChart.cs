using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class RadarChart : MaskableGraphic
{
    [SerializeField] private float radius = 100f;
    [SerializeField] private float maxValue = 10f; // Max value for the chart (edge of the polygon)
    
    // Values for the 3 axes
    private float value1 = 1f; // Top (Intelligence)
    private float value2 = 1f; // Bottom Right (Courage)
    private float value3 = 1f; // Bottom Left (Strength)

    public void SetValues(float v1, float v2, float v3)
    {
        value1 = v1;
        value2 = v2;
        value3 = v3;
        // SetVerticesDirty() is now called in GenerateMesh()
    }

    public void GenerateMesh()
    {
        SetVerticesDirty(); // Request redraw
    }

    [SerializeField] private Color backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    protected override void OnEnable()
    {
        base.OnEnable();
        SetAllDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = rectTransform.rect;
        Debug.Log($"[RadarChart] OnPopulateMesh called. Rect: {rect}, Values: {value1}, {value2}, {value3}");
        if (rect.width <= 0 || rect.height <= 0)
        {
            Debug.LogWarning($"[RadarChart] Rect size is zero or negative: {rect}. Chart will not be visible.");
        }

        Vector2 center = Vector2.zero;

        // Angles
        float rad1 = 90f * Mathf.Deg2Rad;
        float rad2 = -30f * Mathf.Deg2Rad;
        float rad3 = -150f * Mathf.Deg2Rad;

        // --- Draw Background (Max Value) ---
        Vector2 b1 = new Vector2(Mathf.Cos(rad1), Mathf.Sin(rad1)) * radius;
        Vector2 b2 = new Vector2(Mathf.Cos(rad2), Mathf.Sin(rad2)) * radius;
        Vector2 b3 = new Vector2(Mathf.Cos(rad3), Mathf.Sin(rad3)) * radius;

        // Vertices 0-3 for background
        vh.AddVert(center, backgroundColor, Vector2.zero);
        vh.AddVert(b1, backgroundColor, Vector2.zero);
        vh.AddVert(b2, backgroundColor, Vector2.zero);
        vh.AddVert(b3, backgroundColor, Vector2.zero);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(0, 2, 3);
        vh.AddTriangle(0, 3, 1);

        // --- Draw Value Polygon ---
        float n1 = Mathf.Clamp01(value1 / maxValue);
        float n2 = Mathf.Clamp01(value2 / maxValue);
        float n3 = Mathf.Clamp01(value3 / maxValue);

        Vector2 v1 = new Vector2(Mathf.Cos(rad1), Mathf.Sin(rad1)) * radius * n1;
        Vector2 v2 = new Vector2(Mathf.Cos(rad2), Mathf.Sin(rad2)) * radius * n2;
        Vector2 v3 = new Vector2(Mathf.Cos(rad3), Mathf.Sin(rad3)) * radius * n3;

        // Vertices 4-7 for values (Offset by 4)
        int offset = 4;
        vh.AddVert(center, color, Vector2.zero);
        vh.AddVert(v1, color, Vector2.zero);
        vh.AddVert(v2, color, Vector2.zero);
        vh.AddVert(v3, color, Vector2.zero);

        vh.AddTriangle(offset + 0, offset + 1, offset + 2);
        vh.AddTriangle(offset + 0, offset + 2, offset + 3);
        vh.AddTriangle(offset + 0, offset + 3, offset + 1);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif
}
