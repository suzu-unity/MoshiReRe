using Naninovel;
using Naninovel.Commands;

/// <summary>
/// ReReのアドバイスをセットするカスタムコマンド。
/// 使い方: @rere "テキスト"
/// 例: @rere "ギバー属性ね〜。弱みを先に握っておくと後で使えるよ？"
/// </summary>
[CommandAlias("rere")]
public class SetReReAdvice : Command
{
    [ParameterAlias(NamelessParameterAlias), RequiredParameter]
    public StringParameter Text;

    public override UniTask Execute(AsyncToken asyncToken = default)
    {
        var overlay = UnityEngine.Object.FindObjectOfType<ReReOverlay>();
        if (overlay != null)
            overlay.SetAdvice(Text.Value);
        else
            Naninovel.Engine.Warn("[SetReReAdvice] ReReOverlay が見つかりません。プレハブがシーンに存在するか確認してください。");

        return UniTask.CompletedTask;
    }
}
