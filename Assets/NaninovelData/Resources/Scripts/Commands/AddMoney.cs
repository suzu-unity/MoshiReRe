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
        Debug.Log($"[AddMoney Command] Called with amount: {Amount.Value}");
        if (MoneyManager.Instance == null)
        {
            Debug.LogError("MoneyManager.Instance is NULL in AddMoney");
                    }
        else
        {
            MoneyManager.Instance.AddMoney(Amount.Value);
        }

        return UniTask.CompletedTask;
    }
}