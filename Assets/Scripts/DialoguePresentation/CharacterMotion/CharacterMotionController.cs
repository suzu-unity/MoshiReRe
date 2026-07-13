using UnityEngine;

namespace MoshiReRe.DialoguePresentation.CharacterMotion
{
    [DisallowMultipleComponent]
    public sealed class CharacterMotionController : MonoBehaviour
    {
        public static CharacterMotionController Instance { get; private set; }

        [Tooltip("Optional override. When empty, the generated Resources library is used.")]
        [SerializeField] private CharacterMotionLibrary library;

        public CharacterMotionLibrary Library => library;

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                enabled = false;
                return;
            }

            Instance = this;
        }

        private void OnDisable()
        {
            if (Instance != this) return;
            CharacterMotionRuntime.CancelAll();
            Instance = null;
        }
    }
}
