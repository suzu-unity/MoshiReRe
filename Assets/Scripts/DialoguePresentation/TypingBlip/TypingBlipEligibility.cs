namespace MoshiReRe.DialoguePresentation.TypingBlip
{
    public static class TypingBlipEligibility
    {
        public static bool CanStart(bool skipActive, bool revealInstantly, bool backlogVisible)
        {
            return !skipActive && !revealInstantly && !backlogVisible;
        }
    }
}
