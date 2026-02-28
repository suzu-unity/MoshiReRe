using UnityEngine;
using UnityEngine.UI;

public class CheckMenuRootVerticalInstance
{
    public static void Execute()
    {
        GameObject menuRoot = GameObject.Find("MenuRootVertical");
        if (menuRoot == null)
        {
            Debug.LogError("MenuRootVertical not found in the scene.");
            return;
        }

        Canvas canvas = menuRoot.GetComponent<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"Canvas RenderMode: {canvas.renderMode}");
            Debug.Log($"Canvas PixelPerfect: {canvas.pixelPerfect}");
        }
        else
        {
            Debug.LogError("Canvas component missing on MenuRootVertical.");
        }

        RectTransform rect = menuRoot.GetComponent<RectTransform>();
        if (rect != null)
        {
            Debug.Log($"Root Anchors: Min{rect.anchorMin} Max{rect.anchorMax}");
            Debug.Log($"Root Offsets: Min{rect.offsetMin} Max{rect.offsetMax}");
        }

        CanvasGroup group = menuRoot.GetComponent<CanvasGroup>();
        if (group != null)
        {
            Debug.Log($"CanvasGroup Alpha: {group.alpha}");
            Debug.Log($"CanvasGroup Interactable: {group.interactable}");
            Debug.Log($"CanvasGroup BlocksRaycasts: {group.blocksRaycasts}");
        }
        else
        {
            Debug.LogError("CanvasGroup component missing on MenuRootVertical.");
        }

        Transform safeArea = menuRoot.transform.Find("SafeAreaLayout");
        if (safeArea != null)
        {
            RectTransform safeAreaRect = safeArea.GetComponent<RectTransform>();
            if (safeAreaRect != null)
            {
                Debug.Log($"SafeArea Anchors: Min{safeAreaRect.anchorMin} Max{safeAreaRect.anchorMax}");
                Debug.Log($"SafeArea Offsets: Min{safeAreaRect.offsetMin} Max{safeAreaRect.offsetMax}");
            }
        }
        else
        {
            Debug.LogError("SafeAreaLayout child not found.");
        }
    }
}
