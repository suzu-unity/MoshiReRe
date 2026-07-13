using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ComicPanelController))]
public sealed class ComicPanelControllerEditor : Editor
{
    private SerializedProperty layout;
    private SerializedProperty selectedPanel;
    private SerializedProperty buildInEditMode;

    private void OnEnable()
    {
        layout = serializedObject.FindProperty("layout");
        selectedPanel = serializedObject.FindProperty("selectedPanel");
        buildInEditMode = serializedObject.FindProperty("buildInEditMode");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(layout, new GUIContent("Layout"));
        EditorGUILayout.PropertyField(buildInEditMode, new GUIContent("Preview in Edit Mode"));

        var controller = (ComicPanelController)target;
        if (controller.Layout != null && controller.Layout.Panels != null && controller.Layout.Panels.Count > 0)
        {
            var names = new string[controller.Layout.Panels.Count];
            for (var i = 0; i < names.Length; i++)
                names[i] = $"{i + 1}: {controller.Layout.Panels[i].Id}";
            selectedPanel.intValue = EditorGUILayout.Popup("Scene Handle Panel", Mathf.Clamp(selectedPanel.intValue, 0, names.Length - 1), names);
            EditorGUILayout.HelpBox("Scene viewで頂点ハンドルをドラッグできます。コマの形はLayoutアセットにも保存されます。", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("Layoutを割り当てるとコマを編集できます。", MessageType.Info);
        }

        if (serializedObject.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(controller);
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI()
    {
        var controller = (ComicPanelController)target;
        var currentLayout = controller.Layout;
        if (currentLayout == null || currentLayout.Panels == null)
            return;

        var rectTransform = controller.GetComponent<RectTransform>();
        var rect = rectTransform.rect;
        for (var panelIndex = 0; panelIndex < currentLayout.Panels.Count; panelIndex++)
        {
            var panel = currentLayout.Panels[panelIndex];
            var points = panel.SafeVertices;
            var worldPoints = new Vector3[points.Count + 1];
            for (var i = 0; i < points.Count; i++)
                worldPoints[i] = rectTransform.TransformPoint(ToLocal(rect, points[i]));
            worldPoints[points.Count] = worldPoints[0];

            Handles.color = panelIndex == controller.SelectedPanel ? new Color(1f, 0.65f, 0.1f, 1f) : new Color(0.2f, 0.8f, 1f, 0.75f);
            Handles.DrawAAPolyLine(panelIndex == controller.SelectedPanel ? 4f : 2f, worldPoints);
            Handles.Label(worldPoints[0], $"{panelIndex + 1}: {panel.Id}");

            if (panelIndex != controller.SelectedPanel)
                continue;

            for (var vertexIndex = 0; vertexIndex < points.Count; vertexIndex++)
            {
                var world = worldPoints[vertexIndex];
                EditorGUI.BeginChangeCheck();
                var moved = Handles.PositionHandle(world, Quaternion.identity);
                if (!EditorGUI.EndChangeCheck())
                    continue;

                Undo.RecordObject(currentLayout, "Move Comic Panel Vertex");
                var local = rectTransform.InverseTransformPoint(moved);
                var normalized = new Vector2(
                    Mathf.InverseLerp(rect.xMin, rect.xMax, local.x),
                    Mathf.InverseLerp(rect.yMin, rect.yMax, local.y));
                panel.vertices[vertexIndex] = normalized;
                EditorUtility.SetDirty(currentLayout);
                SceneView.RepaintAll();
            }
        }
    }

    private static Vector3 ToLocal(Rect rect, Vector2 normalized)
    {
        return new Vector3(
            Mathf.Lerp(rect.xMin, rect.xMax, normalized.x),
            Mathf.Lerp(rect.yMin, rect.yMax, normalized.y),
            0f);
    }
}
