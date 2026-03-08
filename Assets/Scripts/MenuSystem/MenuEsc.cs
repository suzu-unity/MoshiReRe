using UnityEngine;
using Naninovel;
using Naninovel.UI;

public class MenuEsc : MonoBehaviour
{
    private IUIManager ui;
    private IManagedUI menu;

    private System.Collections.IEnumerator Start()
    {
        while (!Engine.Initialized)
            yield return null;

        ui = Engine.GetService<IUIManager>();
        menu = ui?.GetUI("MenuRoot") as IManagedUI;
    }

    private void Update()
    {
        if (!Engine.Initialized) return;
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (menu == null)
            menu = ui?.GetUI("MenuRoot") as IManagedUI;

        if (menu == null) return;

        if (menu.Visible)
            menu.Hide();
        else
            menu.Show();
    }
}
