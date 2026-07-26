using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draws and raycasts a polygonal map hot spot using normalized map coordinates.
/// </summary>
public class MapLocationPolygon : Graphic, ICanvasRaycastFilter
{
    [SerializeField] private Vector2[] normalizedPoints;
    [SerializeField] private Color fillColor = new Color(1f, 1f, 1f, 0.12f);
    [SerializeField] private Color outlineColor = new Color(1f, 1f, 1f, 0.7f);
    [SerializeField] private Color hoverOutlineColor = new Color(1f, 0.92f, 0.35f, 1f);
    [SerializeField] private Color selectedOutlineColor = new Color(1f, 0.43f, 0.48f, 1f);
    [SerializeField, Min(1f)] private float outlineWidth = 4f;

    private bool isHovered;
    private bool isSelected;
    private Color locationColor = Color.white;

    public void Initialize(Vector2[] points)
    {
        normalizedPoints = points;
        SetVerticesDirty();
    }

    public void SetVisual(bool selected, Color locationTint)
    {
        isSelected = selected;
        locationColor = locationTint;
        SetVerticesDirty();
    }

    public void SetHover(bool hovered)
    {
        if (isHovered == hovered)
            return;

        isHovered = hovered;
        SetVerticesDirty();
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (normalizedPoints == null
            || normalizedPoints.Length < 3
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out var local))
            return false;

        var rect = rectTransform.rect;
        var point = new Vector2((local.x - rect.xMin) / rect.width, (local.y - rect.yMin) / rect.height);
        var inside = false;
        for (int i = 0, j = normalizedPoints.Length - 1; i < normalizedPoints.Length; j = i++)
        {
            var a = normalizedPoints[i];
            var b = normalizedPoints[j];
            if ((a.y > point.y) != (b.y > point.y)
                && point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
                inside = !inside;
        }

        return inside;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (normalizedPoints == null || normalizedPoints.Length < 3)
            return;

        var rect = rectTransform.rect;
        var fill = fillColor * locationColor;
        fill.a = isSelected ? 0.28f : isHovered ? 0.20f : 0.10f;
        var outline = isSelected ? selectedOutlineColor : isHovered ? hoverOutlineColor : outlineColor;

        for (var i = 0; i < normalizedPoints.Length; i++)
            vh.AddVert(ToLocal(normalizedPoints[i], rect), fill, Vector2.zero);
        for (var i = 1; i < normalizedPoints.Length - 1; i++)
            vh.AddTriangle(0, i, i + 1);

        for (var i = 0; i < normalizedPoints.Length; i++)
        {
            var a = ToLocal(normalizedPoints[i], rect);
            var b = ToLocal(normalizedPoints[(i + 1) % normalizedPoints.Length], rect);
            var normal = Vector2.Perpendicular((b - a).normalized) * (outlineWidth * 0.5f);
            var index = vh.currentVertCount;
            vh.AddVert(a - normal, outline, Vector2.zero);
            vh.AddVert(a + normal, outline, Vector2.zero);
            vh.AddVert(b + normal, outline, Vector2.zero);
            vh.AddVert(b - normal, outline, Vector2.zero);
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
        }
    }

    private static Vector2 ToLocal(Vector2 normalized, Rect rect)
    {
        return new Vector2(
            Mathf.Lerp(rect.xMin, rect.xMax, normalized.x),
            Mathf.Lerp(rect.yMin, rect.yMax, normalized.y));
    }
}
