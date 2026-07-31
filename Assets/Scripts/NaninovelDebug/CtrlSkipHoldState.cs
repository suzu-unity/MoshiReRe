#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace MoshiReRe.NaninovelDebug
{
    /// <summary>Tracks the aggregate state of the left and right Ctrl keys.</summary>
    public sealed class CtrlSkipHoldState
    {
        public enum Transition
        {
            None,
            Began,
            Ended
        }

        private bool ctrlHeld;

        public Transition Update(bool leftCtrlHeld, bool rightCtrlHeld)
        {
            var heldNow = leftCtrlHeld || rightCtrlHeld;
            if (heldNow == ctrlHeld)
                return Transition.None;

            ctrlHeld = heldNow;
            return heldNow ? Transition.Began : Transition.Ended;
        }

        public Transition Release()
        {
            if (!ctrlHeld)
                return Transition.None;

            ctrlHeld = false;
            return Transition.Ended;
        }
    }
}
#endif
