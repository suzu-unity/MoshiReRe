using System;
using System.Collections;
using Naninovel;
using UnityEngine;

/// <summary>Keeps the menu's existing day value in Naninovel saves, including quick saves.</summary>
public sealed class MenuDaySaveBridge : MonoBehaviour
{
    public const string StateKey = "MoshiReRe.MenuDay";
    [Serializable] public sealed class DayState { public int day; }
    [SerializeField] private MenuTopHudState dayHud;
    private IStateManager stateManager;
    private IEnumerator Start()
    {
        while (!Engine.Initialized) yield return null;
        if (!Engine.TryGetService(out stateManager)) yield break;
        stateManager.AddOnGameSerializeTask(Serialize);
        stateManager.AddOnGameDeserializeTask(Deserialize);
    }
    private void OnDestroy()
    {
        if (stateManager == null) return;
        stateManager.RemoveOnGameSerializeTask(Serialize);
        stateManager.RemoveOnGameDeserializeTask(Deserialize);
    }
    private void Serialize(GameStateMap map)
    {
        if (dayHud) map.SetState(new DayState { day = dayHud.CurrentDay }, StateKey);
    }
    private UniTask Deserialize(GameStateMap map)
    {
        var value = map.GetState<DayState>(StateKey);
        if (value != null)
            foreach(var hud in GetComponentsInChildren<MenuTopHudState>(true)) hud.SetDay(value.day);
        return UniTask.CompletedTask;
    }
    public static string FormatDay(GameStateMap map)
    {
        var value = map?.GetState<DayState>(StateKey);
        return value == null ? "DAY —" : $"DAY {Mathf.Max(0,value.day):00}";
    }
}
