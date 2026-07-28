using Naninovel;
using Naninovel.Commands;
using UnityEngine;

[CommandAlias("setLocation")]
public class SetLocation : Command
{
    [ParameterAlias(NamelessParameterAlias), RequiredParameter]
    public StringParameter Text;

    public override Naninovel.UniTask Execute(AsyncToken token = default)
    {
        var location = Assigned(Text) ? Text.Value : string.Empty;
        LocationHUDState.SetCurrent(location);
        Debug.Log($"[SetLocation] Location set to: {location}");

        return Naninovel.UniTask.CompletedTask;
    }
}
