using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ClickableAdvice : MonoBehaviour
{
    [TextArea] public string message;
    public bool autoHide = false;

    [SerializeField] private AdviceClickTrigger adviceTrigger;

    private void Awake()
    {
        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClick);
    }

    public void SetTrigger(AdviceClickTrigger trigger) => adviceTrigger = trigger;

    private void OnClick()
    {
        if (!adviceTrigger) return;
        adviceTrigger.ShowAdvice(message, autoHide);
    }
}