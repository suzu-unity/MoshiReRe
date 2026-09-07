using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static partial class MenuRootV2Builder
{
    private static Transform Descendant(Transform root, string name)
    {
        foreach (var child in root.GetComponentsInChildren<Transform>(true))
            if (child.name == name) return child;
        return null;
    }

    private static void Place(Transform target, float x, float y, float w, float h)
    {
        SetRect((RectTransform)target, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(x, -y), new Vector2(w, h));
    }

    private static void PolishLegacyPage(RectTransform page)
    {
        // Retain AutoWire paths and the art/animation references; remove only
        // duplicate chrome, which used to obscure the shared menu navigation.
        foreach (var child in page.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.StartsWith("Unified") || child.name.EndsWith("PhoneFrame")
                || child.name.EndsWith("PhoneBody") || child.name == "DressPhoneArtwork"
                || child.name.EndsWith("HomeHitbox") || child.name.EndsWith("DressHitbox")
                || child.name.EndsWith("StatusHitbox") || child.name.EndsWith("ItemsHitbox")
                || child.name.EndsWith("MapHitbox")) child.gameObject.SetActive(false);
        }
        foreach (var text in page.GetComponentsInChildren<TMP_Text>(true))
            if (text.text == "MoshiReRe" || text.text == "▂▃▆" || text.text == "▰▰▰") text.gameObject.SetActive(false);
        foreach (var button in page.GetComponentsInChildren<Button>(true))
            if (button.name.StartsWith("Items") && button.name.EndsWith("Button")
                || button.name.StartsWith("Characters") && button.name.EndsWith("Button"))
                button.gameObject.SetActive(false);

        var background = ImageRoot("PageSurface", page, ModernPaperLight);
        Stretch(background.rectTransform, Vector2.zero, Vector2.zero);
        background.raycastTarget = false;
        background.transform.SetAsFirstSibling();
        PixelBorder(background.rectTransform, "Frame", ModernMuted, 3);
        var title = page.name == "PageDressStatus" ? "着替え / WARDROBE"
            : page.name == "PageItems" ? "持ち物 / INVENTORY" : "人物 / CONTACTS";
        ModernText(title, page, 36, new Vector2(44, -28), new Vector2(900, 54), ModernInk, FontStyles.Bold);
        ModernText("準備した情報と道具を、次の会話へ。", page, 24, new Vector2(46, -92), new Vector2(1000, 36), ModernMuted);
        if (page.name == "PageDressStatus") PolishDress(page);
        if (page.name == "PageCharacters")
        {
            foreach (var name in new[] { "CharacterInformationNodes", "ReReMemo" })
                foreach (var text in Descendant(page, name).GetComponentsInChildren<TMP_Text>(true))
                    text.fontSize = Mathf.Max(24f, text.fontSize);
            foreach (var text in Descendant(page, "CharacterInformationNodes").GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.text.StartsWith("RE: INFORMATION")) text.text = "情報ノード / 4件";
                if (text.transform.parent.name.StartsWith("DemoInformationNodeRow"))
                {
                    text.fontSize = 22f;
                    text.textWrappingMode = TextWrappingModes.NoWrap;
                    text.rectTransform.sizeDelta = new Vector2(text.rectTransform.sizeDelta.x, 40f);
                }
            }
        }
        if (page.name == "PageItems")
        {
            var detail = (RectTransform)Descendant(page, "SelectedItemDetail");
            detail.sizeDelta = new Vector2(detail.sizeDelta.x, 390f);
            Place(Descendant(detail, "DetailDescription"), 28, 206, 316, 155);
            Descendant(detail, "DetailDescription").GetComponent<TMP_Text>().fontSize = 26f;
            foreach (var label in detail.GetComponentsInChildren<TMP_Text>(true))
                if (label.text == "RELATED") label.gameObject.SetActive(false);
            for (var i = 0; i < 3; i++) Descendant(detail, "RelatedCharacter" + i).gameObject.SetActive(false);
            var note = (RectTransform)Descendant(page, "ReReItemCommentPanel");
            note.sizeDelta = new Vector2(note.sizeDelta.x, 390f);
            Place(Descendant(note, "ReReItemBubble"), 18, 120, 224, 200);
            Descendant(note, "ReReCommentText").GetComponent<TMP_Text>().fontSize = 24f;
            Place(Descendant(note, "ConfirmBagButton"), 138, 336, 92, 42);
        }
    }

    private static void PolishDress(RectTransform page)
    {
        var backdrop = ModernPanel(page, "FittingRoomBackdrop", new Vector2(630, -154), new Vector2(426, 446), new Color(.86f,.83f,.94f,1));
        backdrop.transform.SetSiblingIndex(1);
        ModernText("試着室", backdrop.rectTransform, 28, new Vector2(20,-16), new Vector2(386,40), ModernInk, FontStyles.Bold, TextAlignmentOptions.Center);
        var stats = ModernPanel(page, "StatusBackdrop", new Vector2(180,-154), new Vector2(410,446), new Color(.86f,.94f,.92f,1));
        stats.transform.SetSiblingIndex(1);
        ModernText("ステータス", stats.rectTransform, 28, new Vector2(20,-16), new Vector2(370,40), ModernInk, FontStyles.Bold);
        ModernText("基礎値 ＋ 衣装ボーナス", stats.rectTransform, 22, new Vector2(20,-65), new Vector2(370,36), ModernMuted);
        var names = new[] { "魅力", "知力", "注意力", "攻撃力", "防御力" };
        var labelPos = new[] { new Vector2(168,-150), new Vector2(30,-216), new Vector2(290,-216), new Vector2(55,-344), new Vector2(266,-344) };
        for (var i=0;i<5;i++) ModernText(names[i],stats.rectTransform,22,labelPos[i],new Vector2(100,30),ModernInk);
        var wardrobe = ModernPanel(page,"WardrobeBackdrop",new Vector2(180,-638),new Vector2(1290,234),ModernPaper);
        wardrobe.transform.SetSiblingIndex(1);
        var outfitNames = new[] { "部屋着", "おでかけ", "仕事着", "サイバー", "フォーマル", "カジュアル" };
        for (var i=0;i<6;i++)
        {
            var card=Descendant(page,"OutfitCard"+i) as RectTransform;
            var icon=RawImageRoot("ClosetArtwork",card,DressArtworkPath,new Rect((244+i*201)/1672f,1-830/941f,186/1672f,169/941f));
            Stretch(icon.rectTransform,Vector2.zero,Vector2.zero);
            icon.transform.SetAsFirstSibling();
            ModernText(outfitNames[i],card,24,new Vector2(0,-170),new Vector2(186,34),ModernInk,FontStyles.Normal,TextAlignmentOptions.Center);
        }
        var comment=Descendant(page,"CommentText").GetComponent<TMP_Text>();
        comment.text="服を選ぶと、ReReが印象を教えてくれるよ。";
        comment.fontSize=26;
        Place(comment.transform,1094,218,260,100);
        var bubble=Descendant(page,"DynamicCommentBubble").GetComponent<Image>();
        bubble.sprite=GetDefaultSprite();
        bubble.preserveAspect=false;
        Place(bubble.transform,1088,212,280,110);
        var yes=Descendant(page,"YesButton");
        Place(yes,1110,336,270,48);
        yes.GetComponentInChildren<TMP_Text>().text="この服に着替える";
        Descendant(page,"ReReFaceImage").gameObject.SetActive(false);
        // The static concept image previously displayed a second protagonist.
        // Only StandingSpritePlaceholder is now visible inside the fitting room.
    }

    private static void PolishMapPage(RectTransform page)
    {
        for(var i=0;i<6;i++) Descendant(page,"MapShortcut"+i).gameObject.SetActive(false);
        var banner=Descendant(page,"MapDemoBanner");
        Place(banner,20,84,1460,64);
        Place(Descendant(page,"DemoReadyPill"),1266,9,178,46);
        Place(Descendant(page,"IsometricMapFrame"),20,164,934,628);
        var detail=Descendant(page,"MapLocationDetail") as RectTransform;
        Place(detail,974,164,506,628);
        Descendant(page,"MapDetailContent").gameObject.SetActive(false);
        var so=new SerializedObject(page.GetComponent<MapMenuController>());
        RelocateMapText(so,"detailNameText",detail,24,20,456,48,32);
        RelocateMapText(so,"detailDescriptionText",detail,24,82,456,82,24);
        var hintPanel=ModernPanel(detail,"ReadableHintPanel",new Vector2(16,-178),new Vector2(474,192),new Color(.90f,.86f,.96f,1));
        ModernText("ReReからのヒント",hintPanel.rectTransform,22,new Vector2(16,-12),new Vector2(442,30),ModernMuted);
        RelocateMapText(so,"rereHintText",detail,32,226,442,126,24);
        ModernText("関連する持ち物",detail,20,new Vector2(24,-390),new Vector2(456,30),ModernMuted);
        RelocateMapText(so,"relatedItemText",detail,24,426,456,36,24);
        RelocateMapText(so,"relatedCharacterText",detail,24,480,456,36,24);
        var go=Descendant(page,"MapGoButton");
        go.SetParent(detail,false);
        Place(go,24,550,456,58);
        go.GetComponentInChildren<TMP_Text>().fontSize=26;
        ModernText("ドラッグで地図を移動 / エリアを選択",page,20,new Vector2(400,-36),new Vector2(700,30),ModernMuted);
    }

    private static void RelocateMapText(SerializedObject so,string property,RectTransform parent,float x,float y,float w,float h,float font)
    {
        var text=so.FindProperty(property).objectReferenceValue as TMP_Text;
        text.transform.SetParent(parent,false);
        Place(text.transform,x,y,w,h);
        text.fontSize=font;
        text.alignment=TextAlignmentOptions.TopLeft;
    }

    private static void PolishQuestPage(RectTransform page)
    {
        // The existing two quest boards share a 1126px authoring grid.
        // Fit that grid uniformly into the new page; don't stretch Japanese glyphs.
        foreach(var name in new[]{"QuestInboxRoot","QuestCaseBoardRoot"})
        {
            var board=Descendant(page,name) as RectTransform;
            SetRect(board,new Vector2(0,1),new Vector2(0,1),new Vector2(0,1),new Vector2(60,20),new Vector2(1126,720));
            board.localScale=Vector3.one*1.23f;
        }
    }
}
