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
        // 既存の所持金を MoneyManager から取得し、累積で増減させる。
        // MoneyManager にインスタンスが存在しない場合は 0 とする。
        var manager = MoneyManager.Instance;
        int currentMoney = manager != null ? manager.CurrentMoney : 0;

        // 今回の命令で増減させる金額。負数の場合は減少扱いとする。
        int delta = Amount.Value;

        // MoneyManager へ累積で増減を反映する。
        if (manager != null)
        {
            if (delta >= 0)
                manager.AddMoney(delta);
            else
                manager.SubtractMoney(-delta);

            currentMoney = manager.CurrentMoney;
        }
        else
        {
            // MoneyManager が見つからない場合でもローカルで累積計算する
            currentMoney += delta;
        }

        // Naninovel のカスタム変数へ現在の金額を保存しておく。
        var vars = Engine.GetService<ICustomVariableManager>();
        vars.SetVariableValue("money", new CustomVariableValue(currentMoney));

        return UniTask.CompletedTask;
    }
}