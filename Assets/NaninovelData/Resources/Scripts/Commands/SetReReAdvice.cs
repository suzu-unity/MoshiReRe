using Naninovel;
using Naninovel.Commands;

[CommandAlias("setReReAdvice")]
public class SetReReAdvice : Command
{
    [ParameterAlias(NamelessParameterAlias), RequiredParameter]
    public StringParameter Message;

    [ParameterAlias("marker")]
    public StringParameter Marker;

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        var message = Assigned(Message) ? Message.Value : string.Empty;
        var marker = Assigned(Marker) ? Marker.Value : string.Empty;

        if (ReReButtonController.Instance != null)
            ReReButtonController.Instance.SetAdvice(message, marker);
        else
            UnityEngine.Debug.LogWarning("[setReReAdvice] ReReButtonController is not found in active scene.");

        return UniTask.CompletedTask;
    }
}
