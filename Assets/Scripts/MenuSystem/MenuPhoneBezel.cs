using UnityEngine;
using UnityEngine.UI;

/// <summary>Resolution-independent, chamfered hardware frame for the pixel phone.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class MenuPhoneBezel : MaskableGraphic
{
    [SerializeField] private float cornerSize = 34f;
    [SerializeField] private float rimWidth = 6f;
    [SerializeField] private Color rimColor = new Color(.48f, .43f, .56f, 1f);

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        var outer = rectTransform.rect;
        var width = Mathf.Clamp(rimWidth, 0f, Mathf.Min(outer.width, outer.height) * .25f);
        var inner = new Rect(outer.x + width, outer.y + width, outer.width - width * 2f, outer.height - width * 2f);
        var radius = Mathf.Clamp(cornerSize, width, Mathf.Min(outer.width, outer.height) * .5f);
        for (var i = 0; i < 8; i++) mesh.AddVert(Corner(outer, radius, i), rimColor, Vector2.zero);
        for (var i = 0; i < 8; i++) mesh.AddVert(Corner(inner, radius - width, i), color, Vector2.zero);
        mesh.AddVert(inner.center, color, Vector2.zero);
        for (var i = 0; i < 8; i++)
        {
            var next = (i + 1) % 8;
            mesh.AddTriangle(i, next, next + 8);
            mesh.AddTriangle(i, next + 8, i + 8);
            mesh.AddTriangle(16, i + 8, next + 8);
        }
    }

    private static Vector2 Corner(Rect r, float c, int i)
    {
        switch (i)
        {
            case 0: return new Vector2(r.xMin + c, r.yMin);
            case 1: return new Vector2(r.xMax - c, r.yMin);
            case 2: return new Vector2(r.xMax, r.yMin + c);
            case 3: return new Vector2(r.xMax, r.yMax - c);
            case 4: return new Vector2(r.xMax - c, r.yMax);
            case 5: return new Vector2(r.xMin + c, r.yMax);
            case 6: return new Vector2(r.xMin, r.yMax - c);
            default: return new Vector2(r.xMin, r.yMin + c);
        }
    }
}
