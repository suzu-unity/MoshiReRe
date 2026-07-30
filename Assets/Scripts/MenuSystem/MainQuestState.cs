using System;
using Naninovel;
using UnityEngine;

/// <summary>セーブ可能な現在のメインクエスト。UI はこのイベントだけを購読する。</summary>
public static class MainQuestState
{
    public const string TitleVariableName = "mainQuestTitle";
    public const string ObjectiveVariableName = "mainQuestObjective";
    public const string DeadlineDaysVariableName = "mainQuestDeadlineDays";

    public const string SafeDeadlineColor = "#F2C94C";
    public const string UrgentDeadlineColor = "#E05252";

    public readonly struct Data
    {
        public readonly string Title;
        public readonly string Objective;
        public readonly int DeadlineDays;

        public bool IsAssigned => !string.IsNullOrWhiteSpace(Title);

        public Data(string title, string objective, int deadlineDays)
        {
            Title = title ?? string.Empty;
            Objective = objective ?? string.Empty;
            DeadlineDays = Mathf.Max(0, deadlineDays);
        }
    }

    public static Data Current { get; private set; }
    public static event Action<Data> OnChanged;

    public static void SetCurrent(string title, string objective, int deadlineDays)
    {
        Current = new Data(title, objective, deadlineDays);
        SyncToNaninovel();
        OnChanged?.Invoke(Current);
    }

    public static void SyncFromNaninovel()
    {
        if (!Engine.Initialized) return;

        var variables = Engine.GetService<ICustomVariableManager>();
        if (variables == null || !variables.VariableExists(TitleVariableName)) return;

        var title = variables.GetVariableValue(TitleVariableName);
        var objective = variables.VariableExists(ObjectiveVariableName)
            ? variables.GetVariableValue(ObjectiveVariableName)
            : new CustomVariableValue(string.Empty);
        var days = variables.VariableExists(DeadlineDaysVariableName)
            ? variables.GetVariableValue(DeadlineDaysVariableName)
            : new CustomVariableValue(0);

        Current = CreateDataFromVariables(title, objective, days);
        OnChanged?.Invoke(Current);
    }

    public static Data CreateDataFromVariables(
        CustomVariableValue title,
        CustomVariableValue objective,
        CustomVariableValue deadlineDays)
    {
        return new Data(title.String, objective.String, Mathf.RoundToInt(deadlineDays.Number));
    }

    public static string FormatDeadline(int days)
    {
        var clampedDays = Mathf.Max(0, days);
        var color = clampedDays >= 10 ? SafeDeadlineColor : UrgentDeadlineColor;
        return $"期限: <color={color}>{clampedDays}</color>日後";
    }

    public static Color DeadlineColor(int days) => ColorUtility.TryParseHtmlString(days >= 10 ? SafeDeadlineColor : UrgentDeadlineColor, out var color)
        ? color
        : Color.white;

    public static void ResetForTests()
    {
        Current = default;
        OnChanged = null;
    }

    private static void SyncToNaninovel()
    {
        if (!Engine.Initialized) return;

        var variables = Engine.GetService<ICustomVariableManager>();
        if (variables == null) return;

        variables.SetVariableValue(TitleVariableName, new CustomVariableValue(Current.Title));
        variables.SetVariableValue(ObjectiveVariableName, new CustomVariableValue(Current.Objective));
        variables.SetVariableValue(DeadlineDaysVariableName, new CustomVariableValue(Current.DeadlineDays));
    }
}
