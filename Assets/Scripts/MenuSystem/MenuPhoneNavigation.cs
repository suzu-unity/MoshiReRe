using UnityEngine;
using UnityEngine.UI;

public sealed class MenuPhoneNavigation : MonoBehaviour
{
    [SerializeField] private Image[] icons;
    [SerializeField] private GameObject[] selectedLabels;
    [SerializeField] private GameObject[] selectedMarks;

    public void Select(int index)
    {
        for (var i = 0; i < icons.Length; i++)
        {
            if (icons[i]) icons[i].color = i == index ? Color.white : new Color(.82f, .82f, .9f, 1f);
            if (i < selectedLabels.Length && selectedLabels[i]) selectedLabels[i].SetActive(i == index && i > 0 && i < 6);
            if (i < selectedMarks.Length && selectedMarks[i]) selectedMarks[i].SetActive(i == index);
        }
    }
}
