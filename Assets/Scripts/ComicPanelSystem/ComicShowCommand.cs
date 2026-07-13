using Naninovel;

[Command.CommandAlias("comicShow")]
public sealed class ComicShowCommand : Command
{
    [Command.ParameterAlias("id"), Command.RequiredParameter]
    public StringParameter Id;

    [Command.ParameterAlias("panel")]
    public IntegerParameter Panel;

    [Command.ParameterAlias("mode")]
    public StringParameter Mode;

    [Command.ParameterAlias("time")]
    public DecimalParameter Time;

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        var controller = ComicPanelController.Instance;
        if (controller == null)
        {
            UnityEngine.Debug.LogWarning("[comicShow] ComicPanelController is not available.");
            return UniTask.CompletedTask;
        }

        var panelIndex = Assigned(Panel) ? Panel.Value - 1 : -1;
        var mode = ComicPanelCommandUtility.ParseMode(Mode, panelIndex < 0 ? ComicPanelFocusMode.All : ComicPanelFocusMode.Through);
        var time = Assigned(Time) ? UnityEngine.Mathf.Max(0f, Time.Value) : -1f;
        controller.ShowById(Assigned(Id) ? Id.Value : string.Empty, panelIndex, mode, time);
        return UniTask.CompletedTask;
    }
}
