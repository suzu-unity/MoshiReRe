# メニュー用アイテム差し替え

`InventoryDatabase.asset` に登録した `InventoryItem` が、MenuRootV2 の一覧・詳細・ドラッグ表示・バッグ表示へ自動反映されます。

各 `InventoryItem` では次を差し替えてください。

- `id`: シナリオやセーブデータで使う固定ID
- `displayName`: UI表示名
- `icon`: 一覧とバッグに表示するSprite
- `detailImage`: 詳細表示用Sprite。未設定時は `icon` を使用
- `summary`: ReReの短いコメント
- `description`: 詳細説明

登録件数に応じて余分なカードは非表示になります。
Databaseが未設定または空の場合だけ、コード内の8件の仮データへフォールバックします。

現在の3件と画像は暫定素材です。
正式素材へ置き換える場合も、Prefabやスクリプトの修正は不要です。
