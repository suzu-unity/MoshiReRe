using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ComicPanelLayout))]
public sealed class ComicPanelLayoutEditor : Editor
{
    private SerializedProperty layoutId;
    private SerializedProperty panels;
    private bool[] expanded = new bool[0];

    private void OnEnable()
    {
        layoutId = serializedObject.FindProperty("layoutId");
        panels = serializedObject.FindProperty("panels");
        expanded = new bool[panels.arraySize];
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(layoutId, new GUIContent("Layout ID"));
        EditorGUILayout.Space(4f);

        EnsureExpandedSize();
        for (var i = 0; i < panels.arraySize; i++)
            DrawPanel(i, panels.GetArrayElementAtIndex(i));

        EditorGUILayout.Space(4f);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Add Panel"))
            {
                panels.arraySize++;
                EnsureExpandedSize();
                expanded[panels.arraySize - 1] = true;
                EnsureDefaultVertices(panels.GetArrayElementAtIndex(panels.arraySize - 1));
            }

            using (new EditorGUI.DisabledScope(panels.arraySize == 0))
            {
                if (GUILayout.Button("Remove Last"))
                {
                    panels.arraySize--;
                    EnsureExpandedSize();
                }
            }
        }

        if (serializedObject.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(target);
            SceneView.RepaintAll();
        }
    }

    private void DrawPanel(int index, SerializedProperty panel)
    {
        var id = panel.FindPropertyRelative("id");
        expanded[index] = EditorGUILayout.Foldout(expanded[index], $"Panel {index + 1}: {id.stringValue}", true);
        if (!expanded[index])
            return;

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(id, new GUIContent("ID"));
        EditorGUILayout.PropertyField(panel.FindPropertyRelative("image"));
        EditorGUILayout.PropertyField(panel.FindPropertyRelative("vertices"), new GUIContent("Normalized Vertices"), true);
        EditorGUILayout.PropertyField(panel.FindPropertyRelative("emphasizedColor"));
        EditorGUILayout.PropertyField(panel.FindPropertyRelative("emphasizedDarkness"));
        EditorGUILayout.PropertyField(panel.FindPropertyRelative("nonEmphasizedColor"));
        EditorGUILayout.PropertyField(panel.FindPropertyRelative("nonEmphasizedDarkness"));
        EditorGUILayout.PropertyField(panel.FindPropertyRelative("transitionSeconds"));
        EditorGUILayout.PropertyField(panel.FindPropertyRelative("estimatedScenarioLine"));
        EditorGUI.indentLevel--;
    }

    private void EnsureExpandedSize()
    {
        if (expanded.Length == panels.arraySize)
            return;
        var resized = new bool[panels.arraySize];
        for (var i = 0; i < Mathf.Min(expanded.Length, resized.Length); i++)
            resized[i] = expanded[i];
        expanded = resized;
    }

    private static void EnsureDefaultVertices(SerializedProperty panel)
    {
        var vertices = panel.FindPropertyRelative("vertices");
        if (vertices.arraySize >= 3)
            return;
        vertices.arraySize = 4;
        vertices.GetArrayElementAtIndex(0).vector2Value = new Vector2(0f, 0f);
        vertices.GetArrayElementAtIndex(1).vector2Value = new Vector2(1f, 0f);
        vertices.GetArrayElementAtIndex(2).vector2Value = new Vector2(1f, 1f);
        vertices.GetArrayElementAtIndex(3).vector2Value = new Vector2(0f, 1f);
    }
}
