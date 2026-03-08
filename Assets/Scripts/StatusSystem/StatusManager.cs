using UnityEngine;
using System;
using Naninovel;

public class StatusManager : MonoBehaviour
{
    public static StatusManager Instance { get; private set; }

    public int Guts { get; private set; } = 1;
    public int Intelligence { get; private set; } = 1;
    public int Attention { get; private set; } = 1;
    public int Technique { get; private set; } = 1;
    public int Strength { get; private set; } = 1;

    // Backward compatibility alias.
    public int Courage => Guts;

    public event Action OnStatusChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (Engine.Initialized) SyncFromNaninovel();
        else Engine.OnInitializationFinished += SyncFromNaninovel;
    }

    private void OnDestroy()
    {
        Engine.OnInitializationFinished -= SyncFromNaninovel;
    }

    private void SyncFromNaninovel()
    {
        var varManager = Engine.GetService<ICustomVariableManager>();
        if (varManager == null) return;

        Guts = ReadInt(varManager, "Guts", ReadInt(varManager, "Courage", Guts));
        Intelligence = ReadInt(varManager, "Intelligence", Intelligence);
        Attention = ReadInt(varManager, "Attention", Attention);
        Technique = ReadInt(varManager, "Technique", Technique);
        Strength = ReadInt(varManager, "Strength", Strength);

        OnStatusChanged?.Invoke();
    }

    private void SyncToNaninovel()
    {
        if (!Engine.Initialized) return;

        var varManager = Engine.GetService<ICustomVariableManager>();
        if (varManager == null) return;

        varManager.SetVariableValue("Guts", new CustomVariableValue(Guts.ToString()));
        varManager.SetVariableValue("Courage", new CustomVariableValue(Guts.ToString()));
        varManager.SetVariableValue("Intelligence", new CustomVariableValue(Intelligence.ToString()));
        varManager.SetVariableValue("Attention", new CustomVariableValue(Attention.ToString()));
        varManager.SetVariableValue("Technique", new CustomVariableValue(Technique.ToString()));
        varManager.SetVariableValue("Strength", new CustomVariableValue(Strength.ToString()));
    }

    public void SetGuts(int value)
    {
        Guts = value;
        RaiseAndSync();
    }

    public void SetIntelligence(int value)
    {
        Intelligence = value;
        RaiseAndSync();
    }

    public void SetAttention(int value)
    {
        Attention = value;
        RaiseAndSync();
    }

    public void SetTechnique(int value)
    {
        Technique = value;
        RaiseAndSync();
    }

    public void SetStrength(int value)
    {
        Strength = value;
        RaiseAndSync();
    }

    // Backward compatibility
    public void SetCourage(int value) => SetGuts(value);

    private void RaiseAndSync()
    {
        OnStatusChanged?.Invoke();
        SyncToNaninovel();
    }

    private static int ReadInt(ICustomVariableManager vars, string name, int fallback)
    {
        if (!vars.VariableExists(name)) return fallback;

        var raw = vars.GetVariableValue(name).ToString();
        return int.TryParse(raw, out var parsed) ? parsed : fallback;
    }
}
