using Naninovel;
using Naninovel.Commands;
using System.Threading.Tasks;

[CommandAlias("syncMoney")]
public class SyncMoney : Command
{
    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        var vars = Engine.GetService<ICustomVariableManager>();

        // 変数から読み出し（CustomVariableValue → string → int）
        var curVal = vars.GetVariableValue("money");
        var curStr = curVal.ToString(); // ★
        int current = 0;
        if (!string.IsNullOrEmpty(curStr))
            int.TryParse(curStr, out current);

        // MoneyManager に反映
        MoneyManager.Instance?.SetMoney(current);

        return UniTask.CompletedTask;
    }
}