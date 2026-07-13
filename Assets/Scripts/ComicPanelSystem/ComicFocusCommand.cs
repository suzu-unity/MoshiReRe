using Naninovel;

[Command.CommandAlias("comicFocus")]
public sealed class ComicFocusCommand : Command
{
    [Command.ParameterAlias("panel"), Command.RequiredParameter]
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
            UnityEngine.Debug.LogWarning("[comicFocus] ComicPanelController is not available.");
            return UniTask.CompletedTask;
        }

        if (!Assigned(Panel) || Panel.Value < 1)
        {
            UnityEngine.Debug.LogWarning("[comicFocus] panel must be a 1-based positive index.");
            return UniTask.CompletedTask;
        }

        var panelIndex = Panel.Value - 1;
        var mode = ComicPanelCommandUtility.ParseMode(Mode, ComicPanelFocusMode.Through);
        var time = Assigned(Time) ? UnityEngine.Mathf.Max(0f, Time.Value) : -1f;
        if (mode == ComicPanelFocusMode.Only)
            controller.FocusOnly(panelIndex, time);
        else
            controller.FocusThrough(panelIndex, time);
        return UniTask.CompletedTask;
    }
}

internal static class ComicPanelCommandUtility
{
    public static ComicPanelFocusMode ParseMode(StringParameter parameter, ComicPanelFocusMode fallback)
    {
        if (!Command.Assigned(parameter))
            return fallback;

        if (string.Equals(parameter.Value, "only", System.StringComparison.OrdinalIgnoreCase))
            return ComicPanelFocusMode.Only;
        if (string.Equals(parameter.Value, "all", System.StringComparison.OrdinalIgnoreCase))
            return ComicPanelFocusMode.All;
        return ComicPanelFocusMode.Through;
    }
}
