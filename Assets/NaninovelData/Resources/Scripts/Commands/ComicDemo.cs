using Naninovel;
using Naninovel.Commands;

[CommandAlias("comicDemo")]
public class ComicDemo : Command
{
    [ParameterAlias("page"), RequiredParameter]
    public StringParameter Page;

    [ParameterAlias("focus")]
    public StringParameter Focus;

    [ParameterAlias("next")]
    public StringParameter Next;

    [ParameterAlias("prev")]
    public StringParameter Prev;

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        if (!Assigned(Page))
            return UniTask.CompletedTask;

        var overlay = ComicDemoOverlayController.Instance;
        if (!overlay)
        {
            UnityEngine.Debug.LogWarning("[comicDemo] ComicDemoOverlayController is not available.");
            return UniTask.CompletedTask;
        }

        overlay.ShowPage(
            Page.Value,
            Assigned(Focus) ? Focus.Value : "left",
            Assigned(Next) ? Next.Value : string.Empty,
            Assigned(Prev) ? Prev.Value : string.Empty,
            PlaybackSpot.ScriptPath);

        return UniTask.CompletedTask;
    }
}
