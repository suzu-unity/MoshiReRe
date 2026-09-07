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
            hover.SendMessage("Awake");
            button.transform.localScale *= 1.06f;
            hover.SendMessage("OnDisable");
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
            hover.SendMessage("OnDisable");
            Assert.That(button.transform.localScale, Is.EqualTo(Vector3.one));
        }
        finally { Object.DestroyImmediate(button); }
    }
}
