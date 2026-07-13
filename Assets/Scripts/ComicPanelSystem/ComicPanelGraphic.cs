using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public sealed class ComicPanelGraphic : Graphic
{
    private Sprite sprite;
    private IReadOnlyList<Vector2> normalizedVertices;

    public Sprite Sprite => sprite;

    public void Configure(Sprite newSprite, IReadOnlyList<Vector2> vertices)
    {
        sprite = newSprite;
        normalizedVertices = ComicPanelGeometry.SanitizeVertices(vertices);
        SetVerticesDirty();
        SetMaterialDirty();
    }

    public override Texture mainTexture => sprite != null && sprite.texture != null
        ? sprite.texture
        : Texture2D.whiteTexture;

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
        var points = normalizedVertices ?? ComicPanelGeometry.SanitizeVertices(null);
        var rect = rectTransform.rect;
        var textureRect = sprite != null ? sprite.textureRect : new Rect(0f, 0f, 1f, 1f);
        var textureSize = sprite != null && sprite.texture != null
            ? new Vector2(sprite.texture.width, sprite.texture.height)
            : Vector2.one;

        for (var i = 0; i < points.Count; i++)
        {
            var point = points[i];
            var local = new Vector3(
                Mathf.Lerp(rect.xMin, rect.xMax, point.x),
                Mathf.Lerp(rect.yMin, rect.yMax, point.y),
                0f);
            var uv = new Vector2(
                (textureRect.xMin + textureRect.width * point.x) / textureSize.x,
                (textureRect.yMin + textureRect.height * point.y) / textureSize.y);
            vertexHelper.AddVert(local, color, uv);
        }

        var triangles = ComicPanelGeometry.Triangulate(points);
        for (var i = 0; i < triangles.Count; i += 3)
            vertexHelper.AddTriangle(triangles[i], triangles[i + 1], triangles[i + 2]);
    }
}
