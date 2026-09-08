using UnityEngine;
using UnityEngine.UI;

/// <summary>Clips only the exterior of the supplied landscape phone; leaves source PNGs unchanged.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class MenuPhoneArtworkMask : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        var bounds = rectTransform.rect;
        var left = bounds.xMin + 7; var right = bounds.xMax - 7;
        var top = bounds.yMax - 44; var bottom = bounds.yMin + 43;
        const float radius = 133;
        vh.AddVert(new Vector3((left + right) / 2, (top + bottom) / 2), Color.white, Vector2.zero);
        var centers = new[] { new Vector2(right-radius, top-radius), new Vector2(left+radius, top-radius), new Vector2(left+radius, bottom+radius), new Vector2(right-radius, bottom+radius) };
        for (var corner=0;corner<4;corner++)
            for (var segment=0;segment<=12;segment++)
            {
                var angle=(corner*90+segment*7.5f)*Mathf.Deg2Rad;
                vh.AddVert(centers[corner]+new Vector2(Mathf.Cos(angle), Mathf.Sin(angle))*radius, Color.white, Vector2.zero);
            }
        var count=52;
        for(var i=1;i<=count;i++) vh.AddTriangle(0,i,i==count?1:i+1);
    }
}
