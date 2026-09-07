using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MenuRootV2DiscoverabilityTests
{
    private const string PrefabPath = "Assets/NaninovelData/Resources/UI/MenuRootV2.prefab";

    [Test]
    public void TopPage_OffersOneActionMapEntryForFirstTarget()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.That(prefab, Is.Not.Null, "MenuRootV2 prefab should be generated before running this test.");

        var top = FindChild(prefab.transform, "PageTop");
        var mapButton = FindChild(top, "MapTileHitbox");
        Assert.That(mapButton, Is.Not.Null);
        var mapButtonComponent = mapButton.GetComponent<Button>();
        Assert.That(mapButtonComponent, Is.Not.Null);
        Assert.That(mapButton.GetComponent<MenuUIButtonHover>(), Is.Not.Null);
        Assert.That(mapButtonComponent.colors.pressedColor, Is.Not.EqualTo(mapButtonComponent.colors.normalColor));
        Assert.That(FindText(mapButton), Does.Contain("MAP"));

        var ui = prefab.GetComponent<MenuRootV2UI>();
        var serialized = new SerializedObject(ui);
        Assert.That(serialized.FindProperty("mapTileButton").objectReferenceValue, Is.EqualTo(mapButtonComponent));
    }

    [Test]
    public void MapPage_DescribesCafeRouteAndGoCondition()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.That(prefab, Is.Not.Null);

        var map = FindChild(prefab.transform, "PageMap");
        Assert.That(FindChild(map, "MapDemoBanner"), Is.Not.Null);
        var targetCard = FindChild(map, "MapDemoTargetCard");
        Assert.That(targetCard, Is.Not.Null);
        Assert.That(FindText(targetCard), Does.Contain("初回ターゲット").And.Contain("カフェ下調べ"));

        var goButton = FindChild(map, "MapGoButton");
        Assert.That(goButton, Is.Not.Null);
        Assert.That(goButton.GetComponent<MenuUIButtonHover>(), Is.Not.Null);
        Assert.That(FindText(goButton), Does.Contain("GO").And.Contain("カフェ下調べ"));

        var controller = map.GetComponent<MapMenuController>();
        var controllerSerialized = new SerializedObject(controller);
        Assert.That(controllerSerialized.FindProperty("initialLocationIndex").intValue, Is.EqualTo(3));
        var locations = controllerSerialized.FindProperty("locations");
        Assert.That(locations.arraySize, Is.EqualTo(6));
        Assert.That(locations.GetArrayElementAtIndex(3).FindPropertyRelative("baseName").stringValue, Is.EqualTo("カフェ下調べ"));

        var launcher = map.GetComponent<MapRouteLauncher>();
        var launcherSerialized = new SerializedObject(launcher);
        var routes = launcherSerialized.FindProperty("routes");
        Assert.That(routes.arraySize, Is.EqualTo(1));
        var route = routes.GetArrayElementAtIndex(0);
        Assert.That(route.FindPropertyRelative("enabled").boolValue, Is.True);
        Assert.That(route.FindPropertyRelative("locationIndex").intValue, Is.EqualTo(3));
        Assert.That(route.FindPropertyRelative("entryScriptPath").stringValue, Is.EqualTo("Scenario/PapaQuestDemo"));
    }

    [Test]
    public void PhoneFramesReplaceDashboardAndFreeInput()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.That(prefab, Is.Not.Null);

        var shell = FindChild(prefab.transform, "SharedLandscapeShell").GetComponent<RectTransform>();
        var portrait = FindChild(prefab.transform, "PortraitPhonePresentation").GetComponent<RectTransform>();
        Assert.That(shell.sizeDelta.x, Is.LessThanOrEqualTo(1920f));
        Assert.That(shell.sizeDelta.y, Is.LessThanOrEqualTo(1080f));
        Assert.That(portrait.anchoredPosition.x, Is.GreaterThan(400f));
        Assert.That(portrait.sizeDelta.y, Is.GreaterThan(portrait.sizeDelta.x));
        Assert.That(FindChild(shell, "OuterPhoneBezel"), Is.Not.Null);
        foreach (var bezel in prefab.GetComponentsInChildren<MenuPhoneBezel>(true))
            Assert.That(bezel.GetComponent<CanvasRenderer>(), Is.Not.Null);
        Assert.That(prefab.GetComponentInChildren<TMP_InputField>(true), Is.Null);
        Assert.That(prefab.GetComponentInChildren<ReReConversationUI>(true), Is.Null);
    }

    [Test]
    public void LandscapeNavigation_DoesNotStartTransition()
    {
        var host = new GameObject("PhoneTransitionTest");
        var first = new GameObject("First", typeof(RectTransform));
        var next = new GameObject("Next", typeof(RectTransform));
        try
        {
            var transition = host.AddComponent<MenuRootV2OrientationTransition>();
            transition.SetInitialPage(first, false);
            var applied = false;
            Assert.That(transition.RequestPage(next, false, () => applied = true), Is.True);
            Assert.That(applied, Is.True);
            Assert.That(transition.IsTransitioning, Is.False);
        }
        finally
        {
            Object.DestroyImmediate(host);
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(next);
        }
    }

    [Test]
    public void NavigationMarker_FollowsEveryPage()
    {
        var instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
        try
        {
            var ui = instance.GetComponent<MenuRootV2UI>();
            Object.DestroyImmediate(instance.GetComponent<MenuRootV2OrientationTransition>());
            var actions = new System.Action[] { ui.ShowTop, ui.ShowStatus, ui.ShowItems, ui.ShowCharacters, ui.ShowQuest, ui.ShowMap, ui.ShowSave, ui.ShowSettings };
            var names = new[] { "TopButton", "StatusButton", "ItemsButton", "CharactersButton", "QuestButton", "MapButton", "SaveButton", "SettingsButton" };
            var nav = FindChild(instance.transform, "PersistentNav");
            for (var index = 0; index < actions.Length; index++)
            {
                actions[index]();
                for (var other = 0; other < names.Length; other++)
                    Assert.That(FindChild(FindChild(nav, names[other]), "MenuPageActiveMark").gameObject.activeSelf, Is.EqualTo(index == other));
            }
        }
        finally { Object.DestroyImmediate(instance); }
    }

    [Test]
    public void QuestPage_OffersDemoStartAndCompanyRoute()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.That(prefab, Is.Not.Null);

        var quest = FindChild(prefab.transform, "PageQuest");
        var text = FindText(quest);
        Assert.That(text, Does.Contain("初回ターゲット"));
        Assert.That(text, Does.Contain("カフェ下調べ"));
        Assert.That(text, Does.Contain("条件: なし"));
        Assert.That(text, Does.Contain("会社パート"));
        Assert.That(text, Does.Not.Contain("メインクエストはありません"));

        var goButton = FindChild(quest, "GoToAreaButton");
        Assert.That(goButton, Is.Not.Null);
        Assert.That(FindText(goButton), Does.Contain("GO").And.Contain("CAFE"));
        Assert.That(goButton.GetComponent<MenuUIButtonHover>(), Is.Not.Null);

        var go = goButton.GetComponent<Button>();
        Assert.That(go.onClick.GetPersistentEventCount(), Is.GreaterThan(0));
        Assert.That(go.onClick.GetPersistentMethodName(0), Is.EqualTo(nameof(MenuRootV2UI.ShowMap)));

        var caseGo = FindChild(quest, "CaseGoToAreaButton").GetComponent<Button>();
        Assert.That(caseGo.onClick.GetPersistentEventCount(), Is.GreaterThan(0));
        Assert.That(caseGo.onClick.GetPersistentMethodName(0), Is.EqualTo(nameof(MenuRootV2UI.ShowMap)));
    }

    [Test]
    public void QuestStartButton_TargetsMapPage()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.That(prefab, Is.Not.Null);

        var instance = Object.Instantiate(prefab);
        try
        {
            var ui = instance.GetComponent<MenuRootV2UI>();
            Assert.That(ui, Is.Not.Null);
            // Coroutines don't advance in a plain EditMode test. Disable only
            // the visual transition so the persistent click route can be
            // asserted synchronously; animation behavior is covered separately.
            Object.DestroyImmediate(instance.GetComponent<MenuRootV2OrientationTransition>());
            ui.ShowQuest();

            var quest = FindChild(instance.transform, "PageQuest");
            var map = FindChild(instance.transform, "PageMap");
            Assert.That(quest.gameObject.activeSelf, Is.True);
            Assert.That(map.gameObject.activeSelf, Is.False);

            var click = FindChild(quest, "GoToAreaButton").GetComponent<Button>().onClick;
            Assert.That(click.GetPersistentTarget(0), Is.EqualTo(ui));
            Assert.That(click.GetPersistentMethodName(0), Is.EqualTo(nameof(MenuRootV2UI.ShowMap)));
            ui.ShowMap();

            Assert.That(quest.gameObject.activeSelf, Is.False);
            Assert.That(map.gameObject.activeSelf, Is.True);
        }
        finally
        {
            Object.DestroyImmediate(instance);
        }
    }

    [Test]
    public void CharactersPage_UsesDemoNamesAndVisibleIntelSnapshot()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.That(prefab, Is.Not.Null);

        var characters = FindChild(prefab.transform, "PageCharacters");
        var text = FindText(characters);
        Assert.That(text, Does.Contain("初回ターゲット"));
        Assert.That(text, Does.Contain("元取引先担当"));
        Assert.That(text, Does.Contain("呼称").And.Contain("行きつけ").And.Contain("承認欲求"));
        Assert.That(text, Does.Contain("17番席"));
        Assert.That(text, Does.Not.Contain("仮:").And.Not.Contain("仮：").And.Not.Contain("???"));
        Assert.That(FindChild(characters, "DemoInformationNodeRow3"), Is.Not.Null);

        var panel = characters.GetComponent<CharacterInformationNodePanel>();
        Assert.That(panel, Is.Not.Null);
        var serialized = new SerializedObject(panel);
        Assert.That(serialized.FindProperty("characterRowButtons").arraySize, Is.EqualTo(12));
        Assert.That(serialized.FindProperty("characterRowIndexes").arraySize, Is.EqualTo(12));
    }

    [Test]
    public void ItemsPage_LoadsInventoryDatabaseAndShowsRouteReadyCards()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.That(prefab, Is.Not.Null);

        var items = FindChild(prefab.transform, "PageItems");
        var text = FindText(items);
        Assert.That(text, Does.Contain("カフェ回数券"));
        Assert.That(text, Does.Contain("古びた鍵"));
        Assert.That(text, Does.Contain("免罪符"));
        Assert.That(text, Does.Contain("初回ターゲット準備"));

        var controller = items.GetComponent<ItemMenuController>();
        Assert.That(controller, Is.Not.Null);
        var serialized = new SerializedObject(controller);
        Assert.That(serialized.FindProperty("inventoryDatabase").objectReferenceValue, Is.Not.Null);
        var firstCard = FindChild(items, "ItemCard0");
        Assert.That(firstCard, Is.Not.Null);
        Assert.That(firstCard.GetComponent<MenuUIButtonHover>(), Is.Not.Null);
        Assert.That(firstCard.GetComponent<Button>().colors.pressedColor,
            Is.Not.EqualTo(firstCard.GetComponent<Button>().colors.normalColor));
    }

    private static Transform FindChild(Transform root, string name)
    {
        if (!root)
            return null;

        foreach (var child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
                return child;
        }

        return null;
    }

    private static string FindText(Transform root)
    {
        if (!root)
            return string.Empty;

        var texts = root.GetComponentsInChildren<TMP_Text>(true);
        var values = new string[texts.Length];
        for (var i = 0; i < texts.Length; i++)
            values[i] = texts[i].text;
        return string.Join(" ", values);
    }
}
