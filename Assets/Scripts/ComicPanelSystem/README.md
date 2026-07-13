# Comic Panel System

漫画コマを `ComicPanelLayout`（ScriptableObject）に登録し、`ComicPanelController` をCanvasに追加して使います。各コマの頂点は親RectTransform内の正規化座標（左下 `(0,0)`、右上 `(1,1)`）で、InspectorまたはScene viewのハンドルから編集できます。Sprite、強調/非強調色、暗さ、トランジション秒、発火目安行をコマごとに設定できます。

## Editor

`Tools/Comic Panel System/Create Demo Prefab` で、差し替え用の仮Sprite、`ComicPanelDemoLayout`、デモPrefabを `Assets/Resources/ComicPanelSystem/` に生成します。Prefabをシーンへ置き、`ComicPanelController` のLayoutを差し替えてください。複数のLayoutをNaninovelからID指定する場合は、LayoutをResources配下に置き、`Resources` からの相対パスを `id` に使います。

## Naninovel commands

物理行番号は監視せず、シナリオに明示コマンドを書きます。`panel` は1始まりです。

```text
@comicShow id:ComicPanelDemo
@comicShow id:ComicPanelDemo panel:2 mode:through
@comicFocus panel:2 mode:only time:0.25
@comicHide time:0.2
```

`mode:through` は指定コマまで、`mode:only` は指定コマだけを強調し、残りを非強調色/暗さで表示します。`@comicShow` の `panel` を省略すると全コマを強調します。ControllerやLayoutが未配置でもコマンドは警告だけで終了し、初期化前のNaninovel Engineサービス取得は行いません。
