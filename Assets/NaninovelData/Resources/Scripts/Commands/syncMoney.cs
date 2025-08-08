using Naninovel;
using Naninovel.Commands;
using System.Threading.Tasks;

[CommandAlias("syncMoney")]
public class SyncMoney : Command
{
    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        var stateManager = Engine.GetService<ICustomVariableManager>();
        stateManager.SetVariableValue("money", new CustomVariableValue(MoneyManager.Instance.CurrentMoney));
        return UniTask.CompletedTask;
    }
}