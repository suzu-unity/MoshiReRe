using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static partial class MenuRootV2Builder
{
    private static RectTransform BuildUtilitySavePage(RectTransform parent)
    {
        var page = ModernPageBase(parent, "PageSave", "セーブデータ", "SAVE / LOAD", ModernLavender);
        var frame = ModernPanel(page, "SaveFrame", new Vector2(20f, -92f), new Vector2(1460f, 700f), ModernPaper, ModernLavender);
        TextBox("保存データ", frame.rectTransform, 30f, FontStyles.Bold, TextAlignmentOptions.Left, ModernInk,
            new Vector2(26f, -22f), new Vector2(300f, 42f));

        var saveMode = UtilityButton(frame.rectTransform, "SaveModeButton", "セーブ", new Vector2(620f, -20f),
            new Vector2(168f, 52f), ModernCyan);
        var loadMode = UtilityButton(frame.rectTransform, "LoadModeButton", "ロード", new Vector2(802f, -20f),
            new Vector2(168f, 52f), ModernLavender);
        var mode = TextBox("セーブ / 記録する", frame.rectTransform, 26f, FontStyles.Bold, TextAlignmentOptions.Right, ModernInk,
            new Vector2(1012f, -28f), new Vector2(420f, 42f));

        var views = new MenuSaveLoadController.SlotView[8];
        for (var i = 0; i < views.Length; i++)
        {
            var column = i % 2;
            var row = i / 2;
            var slot = ButtonRoot("SaveSlot" + (i + 1), frame.rectTransform, ModernPaperLight);
            var slotRect = slot.GetComponent<RectTransform>();
            SetRect(slotRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f + column * 706f, -104f - row * 132f), new Vector2(686f, 112f));
            PixelBorder(slotRect, "Frame", column == 0 ? ModernLavender : ModernCyan, 2f);
            var accent = ImageRoot("SlotAccent", slotRect, column == 0 ? ModernLavender : ModernCyan);
            accent.raycastTarget = false;
            SetRect(accent.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(.5f, 1f),
                Vector2.zero, new Vector2(0f, 6f));
            var detail = Text("SLOT " + (i + 1).ToString("00") + "\n空きスロット", slotRect, 24f, FontStyles.Bold,
                TextAlignmentOptions.TopLeft, ModernInk, new Vector2(22f, 18f), new Vector2(-96f, -18f));
            var delete = ButtonRoot("DeleteButton", slotRect, ModernRose);
            var deleteRect = delete.GetComponent<RectTransform>();
            SetRect(deleteRect, new Vector2(1f, .5f), new Vector2(1f, .5f), new Vector2(1f, .5f),
                new Vector2(-16f, 0f), new Vector2(56f, 56f));
            PixelBorder(deleteRect, "Frame", ModernInk, 2f);
            Text("×", deleteRect, 34f, FontStyles.Bold, TextAlignmentOptions.Center, ModernInk);
            views[i] = new MenuSaveLoadController.SlotView(slot, delete, detail);
        }

        var back = UtilityButton(frame.rectTransform, "BackButton", "戻る", new Vector2(24f, 24f),
            new Vector2(168f, 54f), ModernCyan, true);
        var confirmation = ModernPanel(frame.rectTransform, "Confirmation", new Vector2(350f, -224f),
            new Vector2(760f, 250f), ModernInk, ModernRose);
        var confirmationText = TextBox("セーブデータを上書きしますか？\n元の記録は戻せません。", confirmation.rectTransform,
            26f, FontStyles.Bold, TextAlignmentOptions.Center, Cream, new Vector2(30f, -26f), new Vector2(700f, 106f));
        var confirm = UtilityButton(confirmation.rectTransform, "ConfirmButton", "実行", new Vector2(66f, 24f),
            new Vector2(220f, 58f), ModernCyan, true);
        var cancel = UtilityButton(confirmation.rectTransform, "CancelButton", "キャンセル", new Vector2(-66f, 24f),
            new Vector2(220f, 58f), ModernRose, false, true);
        confirmation.gameObject.SetActive(false);

        page.gameObject.AddComponent<MenuSaveLoadController>().Configure(views, saveMode, loadMode, back, confirm, cancel,
            confirmation.gameObject, confirmationText, mode);
        return page;
    }

    private static RectTransform BuildUtilitySettingsPage(RectTransform parent)
    {
        var page = ModernPageBase(parent, "PageSettings", "設定", "AUDIO / DISPLAY", ModernCyan);
        var frame = ModernPanel(page, "SettingsFrame", new Vector2(20f, -92f), new Vector2(1460f, 700f), ModernPaper, ModernCyan);
        TextBox("音声・表示", frame.rectTransform, 30f, FontStyles.Bold, TextAlignmentOptions.Left, ModernInk,
            new Vector2(26f, -22f), new Vector2(300f, 42f));

        var ids = new[] { "BGM", "SE", "VOICE", "TEXTSPEED", "AUTOSPEED" };
        var labels = new[] { "BGM音量", "SE音量", "ボイス音量", "文字表示速度", "オート送り速度" };
        var sliders = new Slider[ids.Length];
        for (var i = 0; i < sliders.Length; i++)
        {
            var row = ModernPanel(frame.rectTransform, "ConfigRow" + ids[i], new Vector2(24f, -88f - i * 84f),
                new Vector2(1412f, 68f), ModernPaperLight, i < 3 ? ModernLavender : ModernCyan);
            TextBox(labels[i], row.rectTransform, 26f, FontStyles.Bold, TextAlignmentOptions.Left, ModernInk,
                new Vector2(20f, -15f), new Vector2(284f, 38f));
            sliders[i] = UtilitySlider(row.rectTransform, "Config" + ids[i] + "Slider", new Vector2(330f, -20f),
                new Vector2(820f, 30f));
            sliders[i].value = i < 3 ? 1f : .5f;
            var value = TextBox(i < 3 ? "100%" : "50%", row.rectTransform, 26f, FontStyles.Bold,
                TextAlignmentOptions.Right, ModernInk, new Vector2(1170f, -15f), new Vector2(210f, 38f));
            var valueLabel = row.gameObject.AddComponent<MenuSliderValueLabel>();
            valueLabel.Configure(sliders[i], value);
        }

        var fullscreenRow = ModernPanel(frame.rectTransform, "FullscreenRow", new Vector2(24f, -516f), new Vector2(1412f, 68f),
            ModernPaperLight, ModernCyan);
        TextBox("フルスクリーン表示", fullscreenRow.rectTransform, 26f, FontStyles.Bold, TextAlignmentOptions.Left,
            ModernInk, new Vector2(20f, -15f), new Vector2(360f, 38f));
        var toggleRoot = ImageRoot("FullscreenToggle", fullscreenRow.rectTransform, ModernLavender);
        var toggleRect = toggleRoot.rectTransform;
        SetRect(toggleRect, new Vector2(1f, .5f), new Vector2(1f, .5f), new Vector2(1f, .5f),
            new Vector2(-26f, 0f), new Vector2(52f, 52f));
        PixelBorder(toggleRect, "Frame", ModernInk, 2f);
        var toggle = toggleRoot.gameObject.AddComponent<Toggle>();
        toggle.targetGraphic = toggleRoot;
        var check = ImageRoot("Check", toggleRect, ModernCyan);
        check.raycastTarget = false;
        Stretch(check.rectTransform, new Vector2(8f, 8f), new Vector2(-8f, -8f));
        toggle.graphic = check;
        toggle.isOn = true;

        var reset = UtilityButton(frame.rectTransform, "ResetButton", "初期設定に戻す", new Vector2(24f, 24f),
            new Vector2(250f, 54f), ModernRose, true);
        var back = UtilityButton(frame.rectTransform, "BackButton", "戻る", new Vector2(-24f, 24f),
            new Vector2(168f, 54f), ModernCyan, false, true);
        TextBox("変更は戻るときに保存されます", frame.rectTransform, 24f, FontStyles.Normal, TextAlignmentOptions.Center,
            ModernMuted, new Vector2(430f, -638f), new Vector2(620f, 32f));
        page.gameObject.AddComponent<MenuSettingsController>().Configure(sliders[0], sliders[1], sliders[2], sliders[3], sliders[4],
            toggle, reset, back);
        return page;
    }

    private static Button UtilityButton(RectTransform parent, string name, string label, Vector2 position, Vector2 size,
        Color color, bool bottomLeft = false, bool bottomRight = false)
    {
        var button = ButtonRoot(name, parent, color);
        var rect = button.GetComponent<RectTransform>();
        if (bottomRight)
            SetRect(rect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), position, size);
        else if (bottomLeft)
            SetRect(rect, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), position, size);
        else
            SetRect(rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), position, size);
        PixelBorder(rect, "Frame", ModernInk, 2f);
        Text(label, rect, 26f, FontStyles.Bold, TextAlignmentOptions.Center, ModernInk);
        return button;
    }

    private static Slider UtilitySlider(RectTransform parent, string name, Vector2 position, Vector2 size)
    {
        var slider = SliderRoot(name, parent, position, size);
        var rect = slider.GetComponent<RectTransform>();
        var background = rect.Find("Background")?.GetComponent<Image>();
        if (background) background.color = new Color(ModernInk.r, ModernInk.g, ModernInk.b, .92f);
        var fill = rect.Find("Fill Area/Fill")?.GetComponent<Image>();
        if (fill) fill.color = ModernCyan;
        var slideArea = RectRoot("Handle Slide Area", rect);
        Stretch(slideArea, new Vector2(14f, 0f), new Vector2(-14f, 0f));
        var handle = ImageRoot("Handle", slideArea, ModernPaperLight);
        SetRect(handle.rectTransform, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f),
            Vector2.zero, new Vector2(32f, 46f));
        PixelBorder(handle.rectTransform, "HandleFrame", ModernInk, 2f);
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.interactable = true;
        return slider;
    }
}
