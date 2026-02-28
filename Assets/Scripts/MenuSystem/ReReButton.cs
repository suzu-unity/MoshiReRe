using UnityEngine;
using UnityEngine.UI;
using Naninovel;

/// <summary>
/// ReRe ボタン：シナリオ進行度に応じて画像が変わり、クリックでメッセージ表示
/// </summary>
public class ReReButton : MonoBehaviour
{
    [Header("Images")]
    [SerializeField] private Image reReImage;
    [SerializeField] private Sprite[] stateSprites; // シナリオ進行度別の画像配列
    [SerializeField] private Sprite hoverSprite;    // ホバー時の画像（進行度によらない）

    [Header("Message")]
    [SerializeField] private AdviceBubble adviceBubble; // メッセージ表示用（吹き出し）

    private int currentState = 0;
    private Sprite currentStateSprite; // 現在の進行度に対応した画像
    private bool isHovering = false;
    private ICustomVariableManager variableManager;
    private string currentMessage = ""; // 現在設定されているメッセージ
    private string previousMessage = ""; // 前回設定されたメッセージ（クリック時に表示）

    private void Start()
    {
        variableManager = Engine.GetService<ICustomVariableManager>();

        var button = GetComponent<Button>();
        if (button)
        {
            button.onClick.AddListener(OnButtonClick);
            Debug.Log("[ReReButton.Start] Button listener added");
        }
        else
        {
            Debug.LogWarning("[ReReButton.Start] No Button component found!");
        }

        var pointerHandler = GetComponent<UIPointerEvents>();
        if (!pointerHandler) pointerHandler = gameObject.AddComponent<UIPointerEvents>();

        pointerHandler.onEnter = OnPointerEnter;
        pointerHandler.onExit = OnPointerExit;

        UpdateImageBasedOnState();

        // デバッグ情報
        Debug.Log($"[ReReButton.Start] AdviceBubble assigned: {adviceBubble != null}");
        if (adviceBubble != null)
        {
            Debug.Log($"[ReReButton.Start] AdviceBubble name: {adviceBubble.gameObject.name}");
            Debug.Log($"[ReReButton.Start] AdviceBubble active: {adviceBubble.gameObject.activeSelf}");
        }
    }

    private void Update()
    {
        // シナリオ進行度を確認して画像を更新
        UpdateStateFromVariable();
    }

    /// <summary>
    /// Naninovel 変数から現在の進行度を取得して画像を更新
    /// </summary>
    private void UpdateStateFromVariable()
    {
        if (variableManager == null) return;

        try
        {
            var stateValue = variableManager.GetVariableValue("rereState");
            if (stateValue != null && int.TryParse(stateValue.ToString(), out int newState))
            {
                if (newState != currentState)
                {
                    currentState = newState;
                    if (!isHovering)
                    {
                        UpdateImageBasedOnState();
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            // rereState 変数がまだ定義されていない場合、state=0 を使用
            if (currentState != 0)
            {
                currentState = 0;
                if (!isHovering)
                {
                    UpdateImageBasedOnState();
                }
            }
            // Debug.LogWarning($"[ReReButton] rereState not found: {ex.Message}");
        }
    }

    /// <summary>
    /// 現在の進行度に対応した画像に更新
    /// </summary>
    private void UpdateImageBasedOnState()
    {
        if (reReImage == null) return;

        if (currentState >= 0 && currentState < stateSprites.Length)
        {
            currentStateSprite = stateSprites[currentState];
            reReImage.sprite = currentStateSprite;
            Debug.Log($"[ReReButton] Updated image to state {currentState}");
        }
        else
        {
            Debug.LogWarning($"[ReReButton] State {currentState} is out of range for stateSprites array");
        }
    }

    /// <summary>
    /// ボタンクリック時：現在設定されたメッセージを表示
    /// </summary>
    private void OnButtonClick()
    {
        Debug.Log("[ReReButton] Clicked!");
        if (!string.IsNullOrEmpty(currentMessage))
        {
            ShowMessage(currentMessage);
            Debug.Log($"[ReReButton] Showing current message: {currentMessage}");
        }
        else
        {
            Debug.Log("[ReReButton] No message to display");
        }
    }

    /// <summary>
    /// マウスホバー時
    /// </summary>
    private void OnPointerEnter()
    {
        if (reReImage == null) return;

        isHovering = true;
        if (hoverSprite != null)
        {
            reReImage.sprite = hoverSprite;
            Debug.Log("[ReReButton] Mouse enter - showing hover sprite");
        }
    }

    /// <summary>
    /// マウスホバー終了時
    /// </summary>
    private void OnPointerExit()
    {
        isHovering = false;
        UpdateImageBasedOnState();
        Debug.Log("[ReReButton] Mouse exit - restored state sprite");
    }

    /// <summary>
    /// シナリオから呼び出される：メッセージを吹き出しで表示
    /// </summary>
    public void ShowMessage(string message)
    {
        Debug.Log($"[ReReButton.ShowMessage] Called with message: {message}");
        Debug.Log($"[ReReButton.ShowMessage] AdviceBubble is null: {adviceBubble == null}");

        if (adviceBubble != null)
        {
            Debug.Log($"[ReReButton.ShowMessage] AdviceBubble active before Show: {adviceBubble.gameObject.activeSelf}");
            adviceBubble.Show(message, autoHide: true);
            Debug.Log($"[ReReButton.ShowMessage] AdviceBubble.Show() called");
            Debug.Log($"[ReReButton.ShowMessage] AdviceBubble active after Show: {adviceBubble.gameObject.activeSelf}");
        }
        else
        {
            Debug.LogError("[ReReButton.ShowMessage] AdviceBubble is not assigned!");
        }
    }

    /// <summary>
    /// ReReMessage コマンドから呼び出される：メッセージを設定
    /// クリック時に currentMessage が表示される（前回のメッセージは previousMessage に保存される）
    /// </summary>
    public void SetCurrentMessage(string message)
    {
        // 前回のメッセージを保存
        previousMessage = currentMessage;

        // 新しいメッセージを設定
        currentMessage = message;

        Debug.Log($"[ReReButton] Message set: {message}");
        Debug.Log($"[ReReButton] Previous message saved: {previousMessage}");
    }
}
