using Naninovel;
using Naninovel.Commands;

[CommandAlias("hideComicDemo")]
public class HideComicDemo : Command
{
    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        if (ComicDemoOverlayController.Instance)
            ComicDemoOverlayController.Instance.Hide();

        return UniTask.CompletedTask;
    }
}
