using UnityEngine;
using System;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    public int CurrentMoney { get; private set; } = 0;

    public event Action<int> OnMoneyChanged;

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
        if (Naninovel.Engine.Initialized)
        {
            SyncFromNaninovel();
        }
        else
        {
            Naninovel.Engine.OnInitializationFinished += SyncFromNaninovel;
        }
    }

    private void OnDestroy()
    {
        Naninovel.Engine.OnInitializationFinished -= SyncFromNaninovel;
    }

    private void SyncFromNaninovel()
    {
        var varManager = Naninovel.Engine.GetService<Naninovel.ICustomVariableManager>();
        if (varManager != null && varManager.VariableExists("Money"))
        {
            // CustomVariableValue does not have a .Value property, so we use .ToString() to get the string value.
            if (int.TryParse(varManager.GetVariableValue("Money").ToString(), out int value))
            {
                CurrentMoney = value;
                OnMoneyChanged?.Invoke(CurrentMoney);
            }
        }
    }

    private void SyncToNaninovel()
    {
        if (!Naninovel.Engine.Initialized) return;
        var varManager = Naninovel.Engine.GetService<Naninovel.ICustomVariableManager>();
        // Wrap the string in CustomVariableValue
        varManager?.SetVariableValue("Money", new Naninovel.CustomVariableValue(CurrentMoney.ToString()));
    }

    public void AddMoney(int amount)
    {
        CurrentMoney += amount;
        OnMoneyChanged?.Invoke(CurrentMoney);
        SyncToNaninovel();
    }

    public void SubtractMoney(int amount)
    {
        CurrentMoney -= amount;
        OnMoneyChanged?.Invoke(CurrentMoney);
        SyncToNaninovel();
    }

    public void SetMoney(int value)
    {
        CurrentMoney = value;
        OnMoneyChanged?.Invoke(CurrentMoney);
        SyncToNaninovel();
    }
}
