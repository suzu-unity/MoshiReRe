using NUnit.Framework;
using UnityEngine;

public sealed class ComicPanelGeometryTests
{
    [Test]
    public void Triangulate_ConcavePolygon_ReturnsCompleteIndexBuffer()
    {
        var points = new[]
        {
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0.4f),
            new Vector2(0.55f, 0.4f), new Vector2(0.55f, 1f), new Vector2(0f, 1f)
        };

        var triangles = ComicPanelGeometry.Triangulate(points);

        Assert.AreEqual((points.Length - 2) * 3, triangles.Count);
        for (var i = 0; i < triangles.Count; i++)
            Assert.That(triangles[i], Is.InRange(0, points.Length - 1));
    }

    [Test]
    public void SanitizeVertices_ClampsAndProvidesRectangleForInvalidInput()
    {
        var result = ComicPanelGeometry.SanitizeVertices(new[] { new Vector2(-1f, 2f) });

        Assert.AreEqual(4, result.Count);
        Assert.AreEqual(new Vector2(0f, 0f), result[0]);
        Assert.AreEqual(new Vector2(1f, 1f), result[2]);
    }

    [TestCase(0, 1, ComicPanelFocusMode.Through, true)]
    [TestCase(2, 1, ComicPanelFocusMode.Through, false)]
    [TestCase(1, 1, ComicPanelFocusMode.Only, true)]
    [TestCase(0, 1, ComicPanelFocusMode.Only, false)]
    public void FocusState_UsesRequestedMode(int panel, int focus, ComicPanelFocusMode mode, bool expected)
    {
        Assert.AreEqual(expected, ComicPanelFocusState.IsEmphasized(panel, focus, mode));
    }
}
