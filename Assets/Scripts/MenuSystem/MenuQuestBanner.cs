using TMPro;
using UnityEngine;
public sealed class MenuQuestBanner : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text objective;
    private void OnEnable() { MainQuestState.OnChanged += Refresh; Refresh(MainQuestState.Current); }
    private void OnDisable() => MainQuestState.OnChanged -= Refresh;
    private void Refresh(MainQuestState.Data data)
    {
        if (!data.IsAssigned) return;
        if (title) title.text = data.Title;
        if (objective) objective.text = data.Objective;
    }
}
