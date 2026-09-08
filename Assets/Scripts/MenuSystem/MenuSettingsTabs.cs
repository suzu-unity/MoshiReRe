using UnityEngine;
using UnityEngine.UI;

public sealed class MenuSettingsTabs : MonoBehaviour
{
    [SerializeField] private GameObject[] panels;
    [SerializeField] private Button[] buttons;
    private void Awake()
    {
        for (var i = 0; i < buttons.Length; i++) { var index = i; buttons[i].onClick.AddListener(() => Select(index)); }
    }
    private void OnEnable() => Select(0);
    public void Select(int index)
    {
        for (var i = 0; i < panels.Length; i++)
        {
            if (panels[i]) panels[i].SetActive(i == index);
            if (i < buttons.Length && buttons[i]) buttons[i].image.color = i == index ? Color.white : new Color(.8f, .8f, .9f);
        }
    }
}
