using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Comic Panel/Panel Layout", fileName = "ComicPanelLayout")]
public sealed class ComicPanelLayout : ScriptableObject
{
    [Tooltip("@comicShow id: に指定するID。Resourcesフォルダ配下に置く場合はResourcesからの相対パスも使用できます。")]
    [SerializeField] private string layoutId = "ComicPanelLayout";
    [SerializeField] private List<ComicPanelDefinition> panels = new List<ComicPanelDefinition>();

    public string LayoutId => layoutId;
    public List<ComicPanelDefinition> Panels => panels;

    public int FindPanelIndex(string panelId)
    {
        if (string.IsNullOrWhiteSpace(panelId) || panels == null)
            return -1;

        for (var i = 0; i < panels.Count; i++)
        {
            if (string.Equals(panels[i].Id, panelId, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    public bool MatchesId(string id)
    {
        return string.IsNullOrWhiteSpace(id)
            || string.Equals(layoutId, id, StringComparison.OrdinalIgnoreCase)
            || string.Equals(name, id, StringComparison.OrdinalIgnoreCase);
    }

#if UNITY_EDITOR
    public void SetLayoutIdForEditor(string value)
    {
        layoutId = value;
    }
#endif
}

[Serializable]
public sealed class ComicPanelDefinition
{
    [Tooltip("InspectorとコマID指定で使う一意なID")]
    public string id = "panel";

    [Tooltip("コマに表示するSprite。差し替え可能です。")]
    public Sprite image;

    [Tooltip("親RectTransform内の正規化座標。左下が(0,0)、右上が(1,1)。時計回り/反時計回りどちらでも可。")]
    public List<Vector2> vertices = new List<Vector2>
    {
        new Vector2(0f, 0f),
        new Vector2(1f, 0f),
        new Vector2(1f, 1f),
        new Vector2(0f, 1f)
    };

    [Header("Emphasized")]
    public Color emphasizedColor = Color.white;
    [Range(0f, 1f)] public float emphasizedDarkness;

    [Header("Non-emphasized")]
    public Color nonEmphasizedColor = Color.white;
    [Range(0f, 1f)] public float nonEmphasizedDarkness = 0.68f;

    [Min(0f), Tooltip("色が切り替わる秒数。0で即時切り替え。")]
    public float transitionSeconds = 0.18f;

    [Tooltip("整理用の目安行。実行判定には使わず、Naninovelの明示コマンドで操作します。")]
    public int estimatedScenarioLine = -1;

    public string Id => string.IsNullOrWhiteSpace(id) ? "panel" : id;

    public IReadOnlyList<Vector2> SafeVertices => ComicPanelGeometry.SanitizeVertices(vertices);

    public Color GetColor(bool emphasized)
    {
        var source = emphasized ? emphasizedColor : nonEmphasizedColor;
        var darkness = Mathf.Clamp01(emphasized ? emphasizedDarkness : nonEmphasizedDarkness);
        var brightness = 1f - darkness;
        return new Color(source.r * brightness, source.g * brightness, source.b * brightness, source.a);
    }
}
