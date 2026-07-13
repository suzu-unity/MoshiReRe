using Naninovel;

namespace MoshiReRe.DialoguePresentation.Beat
{
    [Command.CommandAlias("beat")]
    public sealed class BeatCommand : Command
    {
        [Command.ParameterAlias("type"), Command.RequiredParameter]
        public StringParameter Type;

        public override UniTask Execute(AsyncToken asyncToken = default)
        {
            // Do not query any Naninovel service before this guard.
            if (!Engine.Initialized) return UniTask.CompletedTask;
            return BeatController.Instance
                ? BeatController.Instance.Play(Assigned(Type) ? Type.Value : string.Empty, asyncToken)
                : UniTask.CompletedTask;
        }
    }
}
