# シナリオ探索アセット生成台帳

生成日：2026-07-27

## 生成方式

画像は組み込みの `image_gen` を使用した。
背景は添付された自室と会社の画像を、画風と構図の参照として入力した。
キーアイテムは均一な緑背景で生成し、`remove_chroma_key.py` で透過PNGへ変換した。

## 背景の共通プロンプト

```text
2D横スクロール探索ゲーム用の横長背景。
参照画像の淡いクリーム、くすんだ水色、紺、ミント、薄桃の配色、細い濃灰色の輪郭、細密なピクセル調を維持する。
カメラへ正対するorthographic frontal elevationとし、見下ろし、アイソメトリック、強い一点透視を使わない。
下部20パーセント前後を、前景家具に塞がれない連続した歩行床にする。
インタラクション対象は背景へ自然に置き、輪郭、色差、余白で発見できるようにする。
モブは顔と衣装の細部を持たない薄暗い青灰色の影として描く。
主人公、UI、矢印、説明ラベル、透かし、可読テキストを描かない。
```

各画像では、この共通プロンプトへ美術仕様書に記載した部屋、遷移点、インタラクションを追加した。

## 背景ファイル

| 拠点 | ファイル | 役割 | 遷移 |
| --- | --- | --- | --- |
| 01 | `Backgrounds/01_vtuber_studio.png` | 配信スタジオと秘密ブース | 防音ドア |
| 02 | `Backgrounds/02_bakery_cafe.png` | ベーカリーと旧事務室 | 半透明ドア |
| 03 | `Backgrounds/03_smart_lounge.png` | スマートホテルラウンジ | 半個室ドア |
| 04 | `Backgrounds/04_funeral_hall.png` | 生前葬ホール | 控室ドア |
| 04 | `Backgrounds/04_funeral_archive.png` | 舞台裏と記録庫 | 搬入口、地下階段 |
| 05 | `Backgrounds/05_memory_study.png` | 元銀行員の書斎 | 玄関ドア |
| 06 | `Backgrounds/06_magic_theater.png` | 小劇場 | 楽屋口 |
| 06 | `Backgrounds/06_magic_backstage.png` | 舞台裏と契約机 | 搬入口 |
| 07 | `Backgrounds/07_rental_home.png` | レンタル家族のモデルハウス | 二階階段 |
| 07 | `Backgrounds/07_contract_office.png` | 衣装部屋と契約事務所 | 階段口 |
| 08 | `Backgrounds/08_island_lodge.png` | 離島ロッジ | 宿泊廊下、地下階段 |
| 08 | `Backgrounds/08_island_underground.png` | 発電機室と通信設備 | 地下階段 |
| 08 | `Backgrounds/08_island_dock.png` | 船着場 | ロッジ方面、桟橋 |
| 09 | `Backgrounds/09_crisis_conference.png` | 危機管理会社の会議室 | 防音ドア |
| 09 | `Backgrounds/09_audio_archive.png` | 音声記録庫 | 会議室ドア、非常口 |
| 10 | `Backgrounds/10_shelter_showroom.png` | 防災設備ショールーム | 気密扉、地下階段 |
| 10 | `Backgrounds/10_underground_bunker.png` | 地下シェルター | 気密扉、脱出梯子 |

## キーアイテムの共通プロンプト

```text
拠点固有のキーアイテムを、互いに接触しない横一列のスプライトシートとして生成する。
淡いクリーム、紺、ミント、薄桃と細い濃灰色輪郭を使う、細密なピクセルアート。
96ピクセル程度へ縮小しても用途が形で判別できるようにする。
背景は完全に均一な#00ff00とし、影、床面、反射、枠、スロット、ラベル、文字、人物を描かない。
```

## キーアイテムシート

| 拠点 | シート | 個数 |
| --- | --- | ---: |
| 01 | `Items/01_items.png` | 3 |
| 02 | `Items/02_items.png` | 3 |
| 03 | `Items/03_items.png` | 3 |
| 04 | `Items/04_items.png` | 3 |
| 05 | `Items/05_items.png` | 3 |
| 06 | `Items/06_items.png` | 3 |
| 07 | `Items/07_items.png` | 3 |
| 08 | `Items/08_items.png` | 4 |
| 09 | `Items/09_items.png` | 3 |
| 10 | `Items/10_items.png` | 3 |

## 個別キーアイテム

| 拠点 | ファイル |
| --- | --- |
| 01 | `01_studio_card_key.png`、`01_cosmetics_pouch.png`、`01_hash_storage.png` |
| 02 | `02_hospital_custody_ticket.png`、`02_machine_serial_plate.png`、`02_response_cards.png` |
| 03 | `03_emoticon_napkin.png`、`03_romance_ai_history_phone.png`、`03_style_comparison_notes.png` |
| 04 | `04_seating_chart.png`、`04_driver_logbook.png`、`04_ceremony_photo.png` |
| 05 | `05_banknote_number_ledger.png`、`05_duplicate_aid_envelope.png`、`05_transfer_stub.png` |
| 06 | `06_trunk_key.png`、`06_original_contract.png`、`06_security_memory_card.png` |
| 07 | `07_family_scripts.png`、`07_costume_tags.png`、`07_phantom_staff_invoice.png` |
| 08 | `08_generator_log.png`、`08_satellite_terminal.png`、`08_emergency_food_seal.png`、`08_encrypted_storage.png` |
| 09 | `09_apology_script.png`、`09_audio_cassette.png`、`09_crisis_response_binder.png` |
| 10 | `10_defect_serial_plate.png`、`10_oxygen_calibration_tag.png`、`10_inspection_report.png` |

個別ファイルはすべて `Items/Individual` にあり、512ピクセル四方の透過PNGである。

## 採否

目視評価は `Reviews/visual_review.md` に記録した。
不採用になった初回版とクロマキー素材は `tmp/ScenarioExplorationIntermediates` へ移し、ゲーム側から参照する安定ファイル名には採用版だけを置いた。
