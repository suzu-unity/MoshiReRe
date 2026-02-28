using UnityEngine;
using Naninovel;
using Naninovel.Commands;
using System.Threading.Tasks;

/// <summary>
/// ReRe ボタンにメッセージを設定するカスタムコマンド
/// 使用例：@reReMessage "このメッセージはクリック時に表示されます"
/// </summary>
[CommandAlias("reReMessage")]
public class ReReMessage : Command
{
    [ParameterAlias(NamelessParameterAlias), RequiredParameter]
    public StringParameter Message;

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        // ReReButton をシーンから探す
        var reReButton = Object.FindObjectOfType<ReReButton>();
        if (reReButton != null)
        {
            // メッセージを ReReButton に設定
            reReButton.SetCurrentMessage(Message.Value);
            Debug.Log($"[ReReMessage] Message set: {Message.Value}");
        }
        else
        {
            Debug.LogWarning("[ReReMessage] ReReButton not found in scene!");
        }

        return UniTask.CompletedTask;
    }
}
