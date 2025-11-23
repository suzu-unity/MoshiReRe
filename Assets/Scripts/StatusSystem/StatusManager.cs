using UnityEngine;
using System;
using Naninovel;

public class StatusManager : MonoBehaviour
{
    public static StatusManager Instance { get; private set; }

    // Stats
    public int Intelligence { get; private set; } = 1;
    public int Courage { get; private set; } = 1;
    public int Strength { get; private set; } = 1;

    public event Action OnStatusChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        if (Engine.Initialized)
        {
            SyncFromNaninovel();
        }
        else
        {
            Engine.OnInitializationFinished += SyncFromNaninovel;
        }
    }

    private void OnDestroy()
    {
        Engine.OnInitializationFinished -= SyncFromNaninovel;
    }

    private void SyncFromNaninovel()
    {
        var varManager = Engine.GetService<ICustomVariableManager>();
        if (varManager == null) return;

        if (varManager.VariableExists("Intelligence") && int.TryParse(varManager.GetVariableValue("Intelligence").ToString(), out int intelligence))
        {
            Intelligence = intelligence;
        }
        if (varManager.VariableExists("Courage") && int.TryParse(varManager.GetVariableValue("Courage").ToString(), out int courage))
        {
            Courage = courage;
        }
        if (varManager.VariableExists("Strength") && int.TryParse(varManager.GetVariableValue("Strength").ToString(), out int strength))
        {
            Strength = strength;
        }
        OnStatusChanged?.Invoke();
    }

    private void SyncToNaninovel()
    {
        if (!Engine.Initialized) return;
        var varManager = Engine.GetService<ICustomVariableManager>();
        if (varManager == null) return;

        varManager.SetVariableValue("Intelligence", new CustomVariableValue(Intelligence.ToString()));
        varManager.SetVariableValue("Courage", new CustomVariableValue(Courage.ToString()));
        varManager.SetVariableValue("Strength", new CustomVariableValue(Strength.ToString()));
    }

    public void SetIntelligence(int value)
    {
        Intelligence = value;
        OnStatusChanged?.Invoke();
        SyncToNaninovel();
    }

    public void SetCourage(int value)
    {
        Courage = value;
        OnStatusChanged?.Invoke();
        SyncToNaninovel();
    }

    public void SetStrength(int value)
    {
        Strength = value;
        OnStatusChanged?.Invoke();
        SyncToNaninovel();
    }
}
