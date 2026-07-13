using Naninovel;

namespace MoshiReRe.DialoguePresentation.CharacterMotion
{
    [Command.CommandAlias("charMotion")]
    public sealed class CharacterMotionCommand : Command
    {
        [Command.ParameterAlias("id"), Command.RequiredParameter]
        public StringParameter Id;

        [Command.ParameterAlias("type"), Command.RequiredParameter]
        public StringParameter Type;

        public override UniTask Execute(AsyncToken asyncToken = default)
        {
            // Never query ICharacterManager before Naninovel has finished initializing.
            if (!Engine.Initialized) return UniTask.CompletedTask;

            var id = Assigned(Id) ? Id.Value : string.Empty;
            var type = Assigned(Type) ? Type.Value : string.Empty;
            var library = CharacterMotionController.Instance
                ? CharacterMotionController.Instance.Library
                : null;
            library = library ? library : UnityEngine.Resources.Load<CharacterMotionLibrary>(CharacterMotionLibrary.DefaultResourcePath);
            return CharacterMotionRuntime.Play(id, type, library, asyncToken);
        }
    }
}
