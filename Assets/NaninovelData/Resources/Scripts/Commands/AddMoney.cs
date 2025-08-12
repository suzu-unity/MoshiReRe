using UnityEngine;
using Naninovel;
using Naninovel.Commands;
using System.Threading.Tasks;

[CommandAlias("addMoney")]
public class AddMoney : Command
{
    [ParameterAlias(NamelessParameterAlias), RequiredParameter]
    public IntegerParameter Amount;

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        var vars = Engine.GetService<ICustomVariableManager>();

        // money 変数を取得（CustomVariableValue → string化してから数値へ）
        var curVal = vars.GetVariableValue("money");
        var curStr = curVal.ToString(); // ★ ここがポイント
        int current = 0;
        if (!string.IsNullOrEmpty(curStr))
            int.TryParse(curStr, out current);

        var next = current + Amount.Value;

        // 変数へ保存（CustomVariableValueで渡す）
        vars.SetVariableValue("money", new CustomVariableValue(next));

        // UI（MoneyManager）へ反映
        MoneyManager.Instance?.SetMoney(next);

        return UniTask.CompletedTask;
    }
}