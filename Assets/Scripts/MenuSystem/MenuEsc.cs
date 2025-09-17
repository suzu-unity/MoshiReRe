using UnityEngine;
using Naninovel;
using Naninovel.UI;

public class MenuToggleInput : MonoBehaviour
{
    IUIManager ui;
    IManagedUI menu;

    private System.Collections.IEnumerator Start()
    {
        while (!Engine.Initialized) yield return null;
        ui = Engine.GetService<IUIManager>();
        // 最初に一回だけ試す
        menu = ui?.GetUI("MenuRoot") as IManagedUI;
        Debug.Log($"[MenuToggleInput] Engine ready. menu found? => {menu != null}");
    }

    void Update()
    {
        if (!Engine.Initialized) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 毎回取り直す（初回遅延に強い）
            if (menu == null)
            {
                menu = ui?.GetUI("MenuRoot") as IManagedUI;
                Debug.Log($"[MenuToggleInput] Try resolve menu => {(menu != null ? "OK" : "NG")}");
            }
            if (menu == null) return;

            if (menu.Visible) { Debug.Log("[MenuToggleInput] Hide Menu"); menu.Hide(); }
            else { Debug.Log("[MenuToggleInput] Show Menu"); menu.Show(); }
        }
    }
}
