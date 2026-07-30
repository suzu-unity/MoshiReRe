using UnityEngine;
using UnityEngine.InputSystem;

namespace MoshiReRe.Exploration
{
    /// <summary>Forwards the Input System Escape key to the menu owned by the UI assembly.</summary>
    [DisallowMultipleComponent]
    public sealed class ExplorationMenuInputBridge : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour menuEsc;

        private void Update()
        {
            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
                menuEsc?.SendMessage("ToggleMenu", SendMessageOptions.DontRequireReceiver);
        }
    }
}
