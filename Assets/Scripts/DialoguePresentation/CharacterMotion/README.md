# Character Motion

`@charMotion id:Wanabi type:shock` は、明示的に指定した Naninovel のキャラクター actor にだけ感情表現用の短い motion を再生します。自動感情解析は行いません。立ち絵の差し替えや完成度に依存せず、既存 actor の Position / Scale / Rotation を使います。

## Setup

1. `MoshiReRe/Dialogue Presentation/Character Motion/Create Default Library` を実行して、`Assets/Resources/DialoguePresentation/CharacterMotion/DefaultCharacterMotionLibrary.asset` を生成します。
2. 必要なら `CharacterMotionController` を常駐 GameObject に追加し、別の `CharacterMotionLibrary` を割り当てます。未配置なら Resources のデフォルト library を使います。
3. Naninovel script から `@charMotion id:<actorId> type:<motionName>` を呼びます。

初期 motion は `shock`、`nervous`、`pressure`、`withdraw`、`awkwardGap` です。各 preset は position offset、scale multiplier、rotation、duration、ease、loop/repeat、returnToOrigin、段階数 (`steps`) を持ちます。`steps` が 0 の場合は連続 Tween、1 以上の場合は各区間を指定段階に分けた pixel-snapped 風の補間になります。

同じ actor に連続して実行すると前の motion をキャンセルしてから次を開始します。Naninovel の actor が存在しない場合は、その actor ID について簡潔な警告を一度だけ出して安全に終了します。キャンセル時、actor の除去時、または controller 無効化時は可能な範囲で元の pose に戻します。

## Example

`Examples/CharacterMotionExample.nani` に実行例があります。
