# Grok 4.5 tools for MoshiReRe

xAI APIを使うローカルCLI/WebUIです。

- `grok_agent.py`: Codexから委譲するCLI
- `web_server.py`: 会話履歴をローカル保存するWebUI
- `grok_media.py`: xAI Imagineの画像・動画生成クライアント

## APIキー

PowerShellで設定します。実キーをチャット、ソース、`.env.example`へ貼り付けないでください。

```powershell
$env:XAI_API_KEY = "xai-..."
$env:XAI_MODEL = "grok-4.5"
```

## WebUIの起動

```powershell
cd D:\Unity\MoshiReRe
.\tools\grok\run-web.ps1
```

ブラウザで <http://127.0.0.1:8787> を開きます。`index.html`を直接開くのではなく、必ずサーバー経由で開いてください。

## 添付ファイル

200KBの固定制限は撤去しました。既定値は以下です。

- 1ファイル: 25MB
- 1回のリクエストに含めるファイル合計: 50MB
- 1回のHTTPリクエスト: 64MB

制限はPowerShellで変更できます。画像はbase64 data URLとしてxAIへ送信されます。

```powershell
$env:GROK_MAX_FILE_BYTES = "52428800"        # 1ファイル50MB
$env:GROK_MAX_TOTAL_FILE_BYTES = "104857600" # 合計100MB
$env:GROK_MAX_REQUEST_BYTES = "134217728"    # リクエスト128MB
```

サーバー再起動後に反映されます。完全な無制限は、ブラウザのメモリ、HTTP転送、xAIモデルのコンテキスト上限にぶつかるため対応していません。

テキストファイルに加えて、PNG/JPEG/WebP/GIFなどの画像を添付できます。APIキー、`.env`、認証情報などは添付しないでください。

## Agentモード

WebUIのモード選択で `Agent（画像・動画生成）` を選びます。

Agentモードでは、Grok 4.5が必要に応じて次のローカルツールを呼び出します。

- `generate_image`: `grok-imagine-image-quality`
- `generate_video`: `grok-imagine-video`

例:

```text
Agentモードで、夕暮れの東京を走る電車の短い動画を作ってください。
```

画像を添付してから、次のように依頼できます。

```text
添付画像を元に、カメラがゆっくりズームする5秒動画を作ってください。
```

動画生成は非同期で、完了まで数分かかる場合があります。生成URLはxAI側の一時URLです。必要な場合は表示されたURLから早めに保存してください。

Agentモードはローカルファイルの直接編集やシェル実行は行いません。画像・動画生成だけをツール化しています。

## Imagine設定

```powershell
$env:XAI_IMAGE_MODEL = "grok-imagine-image-quality"
$env:XAI_VIDEO_MODEL = "grok-imagine-video"
$env:XAI_VIDEO_TIMEOUT_SECONDS = "900"
```

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

## 会話データ

会話と添付ファイルの内容は `tools/grok/data/conversations` に保存されます。このフォルダはGit管理対象外です。APIキーは保存されません。

## 設定一覧

| 環境変数 | 既定値 | 用途 |
| --- | --- | --- |
| `XAI_MODEL` | `grok-4.5` | チャットモデル |
| `XAI_IMAGE_MODEL` | `grok-imagine-image-quality` | 画像生成モデル |
| `XAI_VIDEO_MODEL` | `grok-imagine-video` | 動画生成モデル |
| `XAI_BASE_URL` | `https://api.x.ai/v1` | APIのベースURL |
| `XAI_REASONING_EFFORT` | 未指定 | `low` / `medium` / `high` |
| `XAI_TIMEOUT_SECONDS` | `120` | 通常APIのタイムアウト |
| `XAI_VIDEO_TIMEOUT_SECONDS` | `900` | 動画生成の待機上限 |
| `GROK_MAX_FILE_BYTES` | `25MB` | 1ファイルの上限 |
| `GROK_MAX_TOTAL_FILE_BYTES` | `50MB` | 添付合計の上限 |
| `GROK_MAX_REQUEST_BYTES` | `64MB` | HTTPリクエストの上限 |

## ローカル検証

```powershell
python -m unittest discover -s tools/grok -p "test_*.py"
python -m py_compile tools/grok/grok_client.py tools/grok/grok_media.py tools/grok/grok_agent.py tools/grok/web_server.py
```

xAI公式仕様: [画像生成](https://docs.x.ai/developers/model-capabilities/images/generation)、[動画生成](https://docs.x.ai/developers/model-capabilities/video/generation)、[Function Calling](https://docs.x.ai/developers/tools/function-calling)
