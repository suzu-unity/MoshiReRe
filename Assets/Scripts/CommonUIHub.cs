using UnityEngine;

public class CommonUIHub : MonoBehaviour
{
    public GameObject moneyUIPrefab;
    public GameObject reReButtonPrefab;

    private void Start()
    {
        if (moneyUIPrefab) Instantiate(moneyUIPrefab, transform);
        if (reReButtonPrefab) Instantiate(reReButtonPrefab, transform);
    }
}
