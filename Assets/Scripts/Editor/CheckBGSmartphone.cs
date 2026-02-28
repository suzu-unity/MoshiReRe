using UnityEngine;
using UnityEditor;

public class CheckBGSmartphone
{
    public static void Execute()
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/BackGround/BGsmartphone01.png");
        if (sprite)
        {
            Debug.Log($"BGsmartphone01 found: {sprite.name}");
        }
        else
        {
            Debug.Log("BGsmartphone01 not found as Sprite");
        }
    }
}
