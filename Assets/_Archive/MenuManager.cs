using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Auto-wire settings")]
    [SerializeField] private string menuRootName = "MenuRoot";           // シーン上に実体があれば拾う
    [SerializeField] private string menuRootResourcesPath = "UI/MenuRoot"; // なければ Resources から生成
    [SerializeField] private string dimObjectName = "Dim";               // 黒板オブジェクト名
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

    // ランタイムでセットする参照（インスペクタ未使用）
    private GameObject menuRoot;
    private CanvasGroup menuGroup;
    private GameObject dim;
    private AdviceBubble adviceBubble;
    private bool isOpen;

    void Awake ()
    {
        // 1) まずシーン上で探す
        var rootGo = GameObject.Find(menuRootName);

        // 2) 無ければ Resources から生成
        if (!rootGo)
        {
            var prefab = Resources.Load<GameObject>(menuRootResourcesPath);
            if (prefab)
            {
                rootGo = Instantiate(prefab);
                rootGo.name = menuRootName; // (Clone)を消しつつ後続のFindにも対応
            }
            else
            {
                Debug.LogError($"[MenuManager] MenuRoot not found in scene and Resources.Load failed: {menuRootResourcesPath}");
                return;
            }
        }

        // 3) 必要コンポーネントをオート配線
        menuRoot = rootGo;
        menuGroup = menuRoot.GetComponent<CanvasGroup>() ?? menuRoot.AddComponent<CanvasGroup>();
        adviceBubble = menuRoot.GetComponentInChildren<AdviceBubble>(true);
        // Dim は名前で子から検索（任意）
        var dimTr = menuRoot.transform.Find(dimObjectName);
        dim = dimTr ? dimTr.gameObject : null;
        if (dim)
        {
            var img = dim.GetComponent<Image>() ?? dim.AddComponent<Image>();
            img.raycastTarget = true; // クリック貫通防止
        }

        // 4) 起動時は閉じる
        SetOpen(false, true);
    }

    void Update ()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (isOpen) Close();
            else Open();
        }
    }

    public void Open ()
    {
        SetOpen(true, false);
    }

    public void Close ()
    {
        SetOpen(false, false);
    }

    private void SetOpen (bool open, bool instant)
    {
        if (!menuRoot || !menuGroup)
        {
            Debug.LogWarning("[MenuManager] menuRoot/menuGroup not ready.");
            return;
        }

        isOpen = open;

        if (open)
        {
            if (dim) dim.SetActive(true);
            menuRoot.SetActive(true);

            menuGroup.alpha = 1f;
            menuGroup.interactable = true;
            menuGroup.blocksRaycasts = true;

            if (adviceBubble) adviceBubble.Show("次は『駅前』に行くといいかも？");

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            menuGroup.alpha = 0f;
            menuGroup.interactable = false;
            menuGroup.blocksRaycasts = false;

            if (dim) dim.SetActive(false);
            // menuRoot.SetActive(false); // 好みで

            if (adviceBubble) adviceBubble.Hide();

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        Debug.Log($"[MenuManager] {(open ? "OPEN" : "CLOSE")} active={menuRoot.activeSelf} alpha={menuGroup.alpha} interactable={menuGroup.interactable} blocks={menuGroup.blocksRaycasts}");
    }
}