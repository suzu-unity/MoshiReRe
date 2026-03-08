using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class RadarChart : MaskableGraphic
{
    [SerializeField] private float radius = 100f;
    [SerializeField] private float maxValue = 10f;
    [SerializeField] private Color backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.35f);

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

        int bgCenter = vh.currentVertCount;
        vh.AddVert(center, backgroundColor, Vector2.zero);

        int bgStart = vh.currentVertCount;
        for (int i = 0; i < axisCount; i++)
        {
            float angle = Mathf.Deg2Rad * (90f - (360f / axisCount) * i);
            Vector2 point = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            vh.AddVert(point, backgroundColor, Vector2.zero);
        }

        for (int i = 0; i < axisCount; i++)
        {
            int next = (i + 1) % axisCount;
            vh.AddTriangle(bgCenter, bgStart + i, bgStart + next);
        }

        int valCenter = vh.currentVertCount;
        vh.AddVert(center, color, Vector2.zero);

        int valStart = vh.currentVertCount;
        for (int i = 0; i < axisCount; i++)
        {
            float n = Mathf.Clamp01(values[i] / maxValue);
            float angle = Mathf.Deg2Rad * (90f - (360f / axisCount) * i);
            Vector2 point = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius * n;
            vh.AddVert(point, color, Vector2.zero);
        }

        for (int i = 0; i < axisCount; i++)
        {
            int next = (i + 1) % axisCount;
            vh.AddTriangle(valCenter, valStart + i, valStart + next);
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif
}
