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
#if UNITY_2023_1_OR_NEWER
        var hud = Object.FindFirstObjectByType<LocationHUD>();
#else
        var hud = Object.FindObjectOfType<LocationHUD>();
#endif

        if (hud != null)
        {
            hud.SetText(Text);
            Debug.Log($"[SetLocation] LocationHUD found. Text set to: {Text}");
        }
        else
        {
            Debug.LogWarning("[SetLocation] LocationHUD not found in scene.");
        }

        return Naninovel.UniTask.CompletedTask;
    }
}