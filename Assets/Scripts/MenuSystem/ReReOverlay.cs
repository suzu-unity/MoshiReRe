using UnityEngine;
using UnityEngine.UI;
using Naninovel;
using Naninovel.UI;

/// <summary>
/// ノベル本編のテキストウィンドウ上に常駐するReReのオーバーレイUI。
/// Naninovel の CustomUI として Resources/UI/ReReOverlay.prefab に置く。
/// テキストプリント開始/終了イベントに同期する。
/// </summary>
public class ReReOverlay : CustomUI
{
    [SerializeField] private Button rereButton;
    [SerializeField] private AdviceBubble adviceBubble;

    private string currentAdvice = "";
    private ITextPrinterManager printerManager;

    protected override void Awake()
    {
        base.Awake();
        if (rereButton)
            rereButton.onClick.AddListener(OnReReClicked);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (Engine.Initialized)
            SetupPrinterTracking();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (printerManager != null)
        {
            printerManager.OnPrintStarted -= OnPrintStarted;
            printerManager.OnPrintFinished -= OnPrintFinished;
        }
    }

    private void SetupPrinterTracking()
    {
        printerManager = Engine.GetService<ITextPrinterManager>();
        if (printerManager != null)
        {
            printerManager.OnPrintStarted += OnPrintStarted;
            printerManager.OnPrintFinished += OnPrintFinished;
        }
    }

    private void OnPrintStarted(PrintMessageArgs args)
    {
        // テキスト出力開始 → ReRe表示（フェード無し即座）
        gameObject.SetActive(true);
        if (CanvasGroup) CanvasGroup.alpha = 1f;
    }

    private void OnPrintFinished(PrintMessageArgs args)
    {
        // テキスト出力終了 → ReRe非表示（即座に非アクティブ化）
        gameObject.SetActive(false);
    }

    /// <summary>
    /// @rere コマンドから次のアドバイス文をセットする。
    /// </summary>
    public void SetAdvice(string text)
    {
        currentAdvice = text;
    }

    /// <summary>
    /// アドバイス吹き出しを表示する（ReReButton などから呼び出される）
    /// </summary>
    public void ShowAdviceBubble()
    {
        OnReReClicked();
    }

    private void OnReReClicked()
    {
        if (adviceBubble == null) return;
        if (string.IsNullOrEmpty(currentAdvice)) return;
        adviceBubble.Show(currentAdvice, autoHide: true);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (rereButton)
            rereButton.onClick.RemoveListener(OnReReClicked);
        if (printerManager != null)
        {
            printerManager.OnPrintStarted -= OnPrintStarted;
            printerManager.OnPrintFinished -= OnPrintFinished;
        }
    }
}
