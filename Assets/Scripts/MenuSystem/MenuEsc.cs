using UnityEngine;
using Naninovel;
using Naninovel.UI;

public class MenuEsc : MonoBehaviour
{
    [SerializeField] private string primaryMenuName = "MenuRootV2";
    [SerializeField] private string fallbackMenuName = "MenuRoot";

    private IUIManager ui;
    private IManagedUI menu;

    private System.Collections.IEnumerator Start()
    {
        while (!Engine.Initialized)
            yield return null;

        ui = Engine.GetService<IUIManager>();
        menu = ResolveMenu();
    }

    private void Update()
    {
        if (!Engine.Initialized) return;
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (menu == null)
            menu = ResolveMenu();

        if (menu == null) return;

        if (menu.Visible)
            menu.Hide();
        else
        {
            menu.Show();
            if (menu is MenuRootV2UI menuRootV2)
                menuRootV2.ResetToTop();
        }
    }

    private IManagedUI ResolveMenu()
    {
        if (ui == null)
            return null;

        var resolved = ui.GetUI(primaryMenuName) as IManagedUI;
        if (resolved != null)
            return resolved;

        return ui.GetUI(fallbackMenuName) as IManagedUI;
    }
}
