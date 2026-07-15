# Grok 4.5 tools for MoshiReRe

xAI APIを使うローカルツールです。

- `grok_agent.py`: Codexから委譲するCLI。明示したファイルを読み、分析・レビュー・unified diffを返します。
- `web_server.py`: 会話履歴をローカル保存するWebUI。APIキーはサーバーだけが読み込みます。

## APIキー

PowerShellで設定します。実キーをチャット、ソース、`.env.example`へ貼り付けないでください。

```powershell
$env:XAI_API_KEY = "xai-..."
$env:XAI_MODEL = "grok-4.5"
```

## WebUI

```powershell
cd D:\Unity\MoshiReRe
.\tools\grok\run-web.ps1
```

ブラウザで <http://127.0.0.1:8787> を開きます。`index.html`を直接開くのではなく、必ずサーバー経由で開いてください。

WebUIの機能:

- 会話一覧、新規作成、名前変更、削除
- 会話履歴のローカル保存（`tools/grok/data/conversations`）
- 相談、レビュー、編集案モード
- `.cs`, `.nani`, `.md`, `.json`などのテキストファイル添付
- 添付ファイル名を会話履歴に表示
- Grokの回答をコピー
- 編集案をdiffとして確認し、Codexへ渡す

会話と添付ファイルの内容はローカルJSONへ保存されます。APIキーは保存されません。添付したファイル内容はxAI APIへ送信されるため、秘密情報を含むファイルは添付しないでください。

## Codexから委譲するCLI

```powershell
.\tools\grok\run-agent.ps1 `
  --mode analyze `
  --prompt "この変更の影響範囲と潜在バグを調べて" `
  --file AGENTS.md `
  --file Assets/Scripts/MenuSystem/MenuRootUI.cs
```

編集案をunified diffで返させる場合:

```powershell
.\tools\grok\run-agent.ps1 `
  --mode implement `
  --prompt "ESCキーでメニューを閉じる処理を修正して" `
  --file AGENTS.md `
  --file Assets/Scripts/MenuSystem/MenuEsc.cs
```

`--mode` は `analyze`、`review`、`implement` のいずれかです。Grokはファイルを直接変更しません。Codexが結果を確認してから適用します。

## 設定

| 環境変数 | 既定値 | 用途 |
| --- | --- | --- |
| `XAI_MODEL` | `grok-4.5` | xAIのモデルID |
| `XAI_BASE_URL` | `https://api.x.ai/v1` | APIのベースURL |
| `XAI_REASONING_EFFORT` | 未指定 | `low` / `medium` / `high` |
| `XAI_TIMEOUT_SECONDS` | `120` | HTTPタイムアウト |
| `XAI_MAX_OUTPUT_TOKENS` | 未指定 | 出力上限 |

## ローカル検証

```powershell
python -m unittest discover -s tools/grok -p "test_*.py"
```

このテストはネットワークやAPIキーを使用しません。
