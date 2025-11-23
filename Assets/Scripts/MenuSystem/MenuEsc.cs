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
                if (menu != null) Debug.Log($"[MenuToggleInput] Menu Type: {menu.GetType().Name}");
            }
            if (menu == null) return;

            if (menu.Visible) 
            { 
                Debug.Log($"[MenuToggleInput] Hide Menu. Instance Type: {menu.GetType().Name}"); 
                menu.Hide(); 
            }
            else 
            { 
                Debug.Log($"[MenuToggleInput] Show Menu. Instance Type: {menu.GetType().Name}"); 
                
                // Try to cast to MenuRootUI to call OpenMenu explicitly
                if (menu is MenuRootUI rootUI)
                {
                    rootUI.Show(); // Or rootUI.OpenMenu() if Show() is still not working, but Show() now calls OpenMenu()
                    // If Show() is not virtual/overridden correctly in the interface, calling it on the concrete type works.
                }
                else
                {
                    menu.Show(); 
                }
            }
        }
    }
}
