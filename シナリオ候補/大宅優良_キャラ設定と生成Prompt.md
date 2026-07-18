# 大宅優良（仮）キャラクター設定と生成Prompt

この設定は、旧「千津鰯夫」の役割を女性のギャル後輩へ置き換えるための案である。
Naninovel上の内部キャラクターIDは、既存実装との互換性を優先して当面 `Tizu` のままとする。

## 名前案

### 第一候補：大宅 優良（おおや・ゆら）

- 「大宅」の字面に「オタク」を忍ばせられる。
- 「優良」で「優しい」「良い人」を表せる。
- 「ゆら」は呼びやすく、明るいギャルの名前としても不自然になりにくい。
- 元ネタを知らない場面では普通の名前として読めるため、駄洒落が前へ出すぎない。

### 別案

- 宅見 優愛（たくみ・ゆあ）：自然な名前を優先。「宅」「優愛」で要素を残し、「巧み」と同音なのでPC技能ともつながる。
- 大宅 優愛（おおや・ゆあ）：「オタクに優しい」の意味が最も伝わりやすい。やや作為的。
- 御宅 優（みやけ・ゆう）：字面のネタは強いが、姓の読みを説明しないと伝わりにくい。

## 基本設定

- 二十一歳から二十三歳程度。罠美より後輩で、入社半年ほど。
- 一人称は「あーし」。敬語は使えるが、親しい相手には語尾がすぐ砕ける。
- 明るく人懐こい。先輩を軽くいじるが、相手が本当に傷ついたと分かれば即座に引く。
- PCの組み立て、周辺機器、ログ確認、簡単なネットワーク切り分けが得意。
- 見た目と技能の落差を本人は意外だと思っておらず、周囲の反応だけを面白がっている。
- 頼まれると助けるが、コーヒー、ランチ、焼肉などをちゃっかり対価として要求する。
- 「困っている人には優しく、羽振りのよい先輩にはもっと優しく」が冗談交じりの信条。

## デザイン方針

罠美が「黒髪・超ロング・直線的・眼鏡」なので、優良は「明るい髪・高い位置のサイドポニー・曲線的・裸眼」で対比させる。

### 識別記号

1. 暗い地毛を残したハニーブロンド
2. 高い位置で結んだ右サイドポニーテールと、外向きに跳ねた毛先
3. 左こめかみの青緑色のX字ヘアピン二本
4. 青緑色のつり目と、珊瑚色の長いネイル
5. Enterキー型チャームが付いたミント色の社員証ストラップ

色数を減らす絵柄でも、サイドポニー、X字ヘアピン、長いネイルの三点は残す。

## 服装

- 少し大きめの白い襟付きシャツ。袖を肘までまくる。
- 上のボタンを二つ開け、黒いキャミソールをのぞかせる。
- 珊瑚色の細いネクタイを緩く結ぶ。
- シャツは前だけ軽く入れ、片側の裾を出す。
- チャコールグレーの膝上ペンシルスカート、薄い黒タイツ、黒いショートブーツ。
- 小さな銀色フープピアス。派手すぎず、一般企業で注意されないぎりぎりに留める。

## 立ち絵用・自然言語Prompt

```text
A full-body character illustration of Yura Ooya, a cheerful 22-year-old Japanese gyaru office lady and junior employee who is secretly highly skilled with computers. She has warm lightly tanned skin, teal upturned eyes, long eyelashes, peach eye shadow, glossy lips, and a small visible canine when she smiles. Her honey-blonde hair has clearly visible dark brown roots and is tied into a high right-side ponytail, with layered face-framing strands, side-swept bangs, and softly flipped ends. Two crossed teal hairpins sit above her left temple. She wears a slightly oversized white collared shirt with rolled sleeves and the top two buttons open over a black camisole, a loosened thin coral necktie, a charcoal above-knee pencil skirt, sheer black tights, and black ankle boots. One side of her shirt is untucked. She has coral-colored long nails, small silver hoop earrings, and a mint employee lanyard with a tiny Enter-key charm. Relaxed confident posture, friendly teasing smile, fashionable but office-appropriate, no glasses, plain background.
```

## 立ち絵用・DanbooruタグPrompt

```text
1girl, solo, adult woman, 22yo, office lady, junior employee, gyaru, cheerful, friendly, teasing smile, light tan, honey blonde hair, dark roots, high side ponytail, right side ponytail, medium long hair, layered hair, flipped hair, side swept bangs, face framing hair, crossed hairpins, teal hairpin, teal eyes, tsurime, long eyelashes, peach eyeshadow, glossy lips, fang, white collared shirt, oversized shirt, sleeves rolled up, open collar, black camisole, loose necktie, coral necktie, partially untucked shirt, pencil skirt, above knee skirt, charcoal skirt, black pantyhose, ankle boots, coral nails, long fingernails, hoop earrings, id card, mint lanyard, keyboard key charm, relaxed posture, no glasses, plain background
```

## Character sheet用追記

上記Promptの末尾へ追加する。

```text
character sheet, multiple views, front view, side view, back view, three-quarter view, full body, expression chart, neutral background, consistent outfit, consistent hairstyle, no text, no labels
```

## 推奨表情

- `default`：人懐こい笑顔
- `teasing`：半目で先輩をからかう
- `bargaining`：指を一本立て、食事を要求する
- `surprised`：罠美が泣き出して笑顔が消える
- `awkward`：視線を逸らし、話題選びを後悔する
- `serious`：PC確認時。口元を閉じ、目だけ鋭くなる
- `puzzled`：ログに痕跡がなく眉を寄せる
- `concerned`：軽口を止めて罠美を見る
- `smug`：焼肉の約束を取り付けようとする

## Negative Prompt候補

```text
glasses, black hair, very long hair, twin tails, school uniform, schoolgirl, child, underage, heavy jewelry, extreme tan, revealing cleavage, nightclub outfit, maid outfit, multiple people, text, watermark
```
