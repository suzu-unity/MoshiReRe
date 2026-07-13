using Naninovel;

[Command.CommandAlias("comicHide")]
public sealed class ComicHideCommand : Command
{
    [Command.ParameterAlias("time")]
    public DecimalParameter Time;

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        var controller = ComicPanelController.Instance;
        if (controller == null)
        {
            UnityEngine.Debug.LogWarning("[comicHide] ComicPanelController is not available.");
            return UniTask.CompletedTask;
        }

        var time = Assigned(Time) ? UnityEngine.Mathf.Max(0f, Time.Value) : -1f;
        controller.Hide(time);
        return UniTask.CompletedTask;
    }
}
