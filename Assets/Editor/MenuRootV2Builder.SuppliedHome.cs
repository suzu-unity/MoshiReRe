using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static partial class MenuRootV2Builder
{
    private static RectTransform BuildSuppliedHome(RectTransform parent, UnityAction[] actions, out RectTransform phone)
    {
        var page = RectRoot("PageTop", parent);
        Stretch(page, Vector2.zero, Vector2.zero);
        phone = RectRoot("PortraitPhonePresentation", page);
        SetRect(phone, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(604, -4), new Vector2(565, 1004));
        phone.localRotation = Quaternion.Euler(0, 0, -5);
        SuppliedImage(phone, "SuppliedReadingRoom", SuppliedMenuAssetLibrary.Get("home"), new Rect(0, 0, 565, 1004));
        var deadline = SuppliedImage(phone, "DeadlineFrame", SuppliedMenuAssetLibrary.Get("home_parts.deadline"), new Rect(28, 70, 254, 86));
        var debtText = SuppliedLabel(deadline.transform, "DebtValue", "返済期限\nあと7日", new Rect(78, 22, 170, 58), 21, TextAlignmentOptions.Center);
        var stamina = SuppliedImage(phone, "StaminaFrame", SuppliedMenuAssetLibrary.Get("home_parts.stamina"), new Rect(285, 78, 244, 57));
        var staminaText = SuppliedLabel(stamina.transform, "StaminaValue", "85/100", new Rect(170, 24, 60, 22), 15, TextAlignmentOptions.Center);
        staminaText.color = Color.white;
        var fillSprite = SuppliedMenuAssetLibrary.Slice("home_parts.deadline", "stamina_fill", new Rect(470, 152, 32, 24));
        var fill = SuppliedImage(stamina.transform, "StaminaFill", fillSprite, new Rect(61, 31, 105, 8));
        fill.preserveAspect = false;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = .85f;
        var moneyFrame = SuppliedImage(phone, "MoneyFrame", SuppliedMenuAssetLibrary.Get("home_parts.money"), new Rect(285, 140, 244, 46));
        var moneyText = SuppliedLabel(moneyFrame.transform, "MoneyValue", "000,000", new Rect(95, 8, 128, 28), 19, TextAlignmentOptions.Center);
        moneyText.color = Color.white;
        Wire(page.gameObject.AddComponent<MenuTopHudState>(), ("debtDaysText", debtText), ("staminaText", staminaText), ("staminaFill", fill));
        Wire(moneyText.gameObject.AddComponent<MoneyUI>(), ("moneyText", moneyText));

        var ids = new[] { "dress", "items", "characters", "quest", "map", "save", "settings" };
        var targets = new[] { 1, 2, 3, 4, 5, 6, 7 };
        var names = new[] { "DressTileHitbox", "ItemsTileHitbox", "CharactersTileHitbox", "QuestTileHitbox", "MapTileHitbox", "SaveTileHitbox", "SettingsTileHitbox" };
        var positions = new[] { new Rect(391, 231, 126, 148), new Rect(391, 388, 126, 148), new Rect(391, 545, 126, 148), new Rect(49, 854, 137, 101), new Rect(194, 854, 137, 101), new Rect(342, 858, 180, 43), new Rect(342, 912, 180, 43) };
        for (var i = 0; i < ids.Length; i++)
            SuppliedButton(phone, names[i], SuppliedMenuAssetLibrary.Get("home_" + ids[i]), positions[i], actions[targets[i]], SuppliedMenuAssetLibrary.Get("hover_" + ids[i]));

        var banner = SuppliedButton(phone, "EventBanner", SuppliedMenuAssetLibrary.Get("event"), new Rect(42, 731, 485, 112), actions[4]);
        var title = SuppliedLabel(banner.transform, "CurrentObjective", "初回ターゲット", new Rect(124, 30, 260, 34), 24, TextAlignmentOptions.Center);
        title.color = Color.white;
        var objective = SuppliedLabel(banner.transform, "ObjectiveDetail", "カフェ下調べ", new Rect(126, 65, 252, 26), 20, TextAlignmentOptions.Center);
        objective.color = Color.white;

        var target = SuppliedButton(phone, "ReReTapTarget", null, new Rect(65, 283, 314, 433));
        target.image.color = Color.clear;
        var bubble = SuppliedImage(phone, "ReReSpeechBubble", SuppliedMenuAssetLibrary.Get("home_parts.bubble"), new Rect(63, 260, 320, 102));
        var message = SuppliedLabel(bubble.transform, "Comment", "", new Rect(30, 18, 263, 65), 27, TextAlignmentOptions.Center);
        Wire(page.gameObject.AddComponent<MenuHomeComment>(), ("characterButton", target), ("bubble", bubble.gameObject), ("text", message));
        bubble.gameObject.SetActive(false);
        return page;
    }
}
