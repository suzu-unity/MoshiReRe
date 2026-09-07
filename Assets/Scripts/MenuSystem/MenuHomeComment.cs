using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MenuHomeComment : MonoBehaviour
{
    [SerializeField] private Button characterButton;
    [SerializeField] private GameObject bubble;
    [SerializeField] private TMP_Text text;
    [SerializeField] private string[] comments = { "あ、来てたんだ。", "……もう少しだけ、読書。", "今夜の準備、しよっか。" };
    private Coroutine hideRoutine;
    private int nextComment;

    private void Awake() { if (characterButton) characterButton.onClick.AddListener(Tap); }
    private void OnEnable() { if (bubble) bubble.SetActive(false); }
    private void OnDisable() { if (hideRoutine != null) StopCoroutine(hideRoutine); hideRoutine = null; }
    private void OnDestroy() { if (characterButton) characterButton.onClick.RemoveListener(Tap); }
    public void Tap()
    {
        if (comments == null || comments.Length == 0) return;
        ShowComment(comments[nextComment++ % comments.Length]);
    }
    public void ShowComment(string comment)
    {
        if (hideRoutine != null) StopCoroutine(hideRoutine);
        if (text) text.text = comment ?? "";
        if (bubble) bubble.SetActive(!string.IsNullOrWhiteSpace(comment));
        hideRoutine = string.IsNullOrWhiteSpace(comment) ? null : StartCoroutine(HideLater());
    }
    private IEnumerator HideLater()
    {
        yield return new WaitForSecondsRealtime(4.5f);
        if (bubble) bubble.SetActive(false);
        hideRoutine = null;
    }
}
