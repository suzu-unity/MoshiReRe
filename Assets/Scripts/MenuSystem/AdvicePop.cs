// Assets/Scripts/MenuSystem/AdvicePop.cs
using UnityEngine;

public class AdvicePop : MonoBehaviour
{
    [SerializeField] private AdviceBubble adviceBubble;
    [TextArea] public string firstMessage = "次は『駅前』に行くといいかも？";
    [Tooltip("true: 一定時間で自動で消える（AdviceBubbleのHold Secondsを使用） / false: 出しっぱ")]
    public bool autoHide = false;

    void Reset()  { TryCache(); }
    void Awake()  { TryCache(); }
    void OnEnable()
    {
        if (!adviceBubble) return;
        adviceBubble.Show(firstMessage, autoHide ? (bool?)true : (bool?)false);
    }

    public void ShowAdvice(string msg, bool? auto = null)
    {
        if (!adviceBubble) return;
        adviceBubble.Show(msg, auto);
    }

    public void UpdateAdvice(string msg)
    {
        if (!adviceBubble) return;
        adviceBubble.UpdateText(msg);
    }

    private void TryCache()
    {
#if UNITY_2022_2_OR_NEWER
        if (!adviceBubble) adviceBubble = FindAnyObjectByType<AdviceBubble>();
#else
        if (!adviceBubble) adviceBubble = FindObjectOfType<AdviceBubble>();
#endif
    }
}