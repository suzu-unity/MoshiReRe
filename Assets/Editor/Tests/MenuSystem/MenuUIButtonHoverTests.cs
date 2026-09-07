using NUnit.Framework;
using UnityEngine;

public class MenuUIButtonHoverTests
{
    [Test]
    public void DisabledHoveredButton_RestoresOriginalScale()
    {
        var button = new GameObject("HoverTest", typeof(RectTransform));
        try
        {
            button.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            var hover = button.AddComponent<MenuUIButtonHover>();
            // Plain EditMode tests do not run MonoBehaviour.Awake automatically.
            InvokeLifecycle(hover, "Awake");
            button.transform.localScale *= 1.06f;
            InvokeLifecycle(hover, "OnDisable");
            Assert.That(button.transform.localScale, Is.EqualTo(new Vector3(0.9f, 0.9f, 1f)));
        }
        finally { Object.DestroyImmediate(button); }
    }

    [Test]
    public void EditorDisableBeforeAwake_DoesNotCollapseButton()
    {
        var button = new GameObject("BuilderTest", typeof(RectTransform));
        try
        {
            var hover = button.AddComponent<MenuUIButtonHover>();
            InvokeLifecycle(hover, "OnDisable");
            Assert.That(button.transform.localScale, Is.EqualTo(Vector3.one));
        }
        finally { Object.DestroyImmediate(button); }
    }

    private static void InvokeLifecycle(MenuUIButtonHover hover, string method)
    {
        typeof(MenuUIButtonHover).GetMethod(method, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(hover, null);
    }
}
