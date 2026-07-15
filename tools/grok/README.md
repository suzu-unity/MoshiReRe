# Grok 4.5 tools for MoshiReRe

このフォルダには、xAI APIを使う2つのローカルツールがあります。

1. `grok_agent.py`: Codexが作業を委譲するためのCLI。対象ファイルを明示してGrokに渡し、回答やunified diffを標準出力へ返します。Grokがワークスペースへ直接書き込まないため、Codexが内容を確認してから適用できます。
2. `web_server.py`: ブラウザから会話するローカルWebUI。サーバーは既定で `127.0.0.1` にのみbindし、APIキーをブラウザへ送信しません。

## APIキーの設定

PowerShellで、現在のターミナルにだけ設定します。

```powershell
$env:XAI_API_KEY = "xai-..."
```

永続化する場合はPowerShellのユーザー環境変数、CIのsecret store、またはOSの資格情報管理を使ってください。実キーを `.env.example` やソースへ書かないでください。

## WebUI

```powershell
.	ools\grok\run-web.ps1
```

ブラウザで <http://127.0.0.1:8787> を開きます。ポートを変える場合:

```powershell
.	ools\grok\run-web.ps1 -Port 8788
```

## Codexからサブエージェントとして使う

まず対象ファイルだけを渡して分析させます。

```powershell
.	ools\grok\run-agent.ps1 `
  --mode analyze `
  --prompt "この変更の影響範囲と潜在バグを調べて" `
  --file AGENTS.md `
  --file Assets/Scripts/MenuSystem/MenuRootUI.cs
```

実装案をunified diffで返させる場合:

```powershell
.	ools\grok\run-agent.ps1 `
  --mode implement `
  --prompt "ESCキーでメニューを閉じる処理の不具合を修正して" `
  --file AGENTS.md `
  --file Assets/Scripts/MenuSystem/MenuEsc.cs
```

プロンプトをパイプから渡すこともできます。

```powershell
"このスクリプトをレビューして" | .\tools\grok\run-agent.ps1 --mode review --file Assets/Scripts/MoneySystem/MoneyManager.cs
```

`Library`, `Temp`, `Logs`, `Build`, `obj`, `.git` と秘密ファイルはコンテキストとして渡せません。ファイルへの直接変更、コミット、Unity操作は行わない設計です。

## 設定

| 環境変数 | 既定値 | 用途 |
| --- | --- | --- |
| `XAI_MODEL` | `grok-4.5` | xAIのモデルID |
| `XAI_BASE_URL` | `https://api.x.ai/v1` | APIのベースURL |
| `XAI_REASONING_EFFORT` | 未指定 | `low` / `medium` / `high` |
| `XAI_TIMEOUT_SECONDS` | `120` | HTTPタイムアウト |
| `XAI_MAX_OUTPUT_TOKENS` | 未指定 | 出力上限 |

## ローカル検証

ネットワークやAPIキーを使わないテスト:

```powershell
python -m unittest discover -s tools/grok -p "test_*.py"
```

Grok 4.5はxAIのChat Completions APIで `grok-4.5` として利用します。料金・提供地域・レート制限はxAI側で変更されるため、実運用前に公式ドキュメントとコンソールを確認してください。
