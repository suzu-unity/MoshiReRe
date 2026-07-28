using System;

/// <summary>Keeps the latest scenario location available while the HUD is created or shown.</summary>
public static class LocationHUDState
{
    public static string Current { get; private set; } = string.Empty;
    public static event Action<string> OnChanged;

    public static void SetCurrent(string location)
    {
        Current = location ?? string.Empty;
        OnChanged?.Invoke(Current);
    }

    public static void ResetForTests()
    {
        Current = string.Empty;
        OnChanged = null;
    }
}
