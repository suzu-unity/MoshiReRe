# 出勤、会社パートの必要素材と生成プロンプト

対象シナリオ：`出勤_会社案_修正稿.nani`

## 制作方針

- イベントCGは原則16:9、1920×1080基準で作る。
- 漫画風の二コマ、三コマ演出は、コマごとに別画像を生成してUnity側で組む。生成時点で斜め枠やコマ枠を入れない。
- 広告、購入画面、エラー画面の日本語は画像へ焼き込まず、Unity UIで重ねる。
- 罠美の固定要素は「長い黒髪、重いぱっつん前髪、黒縁眼鏡、三白眼、目の下の小さなほくろ、白いシャツ、濃紺のネクタイとタイトスカート、社員証」とする。
- 目の下のほくろは、今後左右どちらかに固定する。左右反転素材を使う場合も、設定上の位置が入れ替わらないよう注意する。

## 優先順位

### 最優先

1. 電車内の引き絵
2. 電車内の罠美の顔アップ
3. 通勤する足元
4. 朝日を背負った会社ビルと罠美の後ろ姿
5. 朝日を眩しがる罠美の顔アップ
6. 千津がPCを確認する一枚絵（PC画面は見せない）
7. 正常なPC画面をこちらへ見せる一枚絵
8. USBを抜いたスマートフォンとハート通知のアップ
9. 広告画面、偽購入画面、独自エラー画面

### 次点

1. 景気のよい罠美が千津へ大盤振る舞いを約束する回想
2. PCへ向かって仕事をする罠美
3. 終業後、USB接続したスマートフォンを横に置いてネットを見る罠美
4. 時計、修正済み付箋、減っていく未処理件数のモンタージュ素材

### 共通背景

- 朝の通勤電車内
- 朝のオフィス街と会社外観
- 会社執務室（朝、日中、定時後の三照明差分）
- 課長席または小会議室
- PC作業机の寄り背景

## キャラクターデザイン

### 千津鰯夫

二十二歳から二十四歳程度。入社半年の青年男性。ギーク気質で、人との距離感は少し雑だが、頼られると結局手を貸す。

#### 基本デザイン

- やや太めで、肩が丸い。立っているだけで少し猫背になる。
- 黒いボサボサ髪。前髪は長さが不揃いで、左右に小さく跳ねた毛束がある。
- 大きめの角丸四角形の黒縁眼鏡。左側のテンプルだけ青緑色にする。
- 白いシャツは少し皺があり、右側の裾だけ常にズボンから出ている。
- 袖は左右で少し違う高さまでまくっている。
- 暗いスラックス、緩んだ青緑色のネクタイ、青緑色の社員証ストラップ。
- 社員証ストラップに、小さな青い魚のピクセルチャームを付ける。
- 靴やPC用品は意外と手入れされている。単なる不潔な人物にはしない。

#### 絵柄を越えて残す識別記号

1. 大きな角丸黒縁眼鏡と、青緑色の左テンプル
2. 丸い体型と猫背
3. 右側だけ出たシャツの裾
4. 青緑色のストラップと魚チャーム

小物が省略される絵柄でも、眼鏡、体型、シャツの裾の三つで判別できる。

#### 必要な立ち絵表情

- `default`：気の抜けた薄い笑顔
- `expectant`：グラボをねだる期待顔
- `surprised`：罠美が泣き出して目を見開く
- `awkward`：笑顔が引きつり、視線が泳ぐ
- `teasing`：先輩をからかう半笑い
- `serious`：PC調査時。口を閉じ、眼鏡の奥だけ鋭くなる
- `puzzled`：痕跡が見つからず眉を寄せる
- `concerned`：軽口を止め、罠美を気遣う

#### キャラクターシート用プロンプト

自然言語：

```text
Character design sheet of Iwao Chizu, a 23-year-old Japanese male junior office worker with a geeky and slightly socially awkward personality. He is slightly chubby with rounded shoulders and a mild slouch. He has messy black hair with uneven bangs and small side tufts, large rounded-square black glasses with a distinctive cyan left temple arm, and tired but friendly dark eyes. He wears a slightly wrinkled white office shirt, a loose muted-cyan necktie, dark slacks, unevenly rolled sleeves, and the right shirt tail is always untucked. A cyan ID-card lanyard carries a tiny blue pixel-fish charm. He should look unfashionable but clean, approachable, and quietly competent with computers. Show front, side, back, three-quarter view, and a row of facial expressions on a plain neutral background. No text, no labels.
```

Danbooruタグ：

```text
1boy, adult male, character sheet, multiple views, expression chart, full body, japanese office worker, nerd, otaku, slightly chubby, round face, slouching, messy black hair, uneven bangs, hair tufts, black-framed eyewear, rounded-square eyewear, cyan eyewear temple, dark eyes, white collared shirt, wrinkled shirt, loose necktie, cyan necktie, rolled-up sleeves, uneven sleeves, untucked shirt, shirt tail out, dark pants, id card, cyan lanyard, fish charm, loafers, friendly, awkward smile, plain background, no text
```

### 町山課長

四十代後半から五十代前半。壮年の男性。営業または広告制作部門の管理職。厳めしい顔だが、怒鳴るより静かに具体的な指摘をする。既婚者。

#### 基本デザイン

- 背筋が伸びた長方形のシルエット。中肉で肩幅はやや広い。
- 黒に近い髪を七三に整え、右のこめかみに明確な銀色の毛束を入れる。
- 太く角度のついた眉、四角い顎、目尻と眉間に薄い皺。
- 眼鏡は使わない。千津との顔の区別を明確にする。
- チャコールグレーの三つ揃い、白いシャツ、深い臙脂色のネクタイ。
- 銀色の山形ネクタイピン。左手に結婚指輪。
- 厳格には見えるが、口元や目元に疲れと穏やかさを残す。悪役顔にはしない。

#### 絵柄を越えて残す識別記号

1. 七三分けと右こめかみの銀色の毛束
2. 太い角眉と四角い顎
3. 臙脂色のネクタイと銀色の山形ネクタイピン
4. 左手の結婚指輪

バストアップでは銀色の毛束、角眉、臙脂色のネクタイを必ず残す。

#### 必要な立ち絵表情

- `default`：厳めしい無表情
- `assessment`：資料と罠美を見比べる観察顔
- `quiet_pressure`：声を荒らげず眉だけを寄せる
- `explaining`：片手で資料を示しながら説明する
- `mild_approval`：わずかに口元を緩める
- `concerned`：残業しようとする罠美を心配する

#### キャラクターシート用プロンプト

自然言語：

```text
Character design sheet of Manager Machiyama, a Japanese married male office manager in his late forties. He has a stern, square face, a broad but not muscular build, thick angular eyebrows, a square jaw, subtle lines between the brows and around the eyes, and no glasses. His nearly black hair is immaculately parted seven-to-three with one unmistakable silver streak at the right temple. He wears a perfectly fitted charcoal three-piece suit, a white shirt, a deep burgundy tie, a small silver mountain-shaped tie clip, polished black shoes, and a wedding ring on his left hand. His posture is straight and disciplined. He looks intimidating at first glance but calm, observant, tired, and fundamentally considerate rather than villainous. Show front, side, back, three-quarter view, and a row of facial expressions on a plain neutral background. No text, no labels.
```

Danbooruタグ：

```text
1man, middle-aged man, character sheet, multiple views, expression chart, full body, japanese office worker, manager, stern, square face, square jaw, thick eyebrows, angular eyebrows, facial wrinkles, black hair, side-parted hair, slicked hair, grey streak, silver hair streak, no eyewear, charcoal suit, three-piece suit, vest, white collared shirt, burgundy necktie, tie clip, mountain-shaped accessory, wedding ring, polished shoes, straight posture, broad shoulders, calm, serious, plain background, no text
```

## 罠美に必要な表情差分

- `dead_eyes`：電車内、焦点の合わない三白眼
- `confused`：グラボの話を思い出せない
- `tearful`：金銭損失を思い出して静かに泣く
- `apology`：町山へ頭を下げる
- `listening`：説教を受ける無表情
- `irritated`：席へ戻った後の内心の苛立ち
- `focused`：仕事に集中する
- `stretch_tired`：定時後、背中を伸ばす
- `suspicious`：ポップアップを訝しむ
- `shocked`：偽購入画面
- `panic`：青いエラー画面
- `embarrassed_sweat`：千津へ助けを求める
- `puzzled`：消えたハート通知を見る

## イベントCG用プロンプト

### 1. 電車内の引き絵

自然言語：

```text
A horizontal 16:9 event illustration inside a crowded Japanese commuter train on a pale weekday morning. Wanabi, an adult Japanese office woman with very long straight black hair, heavy blunt bangs, black rectangular glasses, sanpaku eyes, and a small beauty mark under one eye, sits near the end of a bench with a hollow exhausted stare. She wears a white office shirt, dark navy necktie, navy pencil skirt, black pumps, and an ID-card lanyard. Other commuters stand and sit around her, absorbed in their phones. Include a generic household-budget advertisement above the seats but leave all text areas blank for later UI overlay. Emphasize the contrast between the ordinary morning commute and Wanabi's private financial disaster. Wide composition, no panel border, no readable text.
```

Danbooruタグ：

```text
1girl, adult woman, office lady, long black hair, very long hair, straight hair, blunt bangs, black-framed eyewear, rectangular eyewear, sanpaku, mole under eye, exhausted, empty eyes, white collared shirt, navy necktie, pencil skirt, id card, sitting, train interior, commuter train, crowded, commuters, morning, pale sunlight, wide shot, horizontal composition, 16:9, advertisement, blank sign, no text, no panel border
```

### 2. 電車内の罠美の顔アップ

自然言語：

```text
A tight close-up of Wanabi's face reflected faintly in a commuter-train window. She is an adult Japanese office woman with very long black hair, blunt bangs, black rectangular glasses, sanpaku eyes, and a small mole under one eye. Her eyes are unfocused from sleeplessness and financial shock, her mouth slightly open as she realizes she must get off at the next station. Morning train light, subtle reflections of straps and passing buildings, restrained deadpan comedy, horizontal 16:9 framing, no text, no panel border.
```

Danbooruタグ：

```text
1girl, adult woman, close-up, face focus, long black hair, blunt bangs, black-framed eyewear, rectangular eyewear, sanpaku, mole under eye, empty eyes, unfocused eyes, tired, parted lips, window reflection, train interior, morning light, deadpan, horizontal composition, 16:9, no text, no panel border
```

### 3. 通勤する足元

自然言語：

```text
A low-angle horizontal close shot of a tired female office worker's black pumps and navy pencil skirt hem walking slowly along a city sidewalk toward work. Her steps are heavy, one foot dragging slightly, while faster commuters pass as blurred legs around her. Cool morning pavement, long shadows, restrained composition, no face, no text, no panel border.
```

Danbooruタグ：

```text
1girl, lower body, feet focus, legs, black pumps, pencil skirt, walking, tired walk, low angle, city sidewalk, office district, commuters, motion blur, morning, long shadows, horizontal composition, 16:9, no face, no text, no panel border
```

### 4. 会社ビルを見上げる後ろ姿

自然言語：

```text
A horizontal 16:9 rear-view event illustration of Wanabi standing small at the foot of a tall modern Japanese office building. Her very long straight black hair and white office shirt are seen from behind. The building rises above her with the bright morning sun directly behind it, creating an imposing backlit silhouette and making the ordinary workplace feel absurdly monumental. Office district, clean city architecture, no text, no logo, no panel border.
```

Danbooruタグ：

```text
1girl, from behind, very long black hair, white collared shirt, office lady, standing, looking up, modern office building, skyscraper, office district, backlighting, sun behind building, lens flare, low angle, wide shot, small person, horizontal composition, 16:9, no text, no logo, no panel border
```

### 5. 朝日を眩しがる罠美

自然言語：

```text
A close-up of Wanabi squinting painfully into bright morning sunlight outside her office. She has very long black hair, heavy blunt bangs, black rectangular glasses, sanpaku eyes, and a small mole under one eye. The sunlight reflects harshly across her lenses; her exhausted face is on the verge of tears, allowing the later excuse that the morning sun made her cry. Horizontal 16:9 composition, simple office-building background, no text, no panel border.
```

Danbooruタグ：

```text
1girl, adult woman, close-up, long black hair, blunt bangs, black-framed eyewear, rectangular eyewear, sanpaku, mole under eye, squinting, teary eyes, exhausted, sunlight, glasses reflection, bright morning, office building background, horizontal composition, 16:9, no text, no panel border
```

### 6. 大盤振る舞いを約束する回想

自然言語：

```text
A monochrome office flashback showing Wanabi acting overconfident while cryptocurrency prices are rising. Wanabi, a long-haired Japanese office woman with black rectangular glasses, proudly leans toward her junior coworker and gestures as if promising to buy him anything he wants. Iwao Chizu, a slightly chubby young Japanese male office worker with messy black hair, large rounded-square black glasses with a cyan left temple, a wrinkled white shirt, loose cyan tie, cyan lanyard, and one untucked shirt tail, reacts with surprised greedy interest. Comedic contrast, workplace desks and monitors, horizontal 16:9 framing, no speech bubbles, no readable text.
```

Danbooruタグ：

```text
1girl, 1boy, office, flashback, monochrome, grayscale, adult woman, long black hair, blunt bangs, black-framed eyewear, smug, confident, gesturing, office lady, young adult male, slightly chubby, messy black hair, rounded-square eyewear, wrinkled shirt, loose cyan necktie, untucked shirt, cyan lanyard, surprised, greedy smile, desk, computer, horizontal composition, 16:9, no speech bubble, no text
```

### 7. PC作業中の罠美

自然言語：

```text
A side-view horizontal event illustration of Wanabi working efficiently at a desktop computer in a Japanese office. She has very long black hair, blunt bangs, black rectangular glasses, sanpaku eyes, a white office shirt, navy necktie, navy pencil skirt, and an ID lanyard. Her posture is slightly hunched but her hands move confidently between keyboard, mouse, printed corrections, and color proofs. Sticky notes are being crossed off and the office light shifts toward late afternoon. Focused, competent, quiet work rhythm, no readable screen text, 16:9.
```

Danbooruタグ：

```text
1girl, adult woman, office lady, side view, long black hair, blunt bangs, black-framed eyewear, sanpaku, white collared shirt, navy necktie, pencil skirt, id card, sitting, typing, computer, keyboard, mouse, focused, serious, office, desk, documents, sticky notes, late afternoon, horizontal composition, 16:9, no readable text
```

### 8. 終業後のネット閲覧とUSB接続

自然言語：

```text
A horizontal 16:9 side-view event illustration in a mostly empty Japanese office after working hours. Wanabi, a tired long-haired office woman with black rectangular glasses, leans back at her desktop computer and idly browses the web. A smartphone lies clearly visible beside the keyboard, connected to the PC by a visible USB cable. The phone battery indicator area is visible but contains no readable text. The monitor casts a cool light on her suspicious face as a small pop-up begins to appear in one corner. Quiet evening office, no readable text.
```

Danbooruタグ：

```text
1girl, adult woman, office lady, long black hair, blunt bangs, black-framed eyewear, tired, sitting, computer, browsing, office, empty office, evening, monitor glow, smartphone, phone on desk, usb cable, connected device, keyboard, suspicious, side view, horizontal composition, 16:9, no readable text
```

### 9. 千津がPCを確認する絵（画面は見せない）

自然言語：

```text
A horizontal 16:9 event illustration of Iwao Chizu examining Wanabi's office computer after a suspicious crash. Chizu is a slightly chubby 23-year-old Japanese male office worker with messy black hair, large rounded-square black glasses with a cyan left temple, a wrinkled white shirt, loose cyan tie, cyan ID lanyard with a tiny blue fish charm, dark slacks, and the right shirt tail untucked. He sits close to the desk with a serious focused expression, one hand on the mouse and the other near the keyboard. Wanabi stands anxiously behind him. The monitor faces away from the viewer, so the screen content is completely hidden. Visible USB cable and phone on desk, quiet tense office, no text.
```

Danbooruタグ：

```text
1boy, 1girl, office, computer, monitor from behind, screen not visible, young adult male, slightly chubby, messy black hair, rounded-square eyewear, cyan eyewear temple, wrinkled white shirt, loose cyan necktie, cyan lanyard, fish charm, untucked shirt, serious, focused, sitting, hand on mouse, hand on keyboard, adult woman, long black hair, anxious, standing behind, smartphone, usb cable, evening, horizontal composition, 16:9, no text
```

### 10. 正常なPC画面を見せる絵

自然言語：

```text
A front-facing close event shot of an office computer monitor being turned toward the viewer by a young male coworker's hand. The display shows an entirely ordinary clean desktop with a neutral wallpaper, a taskbar, and a few generic unlabeled icons, with no error message and no readable text. Part of Iwao Chizu's wrinkled white sleeve and cyan lanyard is visible at one edge, while Wanabi's anxious silhouette is visible at the other. The image must clearly communicate that the computer has already returned to normal. Horizontal 16:9 composition, no brand logos, no readable text.
```

Danbooruタグ：

```text
computer monitor, screen focus, front view, normal desktop, desktop wallpaper, generic icons, taskbar, no error, hand pointing, white sleeve, cyan lanyard, office desk, keyboard, anxious silhouette, evening office, close-up, horizontal composition, 16:9, no brand logo, no readable text
```

### 11. スマートフォンのハート通知

自然言語：

```text
A close-up horizontal event illustration of Wanabi's hand unplugging a USB cable from a black smartphone on an office desk. The phone screen has just lit up, showing a tiny mysterious heart-shaped notification icon at the edge of an otherwise ordinary lock screen. Keep the notification area simple and leave all text blank for a Unity UI overlay. Keyboard, mouse, and the cuff of a white office shirt are softly visible around it. Subtle unease rather than horror, evening office light, 16:9, no readable text.
```

Danbooruタグ：

```text
smartphone, phone screen, hand focus, female hand, unplugging, usb cable, heart icon, notification icon, lock screen, office desk, keyboard, mouse, white sleeve, close-up, evening light, mysterious, subtle horror, horizontal composition, 16:9, blank notification, no readable text
```

## 画面素材用プロンプト

### 頂き女子広告の背景

自然言語：

```text
A deliberately tacky but polished pop-up advertisement background for a fictional dating-advice money-making manual. A glamorous woman reclines in an exaggerated pile of banknotes while oversized pink hearts, gold sparkles, cheap luxury motifs, and an absurdly tiny close button crowd the frame. The composition should feel designed by someone targeting men even though the product is supposedly for women. Leave several clean blank panels for Japanese text to be added later in Unity. No readable text, no real currency details, no logos, 16:9.
```

Danbooruタグ：

```text
advertisement, popup, glamorous woman, banknotes, pile of money, pink hearts, gold sparkles, luxury, tacky, gaudy, tiny close button, interface, blank text box, empty text area, parody, horizontal composition, 16:9, no readable text, no logo
```

### 偽購入完了画面

自然言語：

```text
A cheerful fraudulent purchase-complete screen filled with pastel hearts, gold confetti, ribbons, celebratory sparkles, a premium membership badge, and an aggressively friendly confirmation panel. The design should be comically inappropriate for a sudden one-click purchase. Leave the main headline and price areas blank for Japanese UI text. No readable text, no logos, horizontal 16:9.
```

Danbooruタグ：

```text
purchase confirmation, user interface, popup, confetti, ribbon, pastel hearts, gold sparkles, premium badge, celebration, suspicious, scam, parody, blank headline, blank text box, horizontal composition, 16:9, no readable text, no logo
```

### 独自の青いエラー画面

自然言語：

```text
An original fictional computer crash screen dominated by deep blue and cyan, with broken geometric windows, scrambled loading bars, fragmented heart-shaped pixels, and abstract system-warning symbols. It must not resemble any real operating system error screen. Leave one empty area for a short warning message to be overlaid in Unity. Clean graphic design, unsettling digital noise, horizontal 16:9, no readable text, no real logos.
```

Danbooruタグ：

```text
computer error screen, fictional interface, blue screen, deep blue, cyan, glitch, digital noise, broken window, loading bar, fragmented heart, warning icon, abstract ui, blank text area, horizontal composition, 16:9, no readable text, no logo, original design
```

## 音素材

### 環境音

- 朝の走行中の電車音
- 車内アナウンス前後のチャイム
- 駅ホームの雑踏
- オフィスの空調、遠いキーボード音、プリンター音
- 定時後の静かなオフィス環境音

### 演出SE

- 通勤時の重い足音
- PCキーボード、マウスクリック
- 付箋または紙を机へ滑らせる音
- 背伸びした際の軽い関節音（台詞の「バキバキ」ではなくSEにする）
- USB接続音、切断音
- ポップアップ表示音
- 偽購入画面の場違いに明るいファンファーレ
- グリッチノイズ
- 独自エラー画面の短い警告音
- スマートフォンの小さな振動音
- ハート通知用の短い電子音

## Unityで別レイヤーにする素材

- 車内広告の文言
- 頂き女子広告の全テキスト
- 小さな「×」ボタンとクリック判定
- 偽購入画面の見出し、コース名、価格
- エラー画面の警告文
- スマートフォンのハート通知
- 必要なら「接続デバイスを設定しています」の通知

日本語を別レイヤーにすると、誤字修正、翻訳、分岐差分、通知の消失アニメーションを画像の再生成なしで調整できる。
