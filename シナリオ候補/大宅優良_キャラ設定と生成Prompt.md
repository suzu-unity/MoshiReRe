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

罠美が「黒髪・超ロング・直線的・黒縁眼鏡」なので、優良は「明るい外ハネボブ・淡色の強度近視眼鏡・気だるい姿勢」で対比させる。
勤務中は身支度を最低限で済ませた脱力系のギャル、私服ではメイクもアクセサリも強く決める二段構成にする。

### 識別記号

1. 生え際と分け目に二～三センチ見える暗褐色の地毛と、ミルクティーベージュの髪
2. 肩に触れる薄めのレイヤーボブ、弱い外ハネ、淡いミント色のインナーカラー
3. 左こめかみで一つのXを作る、くすみピンクのエナメルヘアピン二本
4. 右耳だけに付ける、ヘアピンと同色の小さなくすみピンクのイヤーカフ
5. 薄い金属フレームと、目が小さく見えるほど分厚い強度近視レンズ
6. ミント色の社員証ストラップと、ストラップ下部の小さなミント色基板ピン
7. くすみピンクの中程度のネイル

色数を減らす絵柄でも、暗い根元、分厚い眼鏡、X字ヘアピンの三点は残す。

## 服装

- 少し大きめの白い襟付きシャツ。袖を左右で違う高さまでまくる。
- 第一ボタンだけを開け、黒いレースキャミソールの縁を襟元へごく細くのぞかせる。
- 珊瑚色の細いネクタイを緩く結ぶ。
- シャツは前だけ軽く入れ、片側の裾だけを少し出す。
- チャコールグレーの膝丈ペンシルスカート、薄い黒タイツ、黒いローファー。
- ピアスは付けず、右耳の小さなイヤーカフだけにする。

## 立ち絵用・自然言語Prompt

```text
A full-body character illustration of Yura Ooya, a 22-year-old Japanese junior office worker with a subtle gyaru style, a languid posture, and a friendly teasing expression. Her natural dark brown hair has grown out visibly: show a clearly defined two-to-three-centimeter band of dark brown roots along the center part, crown, hairline, and both temples. The dyed lengths are milk-tea beige with a pale mint inner layer. Her hair is a thin shoulder-length layered bob with weakly flipped ends and slightly messy side-swept bangs. Exactly two dusty-pink enamel hairpins cross each other to form one clear X above her left temple; there are no other hairpins. A single small dusty-pink enamel ear cuff is attached to her right ear, matching the X hairpins; she wears no earrings. She has freckles, olive-brown half-lidded drooping eyes, natural makeup, and muted pink medium-length nails. She wears thin silver metal-frame glasses with extremely thick high-prescription myopia lenses, clearly visible thick lens edges, strong refraction distortion, and eyes that appear noticeably smaller through the lenses, like realistic bottle-bottom glasses. Her white collared shirt is modestly buttoned except for the first button, its sleeves are rolled to uneven heights, and a very narrow edge of a black lace camisole is visible at the collar. She wears a loosened muted-coral necktie, a charcoal knee-length pencil skirt, sheer black pantyhose, and black loafers. One shirt hem is slightly untucked. A mint employee lanyard holds her employee badge, with one small mint circuit-board enamel pin fixed directly above the badge. Realistic Japanese office wear, relaxed standing pose, plain neutral background.
```

## 立ち絵用・DanbooruタグPrompt

```text
1girl, solo, adult woman, 22yo, japanese office lady, junior employee, subtle gyaru, approachable, languid, slouching, friendly teasing smile, freckles, milk tea beige hair, dark brown roots, clearly visible dark roots, grown-out roots, two-tone hair, pale mint inner hair, shoulder-length hair, thin hair, layered bob, weakly flipped hair, slightly messy side-swept bangs, exactly two hairpins, crossed hairpins, single x-shaped hairpin arrangement, dusty pink hairpins, hairpin above left temple, no extra hairpins, olive brown eyes, tareme, sleepy eyes, half-closed eyes, long eyelashes, natural makeup, thin silver metal-frame glasses, high prescription glasses, strong myopia glasses, bottle-bottom glasses, extremely thick lenses, thick lens edges, refraction distortion, eyes smaller behind glasses, muted pink fingernails, medium fingernails, white collared shirt, first button undone, black lace camisole edge, sleeves rolled up unevenly, loosened muted coral necktie, slightly untucked shirt, charcoal knee-length pencil skirt, sheer black pantyhose, black loafers, mint lanyard, employee badge, single mint circuit-board enamel pin above badge, single dusty pink enamel ear cuff on right ear, no earrings, realistic office wear, standing, relaxed posture, plain background
```

### 眼鏡越しに見る表情用の追記

基本立ち絵では眼鏡を正しく掛け、厚いレンズを識別記号として見せる。からかう表情だけ、次を末尾へ追加する。

```text
glasses lowered slightly on nose, looking over glasses, half-lidded teasing gaze, eyes visible above the upper rim while thick lens edges remain visible
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
fully beige roots, pale roots, uniform hair color, hidden hair roots, black hair lengths, very long hair, ponytail, twin tails, extra hairpins, parallel hairpins, three hairpins, multiple earrings, hoop earrings, opaque glasses, sunglasses, thin lenses, normal lenses, oversized eyes behind glasses, visible bra, exposed underwear, cleavage, open shirt, miniskirt, school uniform, schoolgirl, child, underage, extreme tan, nightclub outfit, maid outfit, multiple people, text, watermark
```
