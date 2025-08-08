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

    public void AddMoney(int amount)
    {
        CurrentMoney += amount;
        OnMoneyChanged?.Invoke(CurrentMoney);
    }

    public void SubtractMoney(int amount)
    {
        CurrentMoney -= amount;
        OnMoneyChanged?.Invoke(CurrentMoney);
    }

    public void SetMoney(int value)
    {
        CurrentMoney = value;
        OnMoneyChanged?.Invoke(CurrentMoney);
    }
}
