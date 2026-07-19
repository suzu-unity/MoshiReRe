# Typing blip

`Dialogue.prefab` のルートに `TypingBlipController` と AudioSource を設定済みです。`DefaultTypingBlipProfile` にはReRe（高め）、Wanabi（低め）、未指定キャラクター用フォールバックが含まれます。Clip が空のエントリーはビルド互換の短いランタイム合成音を使うため、外部音源なしでも動作します。Naninovel Engine初期化後にだけ `ITextPrinterManager` へ購読し、`OnPrintStarted/Finished` と `ITextPrinterActor.RevealProgress` を使って表示進捗に合わせて `AudioSource.PlayOneShot` を呼びます。

句読点・空白・TMPリッチテキストタグ、スキップ中・瞬間表示プリンター・表示中のバックログは対象外です。Clip を空にしたエントリーは既定の合成音を使い、専用Clipを設定すればそれを優先します。`TypingBlipProfile` の Entries に author ID別設定を追加し、空のAuthor Idをフォールバックにしてください。ReReは専用プロファイルを割り当てれば、電子音向けのpitch/interval等を独立して調整できます。

## Editorプリセット

メニューから以下を実行すると、音声未設定の初期プロファイルを作成します。

- `Tools/MoshiReRe/Typing Blip/Create Male Profile`
- `Tools/MoshiReRe/Typing Blip/Create Female Profile`
- `Tools/MoshiReRe/Typing Blip/Create ReRe Electronic Profile`

既存アセットがある場合は上書きせず選択します。

## Unity利用例

1. 空のGameObject（例: `DialogueAudio`）に `TypingBlipController` を追加します。
2. `TypingBlipProfile` を作成し、Entriesの `Author Id` をNaninovelのキャラクターactor ID（例: `ReRe`）に合わせます。未指定キャラクター用に空のAuthor Idを1件置けます。
3. 各Entryへ短いAudioClipを設定し、コンポーネントを常駐オブジェクトへ置きます。

## Naninovel例

`Examples/TypingBlipExample.nani` は通常の台詞表示例です。コードやシーンへの自動接続は行わないため、既存のNaninovel UI構成を変更しません。
