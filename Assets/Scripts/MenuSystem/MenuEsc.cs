using UnityEngine;
using Naninovel;
using Naninovel.UI;

public class MenuEsc : MonoBehaviour
{
    [SerializeField] private string primaryMenuName = "MenuRootV2";
    [SerializeField] private string fallbackMenuName = "MenuRoot";
    [SerializeField, Tooltip("Optional exploration player paused while the menu is visible.")]
    private MonoBehaviour explorationPlayer;
    [SerializeField, Tooltip("Set when an Input System bridge forwards Escape for this menu.")]
    private bool externalInputBridge;

    private IUIManager ui;
    private IManagedUI menu;
    private bool movementEnabledBeforeMenu;
    private bool movementOverridden;
    private bool subscribedToMenu;

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
        if (externalInputBridge || !WasMenuPressed()) return;

        ToggleMenu();
    }

    public void ToggleMenu()
    {
        if (!Engine.Initialized)
            return;

        if (menu == null)
            menu = ResolveMenu();

        if (menu == null) return;

        SubscribeToMenu();

        if (menu.Visible)
            menu.Hide();
        else
        {
            menu.Show();
            if (menu is MenuRootV2UI menuRootV2)
                menuRootV2.ResetToTop();
        }
    }

    private void OnDestroy()
    {
        if (subscribedToMenu && menu != null)
            menu.OnVisibilityChanged -= HandleMenuVisibilityChanged;

        if (movementOverridden)
            SetExplorationMovementEnabled(movementEnabledBeforeMenu);
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

    private bool WasMenuPressed()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.Escape);
#else
        return false;
#endif
    }

    private void SubscribeToMenu()
    {
        if (subscribedToMenu || menu == null)
            return;

        menu.OnVisibilityChanged += HandleMenuVisibilityChanged;
        subscribedToMenu = true;
    }

    private void HandleMenuVisibilityChanged(bool visible)
    {
        if (explorationPlayer == null)
            return;

        if (visible)
        {
            movementEnabledBeforeMenu = GetExplorationMovementEnabled();
            movementOverridden = true;
            SetExplorationMovementEnabled(false);
        }
        else
        {
            SetExplorationMovementEnabled(movementEnabledBeforeMenu);
            movementOverridden = false;
        }
    }

    private bool GetExplorationMovementEnabled()
    {
        var property = explorationPlayer.GetType().GetProperty("MovementEnabled");
        return property?.PropertyType == typeof(bool) && property.GetValue(explorationPlayer) is bool value && value;
    }

    private void SetExplorationMovementEnabled(bool enabled)
    {
        explorationPlayer?.SendMessage("SetMovementEnabled", enabled, SendMessageOptions.DontRequireReceiver);
    }
}
