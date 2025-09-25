using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// キャラクターアイコン上にカーソルが乗った/外れた際に説明文の表示・非表示を行うトリガー。
/// IPointerEnterHandler と IPointerExitHandler を実装してカーソル検知を行っています:contentReference[oaicite:0]{index=0}。
/// </summary>
public class AdviceTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 表示ロジックを担当する AdviceClickTrigger。
    // 同じ GameObject または別のオブジェクトにアタッチしたものをインスペクタで設定します。
    [SerializeField]
    private AdviceClickTrigger clickTrigger;

    /// <summary>
    /// マウスカーソルがアイコン上に入った際に呼び出されます。
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (clickTrigger != null)
        {
            clickTrigger.ShowAdvice();
        }
    }

    /// <summary>
    /// マウスカーソルがアイコンから離れた際に呼び出されます。
    /// 入力用インタフェース(IPointerExitHandler)でポインターが外れたことを検知し、吹き出しを非表示にします:contentReference[oaicite:1]{index=1}。
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (clickTrigger != null)
        {
            clickTrigger.HideAdvice();
        }
    }
}
