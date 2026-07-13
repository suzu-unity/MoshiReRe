using System.Collections.Generic;
using UnityEngine;

public enum ComicPanelFocusMode
{
    Through,
    Only,
    All
}

public static class ComicPanelFocusState
{
    public static bool IsEmphasized(int panelIndex, int focusIndex, ComicPanelFocusMode mode)
    {
        if (mode == ComicPanelFocusMode.All || focusIndex < 0)
            return true;

        return mode == ComicPanelFocusMode.Only
            ? panelIndex == focusIndex
            : panelIndex <= focusIndex;
    }
}

public static class ComicPanelGeometry
{
    private const float Epsilon = 0.00001f;

    public static IReadOnlyList<Vector2> SanitizeVertices(IReadOnlyList<Vector2> source)
    {
        var result = new List<Vector2>();
        if (source != null)
        {
            for (var i = 0; i < source.Count; i++)
            {
                var point = new Vector2(Mathf.Clamp01(source[i].x), Mathf.Clamp01(source[i].y));
                if (result.Count == 0 || (result[result.Count - 1] - point).sqrMagnitude > Epsilon * Epsilon)
                    result.Add(point);
            }
        }

        if (result.Count > 1 && (result[0] - result[result.Count - 1]).sqrMagnitude <= Epsilon * Epsilon)
            result.RemoveAt(result.Count - 1);

        if (result.Count < 3)
        {
            result.Clear();
            result.Add(new Vector2(0f, 0f));
            result.Add(new Vector2(1f, 0f));
            result.Add(new Vector2(1f, 1f));
            result.Add(new Vector2(0f, 1f));
        }

        return result;
    }

    public static List<int> Triangulate(IReadOnlyList<Vector2> points)
    {
        var vertices = SanitizeVertices(points);
        var indices = new List<int>();
        if (vertices.Count < 3)
            return indices;

        var remaining = new List<int>(vertices.Count);
        for (var i = 0; i < vertices.Count; i++)
            remaining.Add(i);

        var winding = SignedArea(vertices) >= 0f ? 1f : -1f;
        var guard = vertices.Count * vertices.Count;
        while (remaining.Count > 3 && guard-- > 0)
        {
            var earFound = false;
            for (var i = 0; i < remaining.Count; i++)
            {
                var previous = remaining[(i + remaining.Count - 1) % remaining.Count];
                var current = remaining[i];
                var next = remaining[(i + 1) % remaining.Count];
                var a = vertices[previous];
                var b = vertices[current];
                var c = vertices[next];

                if (winding * Cross(b - a, c - a) <= Epsilon)
                    continue;

                var containsPoint = false;
                for (var j = 0; j < remaining.Count; j++)
                {
                    var candidate = remaining[j];
                    if (candidate == previous || candidate == current || candidate == next)
                        continue;
                    if (PointInTriangle(vertices[candidate], a, b, c, winding))
                    {
                        containsPoint = true;
                        break;
                    }
                }

                if (containsPoint)
                    continue;

                indices.Add(previous);
                indices.Add(current);
                indices.Add(next);
                remaining.RemoveAt(i);
                earFound = true;
                break;
            }

            if (!earFound)
                return FanTriangulate(vertices.Count);
        }

        if (remaining.Count == 3)
        {
            indices.Add(remaining[0]);
            indices.Add(remaining[1]);
            indices.Add(remaining[2]);
        }

        return indices;
    }

    private static List<int> FanTriangulate(int count)
    {
        var result = new List<int>(Mathf.Max(0, count - 2) * 3);
        for (var i = 1; i < count - 1; i++)
        {
            result.Add(0);
            result.Add(i);
            result.Add(i + 1);
        }

        return result;
    }

    private static float SignedArea(IReadOnlyList<Vector2> points)
    {
        var area = 0f;
        for (var i = 0; i < points.Count; i++)
        {
            var next = points[(i + 1) % points.Count];
            area += points[i].x * next.y - next.x * points[i].y;
        }

        return area * 0.5f;
    }

    private static bool PointInTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c, float winding)
    {
        var ab = winding * Cross(b - a, point - a);
        var bc = winding * Cross(c - b, point - b);
        var ca = winding * Cross(a - c, point - c);
        return ab >= -Epsilon && bc >= -Epsilon && ca >= -Epsilon;
    }

    private static float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }
}
