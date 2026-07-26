using Naninovel;
using Naninovel.Commands;

[CommandAlias("setMainQuest")]
public class SetMainQuest : Command
{
    [ParameterAlias(NamelessParameterAlias), RequiredParameter]
    public StringParameter Title;

    [ParameterAlias("objective")]
    public StringParameter Objective;

    [ParameterAlias("days"), RequiredParameter]
    public IntegerParameter Days;

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        var title = Assigned(Title) ? Title.Value : string.Empty;
        var objective = Assigned(Objective) ? Objective.Value : string.Empty;
        var days = Assigned(Days) ? Days.Value : 0;

        // 先に独立Canvasを生成してイベント購読を確実にしてから状態を変更する。
        MainQuestPopup.EnsureInstance();
        MainQuestState.SetCurrent(title, objective, days);
        return UniTask.CompletedTask;
    }
}
