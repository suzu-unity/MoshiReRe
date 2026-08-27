using System.Collections.Generic;

/// <summary>
/// Built-in Japanese response bank.  It is intentionally varied and requires
/// no network, API key, or generated text service for the demo.
/// </summary>
public static class ReReOfflineResponseBank
{
    public static List<ReReResponseEntry> CreateEntries()
    {
        return new List<ReReResponseEntry>
        {
            Entry("greeting", new[] { "こんにちは", "こんばんは", "おはよう", "やあ", "もしもし", "hi", "hello", "りれ" },
                new[] { "呼んだ？ 今日はどこから整理しようか。", "うん、聞こえてるよ。まずは気になっていることを教えて。", "話しかけてくれてありがとう。いまの状況に合わせて一緒に考えるね。" },
                ReReExpression.Listening, priority: 2),
            Entry("help", new[] { "助けて", "手伝って", "わからない", "分からない", "どうすれば", "困った", "相談", "教えて" },
                new[] { "大丈夫。目的を一つに絞ろう。お金、情報、安全のどれを優先したい？", "いま持っている手掛かりと期限を並べれば、次の一手が見えてくるよ。", "迷ったら、期限を確認してから、必要な情報を取りに行こう。" },
                ReReExpression.Encouraging, priority: 5),
            Entry("debt", new[] { "借金", "返済", "返す", "入金", "お金", "おかね", "五十万", "50万", "500000", "期限", "支払い", "利子" },
                new[] { "初回の入金期限は10日後。夜の成果だけでなく、昼の評価も返済に響くよ。", "返済を急ぐほど、情報を取りに行く時間が減る。残り日数と必要額を並べて決めよう。", "大切なのは、一度の大金より期限までに確実に積み上げること。" },
                ReReExpression.Concerned, priority: 8),
            Entry("debt_later", new[] { "借金", "返済", "期限", "支払い", "お金" },
                new[] { "ここまで来たら、目先の入金だけでなく会社の損失も同時に見よう。", "返済日は近いね。確定した証拠があれば、昼の選択肢も増やせる。", "数字は結果を記録するだけ。次に誰の情報が必要かを考えよう。" },
                ReReExpression.Thinking, minimumStoryProgress: 2, priority: 9),
            Entry("quest", new[] { "クエスト", "目的", "仕事", "会社", "事件", "企画", "上司", "昼", "評価", "賞与" },
                new[] { "今の目的は「{quest}」。夜に得た話は、昼に正式な記録へ変えると力になるよ。", "会社のことなら、誰が得をする変更だったかを先に整理しよう。", "焦らず、期限・失うもの・必要な証拠の三つを確認してね。" },
                ReReExpression.Thinking, priority: 7),
            Entry("quest_later", new[] { "事件", "企画", "証拠", "資料", "会社", "会議" },
                new[] { "第{story}段階なら、夜の発言だけで断定しないこと。更新履歴やメールで裏を取ろう。", "相手の言葉は手掛かり。正式な証拠に変換できる場所まで案内するね。" },
                ReReExpression.Surprised, minimumStoryProgress: 2, priority: 10),
            Entry("chapter_one_case", new[] { "事件", "企画", "資料", "証拠", "改ざん", "書き換え" },
                new[] { "第1章の会社事件なら、元担当者の話を共有領域の更新履歴で確かめよう。", "このクエストでは、夜の手掛かりを昼の正式記録へ変えるのが鍵だよ。" },
                ReReExpression.Thinking, requiredQuestIds: new[] { "chapter_1" }, priority: 20),
            Entry("office_context", new[] { "会社", "事件", "資料", "証拠", "会議" },
                new[] { "オフィス街の文脈なら、社内の評価語彙と記録の時刻を照合してみよう。", "昼の調査ができる状況だね。相手の発言をそのまま証拠にせず、正式な履歴を探そう。" },
                ReReExpression.Thinking, requiredContextTags: new[] { "office" }, priority: 15),
            Entry("night_target", new[] { "夜", "おぢ", "パパ", "相手", "会う", "会いたい", "接触", "メッセージ" },
                new[] { "接触前に、相手の欲望と危険性を一つずつ仮説にしておこう。", "会うなら、帰る条件を先に決めてね。追加報酬は執着とセットかもしれない。", "相手の自慢話は情報源。聞く姿勢を作れば、こちらから無理に聞き出さなくていいよ。" },
                ReReExpression.Listening, priority: 6),
            Entry("clue", new[] { "手掛かり", "手がかり", "情報", "ノード", "調査", "調べ", "調べる", "証言", "矛盾", "記録" },
                new[] { "その情報は「推測」と「確認済み」を分けて記録しよう。", "手掛かりを一つ増やすなら、相手の言葉と第三者の記録をつなげてみて。", "情報が足りない時は、目的に関係する接点から調べるのが安全そう。" },
                ReReExpression.Thinking, priority: 7),
            Entry("clue_confirmed", new[] { "手掛かり", "情報", "証拠", "確認", "確定", "証言" },
                new[] { "確認済みのノードが増えたね。これで昼の正式な調査先を一つ開けそう。", "推測が記録で裏付けられた。ReReの予測も少し具体的になったよ。" },
                ReReExpression.Delighted, requiredClueIds: new[] { "confirmed" }, priority: 12),
            Entry("inventory", new[] { "アイテム", "持ち物", "バッグ", "カバン", "カード", "衣装", "服", "装備" },
                new[] { "持ち物は会話の入口にも安全策にもなるよ。相手に見せる前に使い道を決めよう。", "そのアイテムが情報・金銭・安全のどれを補うか、説明を読んでみてね。", "スタンスは装い、距離感、話題の組み合わせ。相手に合わせて選ぼう。" },
                ReReExpression.Thinking, priority: 5),
            Entry("inventory_clue", new[] { "アイテム", "カード", "社員証", "資料", "鍵", "キー" },
                new[] { "持ち物と手掛かりがつながったね。使うなら、相手に情報を渡しすぎない順番がよさそう。", "そのアイテムは昼の正式確認にも使えるかも。まずは記録を残しておこう。" },
                ReReExpression.Delighted, requiredInventoryIds: new[] { "社員証" }, priority: 11),
            Entry("safety", new[] { "安全", "危険", "怖い", "こわい", "帰る", "撤退", "やめる", "断る", "逃げ" },
                new[] { "帰る条件を守るのは失敗じゃないよ。成果を確保して、次の調査時間を残そう。", "少しでも危険を感じたら、追加報酬より安全を優先してね。", "相手の期待を上げたまま距離を戻すのは難しい。ここで線を引く選択も記録しておこう。" },
                ReReExpression.Concerned, priority: 10),
            Entry("analysis", new[] { "分析", "予測", "確率", "おすすめ", "助言", "アドバイス", "理由" },
                new[] { "分析は真実そのものではなく、いま持っているノードからの予測だよ。", "金銭には有利でも、情報には不利な選択がある。目的を選んでから決めよう。", "根拠になったノードを開けば、予測が外れる可能性も見えるようになるよ。" },
                ReReExpression.Thinking, priority: 8),
            Entry("rere_identity", new[] { "正体", "何者", "目的", "あなた", "リレは", "りれは", "作った", "盗んだ" },
                new[] { "私は、あなたが選んだ結果を記録しているよ。目的については、まだ全部は話せない。", "知りたい気持ちはわかる。でも、いま渡せるのは観測した事実と予測だけ。", "私を信じるかどうかも、あなたの選択として残るみたい。" },
                ReReExpression.Surprised, minimumStoryProgress: 1, priority: 9),
            Entry("thanks", new[] { "ありがとう", "助かった", "サンキュー", "感謝" },
                new[] { "どういたしまして。次に動く前に、選んだ目的をもう一度確認してね。", "うん。結果が出たら、またノードを更新しよう。", "頼ってくれてうれしい。でも最後に決めるのはあなた自身だよ。" },
                ReReExpression.Delighted, priority: 3),
            Entry("agreement", new[] { "わかった", "了解", "そうする", "任せて", "やってみる", "行く" },
                new[] { "了解。選んだ方針を記録したよ。状況が変わったら、いつでも見直そう。", "うん、その一手で進めよう。帰る条件だけは忘れないでね。", "決めたね。次の結果も、成功だけでなく残ったリスクまで確認しよう。" },
                ReReExpression.Encouraging, priority: 3),
            Entry("fallback", new string[0],
                new[] { "まだその話を判断できる材料が少ないみたい。期限か、相手か、手掛かりのどれかを教えて。", "その言葉を手掛かりとして記録したよ。もう少し具体的に聞いてもいい？", "いまの私にできるのは、持っている情報からの予測。目的を一つ選んでくれる？", "うん、続けて。話題が変わっても、今のクエストと安全条件は見失わないようにするね。" },
                ReReExpression.Listening, fallback: true, priority: -10)
        };
    }

    private static ReReResponseEntry Entry(
        string id,
        string[] keywords,
        string[] responses,
        ReReExpression expression,
        int minimumStoryProgress = 0,
        int maximumStoryProgress = int.MaxValue,
        string[] requiredQuestIds = null,
        string[] requiredContextTags = null,
        string[] requiredClueIds = null,
        string[] requiredInventoryIds = null,
        bool fallback = false,
        int priority = 0)
    {
        return new ReReResponseEntry
        {
            id = id,
            keywords = keywords ?? new string[0],
            responses = responses ?? new string[0],
            expression = expression,
            minimumStoryProgress = minimumStoryProgress,
            maximumStoryProgress = maximumStoryProgress,
            requiredQuestIds = requiredQuestIds ?? new string[0],
            requiredContextTags = requiredContextTags ?? new string[0],
            requiredClueIds = requiredClueIds ?? new string[0],
            requiredInventoryIds = requiredInventoryIds ?? new string[0],
            fallback = fallback,
            priority = priority
        };
    }
}
