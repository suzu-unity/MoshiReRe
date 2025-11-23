using System.Linq;
using Naninovel.UI; // Naninovel の CustomUI を利用するため追加
using UnityEngine;
using UnityEngine.UI;
using TMPro;

 // Naninovel UI マネージャーから管理できるよう CustomUI を継承
public class MenuRootUI : CustomUI
{
    [Header("Pages")]
    [SerializeField] private GameObject pageTop;
    [SerializeField] private InventoryPage pageItems;
    [SerializeField] private CharacterPage pageCharacters;
    [SerializeField] private StatusPage pageStatus;

    [Header("Common UI")]
    [SerializeField] private Button rerePortraitButton;   // ReRe 立ち絵のボタン
    // ★ 吹き出し表示を担当する共通トリガー（AdviceClickTrigger）
    [SerializeField] private AdviceClickTrigger sharedAdviceTrigger;

    [Header("Advice (demo messages)")]
    [TextArea] [SerializeField] private string[] adviceMessages;
    [SerializeField] private bool firstAdviceSticky = true; // 最初のアドバイスを自動で消さないかどうか

    /// <summary>
    /// Awake 時に Naninovel 側の初期化も行うように変更。
    /// </summary>
    protected override void Awake()
    {
        Debug.Log("[MenuRootUI] Awake called.");
        // CustomUI 基底クラスの初期化
        base.Awake();
        
        // Initialize pages with advice trigger if needed
        if (pageItems) pageItems.SetAdviceTrigger(sharedAdviceTrigger);
        if (pageCharacters) pageCharacters.SetAdviceTrigger(sharedAdviceTrigger);
        // StatusPage doesn't use advice trigger yet, but we could add it if needed

        ShowPageTop();

        if (rerePortraitButton)
        {
            rerePortraitButton.onClick.RemoveAllListeners();
            rerePortraitButton.onClick.AddListener(NextAdvice);
        }

        Hide();
    }

    /// <summary>
    /// UI が有効になったときに初回メッセージを表示する。
    /// </summary>
    /// <summary>
    /// UI が有効になったときに初回メッセージを表示する。
    /// </summary>
    protected override void OnEnable()
    {
        Debug.Log("[MenuRootUI] OnEnable called. Resetting to Status Page.");
        
        // 共通背景を表示
        if (commonBackground) commonBackground.SetActive(true);
        
        // デフォルトでステータス画面を開く
        ShowPageStatus();

        // 初回にアドバイスを表示
        base.OnEnable(); // CustomUI の登録処理
        if (sharedAdviceTrigger && adviceMessages != null && adviceMessages.Length > 0)
        {
            // firstAdviceSticky が true のときは autoHide を false にする
            bool autoHide = !firstAdviceSticky;
            sharedAdviceTrigger.ShowAdvice(adviceMessages[0], autoHide);
        }
    }

    /// <summary>
    /// UI が非表示になるときの後始末。
    /// </summary>
    protected override void OnDisable()
    {
        Debug.Log("[MenuRootUI] OnDisable called.");
        base.OnDisable(); // CustomUI の登録解除
        if (sharedAdviceTrigger) sharedAdviceTrigger.HideAdvice();
        if (rerePortraitButton) rerePortraitButton.onClick.RemoveListener(NextAdvice);
    }

    /// <summary>
    /// UI マネージャーから表示が要求されたときにトップページを表示する。
    /// </summary>
    [SerializeField] private GameObject commonBackground; // スマホ枠などの共通背景

    // Show() override is apparently not called by IManagedUI.Show() extension/interface in some cases,
    // so we moved the logic to OnEnable. We keep this just in case.
    public override void Show ()
    {
        Debug.Log("[MenuRootUI] Show() called (Manual).");
        base.Show();
        OpenMenu();
    }

    /// <summary>
    /// Explicitly open the menu and reset state.
    /// Called from MenuToggleInput to ensure initialization runs even if Show() override is bypassed.
    /// </summary>
    public void OpenMenu()
    {
        Debug.Log($"[MenuRootUI] OpenMenu called. Resetting to Status Page. commonBackground assigned? {commonBackground != null}");
        if (commonBackground) 
        {
            commonBackground.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[MenuRootUI] commonBackground is NOT assigned! If the background is part of PageTop, it might be hidden.");
        }
        
        ShowPageStatus();
    }

    /// <summary>
    /// ReReの立ち絵をクリックしたときに呼ばれる。メッセージをランダムに表示する。
    /// </summary>
    public void NextAdvice()
    {
        if (adviceMessages == null || adviceMessages.Length == 0 || sharedAdviceTrigger == null) return;

        // ランダムなインデックスを取得して表示
        int index = Random.Range(0, adviceMessages.Length);
        sharedAdviceTrigger.ShowAdvice(adviceMessages[index], true);
    }

    // ===== Top / Items / Characters ページ切替 =====
    public void ShowPageTop()
    {
        if (pageTop) pageTop.SetActive(true);
        if (pageItems) pageItems.Hide();
        if (pageCharacters) pageCharacters.Hide();
        if (pageStatus) pageStatus.Hide();
    }

    public void ShowPageItems()
    {
        if (pageTop) pageTop.SetActive(false);
        if (pageItems) pageItems.Show();
        if (pageCharacters) pageCharacters.Hide();
        if (pageStatus) pageStatus.Hide();
    }

    public void ShowPageCharacters()
    {
        if (pageTop) pageTop.SetActive(false);
        if (pageItems) pageItems.Hide();
        if (pageCharacters) pageCharacters.Show();
        if (pageStatus) pageStatus.Hide();
    }

    public void ShowPageStatus()
    {
        if (pageTop) pageTop.SetActive(false);
        if (pageItems) pageItems.Hide();
        if (pageCharacters) pageCharacters.Hide();
        if (pageStatus) pageStatus.Show();
    }

    public void OnOjToggleChanged(bool isOn)
    {
        if (!isOn) return;
        // If we had filtering logic, we would pass it to pageCharacters here
        if (pageCharacters) pageCharacters.Show();
    }

    public void OnItadakiToggleChanged(bool isOn)
    {
        if (!isOn) return;
        if (pageCharacters) pageCharacters.Show();
    }
}
