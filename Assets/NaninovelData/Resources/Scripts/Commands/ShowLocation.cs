using Naninovel;
using Naninovel.Commands;
using Naninovel.UI;
using System.Threading.Tasks;
using UnityEngine;

[CommandAlias("showLocation")]
public class ShowLocation : Command
{
    [ParameterAlias(NamelessParameterAlias), RequiredParameter]
    public StringParameter Text;

    public DecimalParameter Hold; // hold:秒数

    public override UniTask Execute (AsyncToken asyncToken = default)
    {
        Debug.Log("[ShowLocation] start");

        var uiManager = Engine.GetService<IUIManager>();
        if (uiManager == null)
        {
            Debug.LogError("[ShowLocation] IUIManager is null");
            return UniTask.CompletedTask;
        }

        var ui = uiManager.GetUI("LocationCard");
        if (ui == null)
        {
            Debug.LogError("[ShowLocation] GetUI(\"LocationCard\") returned null. " +
                           "Custom UI の Name と 登録名 を確認してね。");
            return UniTask.CompletedTask;
        }

        ui.Show(); // v1系OK
        Debug.Log("[ShowLocation] ui.Show() called");

        var comp = ui as Component;
        if (comp == null)
        {
            Debug.LogError("[ShowLocation] IManagedUI is not a Component");
            return UniTask.CompletedTask;
        }

        var ctrl = comp.GetComponent<LocationCardUI>();
        if (ctrl == null)
        {
            Debug.LogError("[ShowLocation] LocationCardUI not found on UI root. " +
                           "Prefab の Root に LocationCardUI を付けて、参照を割り当ててください。");
            return UniTask.CompletedTask;
        }

        float hold = Hold.HasValue ? (float)Hold.Value : ctrl.HoldSeconds;
        ctrl.Play(Text, hold);
        Debug.Log($"[ShowLocation] Play: \"{Text}\" hold:{hold}");

        // 合計時間ぶん待つ（ms）
        int charCount = Text?.Value?.Length ?? 0;
        float totalSec = ctrl.FadeDuration + charCount * ctrl.CharInterval + hold + ctrl.FadeDuration;
        int ms = Mathf.CeilToInt(totalSec * 1000f);
        Debug.Log($"[ShowLocation] Delay ms: {ms}");

        return UniTask.Delay(ms, cancellationToken: asyncToken.CancellationToken);
    }
}