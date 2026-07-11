using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DressMenuController : MonoBehaviour
{
    [Serializable]
    public struct OutfitData
    {
        public string id;
        public string displayName;
        public Sprite closetIcon;
        public Sprite standingSprite;
        [TextArea] public string rereComment;
        public int guts;
        public int intelligence;
        public int attention;
        public int attack;
        public int defense;
    }

    [Serializable]
    private struct OutfitTalkSet
    {
        public string outfitId;
        public Sprite[] frames;
    }

    [Serializable]
    private struct OutfitChangeSet
    {
        public string outfitId;
        public Sprite[] frames;
    }

    [SerializeField] private Button[] outfitButtons;
    [SerializeField] private RectTransform[] outfitHighlights;
    [SerializeField] private Image[] bonusFills;
    [SerializeField] private TMP_Text[] bonusValueTexts;
    [SerializeField] private Image[] outfitIconImages;
    [SerializeField] private TMP_Text commentText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private Image rereFaceImage;
    [SerializeField] private Sprite[] rereTalkFrames;
    [SerializeField] private OutfitTalkSet[] outfitTalkSets;
    [SerializeField] private float typewriterInterval = 0.035f;
    [SerializeField] private float talkFrameInterval = 0.14f;
    [SerializeField] private RadarChart baseRadar;
    [SerializeField] private RadarChart outfitRadar;
    [SerializeField] private RectTransform curtainLeft;
    [SerializeField] private RectTransform curtainRight;
    [SerializeField] private Image curtainLeftImage;
    [SerializeField] private Image curtainRightImage;
    [SerializeField] private Sprite[] curtainLeftFrames;
    [SerializeField] private Sprite[] curtainRightFrames;
    [SerializeField] private OutfitChangeSet[] outfitChangeSets;
    [SerializeField] private float curtainFrameInterval = 0.055f;
    [SerializeField] private float outfitFrameInterval = 0.13f;
    [SerializeField] private Image fittingSpriteTint;
    [SerializeField] private OutfitData[] outfits;

    private int selectedIndex;
    private int appliedIndex = -1;
    private int displayedOutfitIndex;
    private Coroutine changingRoutine;
    private Coroutine commentRoutine;
    private Coroutine talkRoutine;
    private readonly int[] appliedBonus = new int[5];
    private static readonly int[] DressBaseStats = { 1, 1, 1, 1, 1 };

    private void Awake()
    {
        AutoWire();
        EnsureDefaultOutfits();
        BindButtons();
        selectedIndex = 0;
        displayedOutfitIndex = 0;
        RefreshAppliedOutfitSprite();
        SelectOutfit(0);
    }

    private void OnEnable()
    {
        RefreshBaseRadar();
        RefreshOutfitPreview();
    }

    private void OnDestroy()
    {
        UnbindButtons();
    }

    private void OnDisable()
    {
        if (commentRoutine != null)
        {
            StopCoroutine(commentRoutine);
            commentRoutine = null;
        }

        StopTalkAnimation();
    }

    private void AutoWire()
    {
        if (outfitButtons == null || outfitButtons.Length == 0)
        {
            var buttons = new System.Collections.Generic.List<Button>();
            for (int i = 0; ; i++)
            {
                var t = transform.Find("OutfitCards/OutfitCard" + i);
                if (!t) break;
                var button = t.GetComponent<Button>();
                if (button) buttons.Add(button);
            }
            outfitButtons = buttons.ToArray();
        }

        if (outfitHighlights == null || outfitHighlights.Length == 0)
        {
            var highlights = new System.Collections.Generic.List<RectTransform>();
            for (int i = 0; i < outfitButtons.Length; i++)
            {
                var h = outfitButtons[i] ? outfitButtons[i].transform.Find("SelectedFrame") : null;
                highlights.Add(h as RectTransform);
            }
            outfitHighlights = highlights.ToArray();
        }

        if (bonusFills == null || bonusFills.Length == 0)
        {
            bonusFills = new Image[5];
            for (int i = 0; i < bonusFills.Length; i++)
            {
                var fill = transform.Find("OutfitBonusPanel/BonusMeterBg" + i + "/Fill");
                bonusFills[i] = fill ? fill.GetComponent<Image>() : null;
            }
        }

        if (bonusValueTexts == null || bonusValueTexts.Length == 0)
        {
            bonusValueTexts = new TMP_Text[5];
            for (int i = 0; i < bonusValueTexts.Length; i++)
            {
                var value = FindChild("BonusValueText" + i);
                bonusValueTexts[i] = value ? value.GetComponent<TMP_Text>() : null;
            }
        }

        if (outfitIconImages == null || outfitIconImages.Length == 0)
        {
            outfitIconImages = new Image[outfitButtons.Length];
            for (int i = 0; i < outfitIconImages.Length; i++)
            {
                var icon = FindChild("OutfitIconDynamic" + i);
                outfitIconImages[i] = icon ? icon.GetComponent<Image>() : null;
            }
        }

        if (!commentText)
        {
            var comment = FindChild("CommentText");
            commentText = comment ? comment.GetComponent<TMP_Text>() : null;
        }

        if (!yesButton)
        {
            var yes = FindChild("YesButton");
            yesButton = yes ? yes.GetComponent<Button>() : null;
        }

        if (!noButton)
        {
            var no = FindChild("NoButton");
            noButton = no ? no.GetComponent<Button>() : null;
        }

        if (!rereFaceImage)
        {
            var face = FindChild("ReReFaceImage");
            rereFaceImage = face ? face.GetComponent<Image>() : null;
        }

        if (!baseRadar)
        {
            var radar = transform.Find("RadarPanel/BaseStatusRadar");
            baseRadar = radar ? radar.GetComponent<RadarChart>() : null;
        }

        if (!outfitRadar)
        {
            var radar = transform.Find("RadarPanel/OutfitAdjustedRadar");
            outfitRadar = radar ? radar.GetComponent<RadarChart>() : null;
        }

        if (!curtainLeft)
        {
            var t = FindChild("ChangingCurtainLeft");
            curtainLeft = t as RectTransform;
        }

        if (!curtainLeftImage && curtainLeft)
            curtainLeftImage = curtainLeft.GetComponent<Image>();

        if (!curtainRight)
        {
            var t = FindChild("ChangingCurtainRight");
            curtainRight = t as RectTransform;
        }

        if (!curtainRightImage && curtainRight)
            curtainRightImage = curtainRight.GetComponent<Image>();

        if (!fittingSpriteTint)
        {
            var tint = FindChild("StandingSpritePlaceholder");
            fittingSpriteTint = tint ? tint.GetComponent<Image>() : null;
        }
    }

    private Transform FindChild(string childName)
    {
        foreach (var rect in GetComponentsInChildren<RectTransform>(true))
        {
            if (rect.name == childName)
                return rect.transform;
        }

        return null;
    }

    private void EnsureDefaultOutfits()
    {
        if (outfits != null && outfits.Length > 0)
            return;

        outfits = new[]
        {
            Outfit("room", "部屋着", "落ち着いて動ける基本コーデだよ。今日の様子見にはこれが無難かも。これにする？", 1, 1, 2, 0, 1),
            Outfit("date", "おでかけ", "印象を少し盛れる服だね。会話前に着ると良さそう。これにする？", 1, 2, 1, 2, 0),
            Outfit("work", "仕事着", "知的に見せたい時向きかな。判断ミスも減りそう。これにする？", 0, 3, 2, 1, 1),
            Outfit("cyber", "サイバー", "AI同期高めの攻めコーデ。ちょっと目立つけど強いよ。これにする？", 2, 1, 2, 3, 0),
            Outfit("formal", "フォーマル", "守りが固くて安定感あり。大事な場面に良いかも。これにする？", 1, 2, 1, 0, 3),
            Outfit("casual", "カジュアル", "自然体でいける服だね。リスクを抑えたい時に便利。これにする？", 2, 0, 1, 1, 2)
        };
    }

    private static OutfitData Outfit(string id, string name, string comment, int guts, int intelligence, int attention, int attack, int defense)
    {
        return new OutfitData
        {
            id = id,
            displayName = name,
            rereComment = comment,
            guts = guts,
            intelligence = intelligence,
            attention = attention,
            attack = attack,
            defense = defense
        };
    }

    private void BindButtons()
    {
        UnbindButtons();

        for (int i = 0; i < outfitButtons.Length; i++)
        {
            int index = i;
            if (outfitButtons[i])
                outfitButtons[i].onClick.AddListener(() => SelectOutfit(index));
        }

        if (yesButton) yesButton.onClick.AddListener(ApplySelectedOutfit);
        if (noButton) noButton.onClick.AddListener(CancelSelection);
    }

    private void UnbindButtons()
    {
        if (outfitButtons != null)
        {
            foreach (var button in outfitButtons)
            {
                if (button)
                    button.onClick.RemoveAllListeners();
            }
        }

        if (yesButton) yesButton.onClick.RemoveListener(ApplySelectedOutfit);
        if (noButton) noButton.onClick.RemoveListener(CancelSelection);
    }

    private void SelectOutfit(int index)
    {
        if (outfits == null || outfits.Length == 0)
            return;

        selectedIndex = Mathf.Clamp(index, 0, outfits.Length - 1);
        RefreshOutfitPreview();
    }

    private void RefreshOutfitPreview()
    {
        if (outfits == null || outfits.Length == 0)
            return;

        var outfit = outfits[selectedIndex];
        PlayComment(outfit.rereComment, true);
        RefreshOutfitIcons();

        var bonus = ToArray(outfit);
        for (int i = 0; i < 5; i++)
        {
            if (bonusValueTexts != null && i < bonusValueTexts.Length && bonusValueTexts[i])
                bonusValueTexts[i].text = "+" + bonus[i];

            if (bonusFills != null && i < bonusFills.Length && bonusFills[i])
            {
                var rect = bonusFills[i].rectTransform;
                var parentWidth = rect.parent is RectTransform parent ? parent.rect.width : 210f;
                var fillWidth = Mathf.Max(0f, parentWidth - 8f) * Mathf.Clamp01(bonus[i] / 6f);
                rect.sizeDelta = new Vector2(fillWidth, rect.sizeDelta.y);
            }
        }

        if (outfitHighlights != null)
        {
            for (int i = 0; i < outfitHighlights.Length; i++)
            {
                if (outfitHighlights[i])
                    outfitHighlights[i].gameObject.SetActive(i == selectedIndex);
            }
        }

        RefreshBaseRadar();
        RefreshOutfitRadar();
    }

    public void SetOutfits(OutfitData[] newOutfits, int initialIndex = 0)
    {
        if (newOutfits == null || newOutfits.Length == 0)
            return;

        outfits = newOutfits;
        SelectOutfit(Mathf.Clamp(initialIndex, 0, outfits.Length - 1));
    }

    private void RefreshOutfitIcons()
    {
        for (int i = 0; outfitIconImages != null && i < outfitIconImages.Length && i < outfits.Length; i++)
        {
            if (!outfitIconImages[i])
                continue;

            outfitIconImages[i].sprite = outfits[i].closetIcon;
            outfitIconImages[i].gameObject.SetActive(outfits[i].closetIcon != null);
        }
    }

    private void RefreshAppliedOutfitSprite()
    {
        if (!fittingSpriteTint || outfits == null || outfits.Length == 0)
            return;

        var index = Mathf.Clamp(displayedOutfitIndex, 0, outfits.Length - 1);
        if (outfits[index].standingSprite)
        {
            fittingSpriteTint.sprite = outfits[index].standingSprite;
            fittingSpriteTint.color = Color.white;
        }
        else
        {
            var frames = GetChangeFrames(index);
            if (frames.Length > 0)
            {
                fittingSpriteTint.sprite = frames[0];
                fittingSpriteTint.color = Color.white;
            }
        }
    }

    private void PlayComment(string message, bool showConfirmWhenComplete)
    {
        if (commentRoutine != null)
            StopCoroutine(commentRoutine);

        commentRoutine = StartCoroutine(TypeCommentRoutine(message, showConfirmWhenComplete));
    }

    private IEnumerator TypeCommentRoutine(string message, bool showConfirmWhenComplete)
    {
        if (yesButton)
            yesButton.gameObject.SetActive(false);

        StartTalkAnimation();

        if (commentText)
        {
            commentText.text = string.Empty;
            for (int i = 0; i < message.Length; i++)
            {
                commentText.text += message[i];
                yield return new WaitForSecondsRealtime(typewriterInterval);
            }
        }

        StopTalkAnimation();

        if (yesButton)
            yesButton.gameObject.SetActive(showConfirmWhenComplete);
    }

    private void StartTalkAnimation()
    {
        var frames = GetCurrentTalkFrames();
        if (!rereFaceImage || frames.Length == 0)
            return;

        if (talkRoutine != null)
            StopCoroutine(talkRoutine);

        talkRoutine = StartCoroutine(TalkAnimationRoutine());
    }

    private void StopTalkAnimation()
    {
        if (talkRoutine != null)
        {
            StopCoroutine(talkRoutine);
            talkRoutine = null;
        }

        var frames = GetCurrentTalkFrames();
        if (rereFaceImage && frames.Length > 0)
            rereFaceImage.sprite = frames[0];
    }

    private IEnumerator TalkAnimationRoutine()
    {
        var frames = GetCurrentTalkFrames();
        if (frames.Length == 0)
            yield break;

        var index = 0;
        while (true)
        {
            rereFaceImage.sprite = frames[index % frames.Length];
            index++;
            yield return new WaitForSecondsRealtime(talkFrameInterval);
        }
    }

    private Sprite[] GetCurrentTalkFrames()
    {
        if (outfits != null && selectedIndex >= 0 && selectedIndex < outfits.Length && outfitTalkSets != null)
        {
            var id = outfits[selectedIndex].id;
            for (int i = 0; i < outfitTalkSets.Length; i++)
            {
                if (outfitTalkSets[i].outfitId == id && outfitTalkSets[i].frames != null && outfitTalkSets[i].frames.Length > 0)
                    return outfitTalkSets[i].frames;
            }
        }

        return rereTalkFrames ?? Array.Empty<Sprite>();
    }

    private void RefreshBaseRadar()
    {
        if (!baseRadar)
            return;

        baseRadar.transform.SetAsLastSibling();
        baseRadar.SetValues(DressBaseStats[0], DressBaseStats[1], DressBaseStats[2], DressBaseStats[3], DressBaseStats[4]);
        baseRadar.GenerateMesh();
    }

    private void RefreshOutfitRadar()
    {
        if (!outfitRadar)
            return;

        outfitRadar.transform.SetAsFirstSibling();
        outfitRadar.SetValues(
            DressBaseStats[0] + appliedBonus[0],
            DressBaseStats[1] + appliedBonus[1],
            DressBaseStats[2] + appliedBonus[2],
            DressBaseStats[3] + appliedBonus[3],
            DressBaseStats[4] + appliedBonus[4]);
        outfitRadar.GenerateMesh();
    }

    private void ApplySelectedOutfit()
    {
        if (changingRoutine != null)
            StopCoroutine(changingRoutine);

        changingRoutine = StartCoroutine(ChangeOutfitRoutine());
    }

    private IEnumerator ChangeOutfitRoutine()
    {
        if (yesButton)
            yesButton.gameObject.SetActive(false);

        StopTalkAnimation();
        var previousDisplayIndex = displayedOutfitIndex;
        yield return PlayOutfitChangeFrame(previousDisplayIndex, 0, Color.white);
        yield return AnimateCurtains(true);
        yield return PlayOutfitChangeFrames(selectedIndex, 1, 5, new Color(1f, 1f, 1f, 0.42f));
        ApplyBonusToStatus();
        yield return new WaitForSecondsRealtime(0.25f);
        yield return AnimateCurtains(false);
        displayedOutfitIndex = selectedIndex;
        yield return PlayOutfitChangeFrames(selectedIndex, 5, 8, Color.white);

        if (commentText)
            PlayComment(outfits[selectedIndex].displayName + "に着替えたよ。ステータスも反映済み。", false);
    }

    private void ApplyBonusToStatus()
    {
        if (outfits == null || outfits.Length == 0)
            return;

        var next = ToArray(outfits[selectedIndex]);
        if (StatusManager.Instance != null)
        {
            var current = GetBaseValuesWithoutAppliedBonus();
            StatusManager.Instance.SetGuts(current[0] + next[0]);
            StatusManager.Instance.SetIntelligence(current[1] + next[1]);
            StatusManager.Instance.SetAttention(current[2] + next[2]);
            StatusManager.Instance.SetTechnique(current[3] + next[3]);
            StatusManager.Instance.SetStrength(current[4] + next[4]);
        }

        Array.Copy(next, appliedBonus, appliedBonus.Length);
        appliedIndex = selectedIndex;
        RefreshBaseRadar();
        RefreshOutfitRadar();
    }

    private void CancelSelection()
    {
        if (appliedIndex >= 0)
            SelectOutfit(appliedIndex);
        else
            RefreshOutfitPreview();
    }

    private IEnumerator AnimateCurtains(bool close)
    {
        if (!curtainLeft || !curtainRight || !curtainLeftImage || !curtainRightImage)
            yield break;

        curtainLeft.gameObject.SetActive(true);
        curtainRight.gameObject.SetActive(true);
        curtainLeftImage.color = Color.white;
        curtainRightImage.color = Color.white;
        curtainLeft.SetAsLastSibling();
        curtainRight.SetAsLastSibling();

        var leftOpen = new Vector2(-116f, 58f);
        var rightOpen = new Vector2(116f, 58f);
        var leftClosed = new Vector2(-42f, 58f);
        var rightClosed = new Vector2(42f, 58f);
        var count = Mathf.Max(curtainLeftFrames != null ? curtainLeftFrames.Length : 0, curtainRightFrames != null ? curtainRightFrames.Length : 0);
        count = Mathf.Max(count, 2);

        for (int i = 0; i < count; i++)
        {
            var index = close ? i : count - 1 - i;
            var t = count <= 1 ? 1f : index / (float)(count - 1);
            curtainLeft.anchoredPosition = Vector2.Lerp(leftOpen, leftClosed, t);
            curtainRight.anchoredPosition = Vector2.Lerp(rightOpen, rightClosed, t);
            SetSpriteFromFrames(curtainLeftImage, curtainLeftFrames, index);
            SetSpriteFromFrames(curtainRightImage, curtainRightFrames, index);
            yield return new WaitForSecondsRealtime(curtainFrameInterval);
        }

        if (!close)
        {
            curtainLeft.gameObject.SetActive(false);
            curtainRight.gameObject.SetActive(false);
        }
    }

    private IEnumerator PlayOutfitChangeFrames(int startInclusive, int endExclusive)
    {
        yield return PlayOutfitChangeFrames(selectedIndex, startInclusive, endExclusive, Color.white);
    }

    private IEnumerator PlayOutfitChangeFrames(int outfitIndex, int startInclusive, int endExclusive, Color color)
    {
        var frames = GetChangeFrames(outfitIndex);
        if (!fittingSpriteTint || frames.Length == 0)
            yield break;

        fittingSpriteTint.color = color;
        var start = Mathf.Clamp(startInclusive, 0, frames.Length);
        var end = Mathf.Clamp(endExclusive, start, frames.Length);
        for (int i = start; i < end; i++)
        {
            fittingSpriteTint.sprite = frames[i];
            yield return new WaitForSecondsRealtime(outfitFrameInterval);
        }
    }

    private IEnumerator PlayOutfitChangeFrame(int outfitIndex, int frameIndex, Color color)
    {
        var frames = GetChangeFrames(outfitIndex);
        if (!fittingSpriteTint || frames.Length == 0)
            yield break;

        fittingSpriteTint.sprite = frames[Mathf.Clamp(frameIndex, 0, frames.Length - 1)];
        fittingSpriteTint.color = color;
        yield return null;
    }

    private Sprite[] GetCurrentChangeFrames()
    {
        return GetChangeFrames(selectedIndex);
    }

    private Sprite[] GetChangeFrames(int outfitIndex)
    {
        if (outfits != null && outfitIndex >= 0 && outfitIndex < outfits.Length && outfitChangeSets != null)
        {
            var id = outfits[outfitIndex].id;
            for (int i = 0; i < outfitChangeSets.Length; i++)
            {
                if (outfitChangeSets[i].outfitId == id && outfitChangeSets[i].frames != null && outfitChangeSets[i].frames.Length > 0)
                    return outfitChangeSets[i].frames;
            }
        }

        return Array.Empty<Sprite>();
    }

    private static void SetSpriteFromFrames(Image image, Sprite[] frames, int index)
    {
        if (!image || frames == null || frames.Length == 0)
            return;

        image.sprite = frames[Mathf.Clamp(index, 0, frames.Length - 1)];
    }

    private void TintFittingSprite(int index)
    {
        if (!fittingSpriteTint)
            return;

        var palette = new[]
        {
            new Color(1f, 0.76f, 0.86f, 0.72f),
            new Color(0.95f, 0.48f, 0.54f, 0.72f),
            new Color(0.48f, 0.86f, 0.74f, 0.72f),
            new Color(0.42f, 0.78f, 0.95f, 0.72f),
            new Color(0.26f, 0.24f, 0.32f, 0.72f),
            new Color(0.92f, 0.70f, 0.50f, 0.72f)
        };

        fittingSpriteTint.color = palette[Mathf.Abs(index) % palette.Length];
    }

    private static int[] ToArray(OutfitData outfit)
    {
        return new[] { outfit.guts, outfit.intelligence, outfit.attention, outfit.attack, outfit.defense };
    }

    private int[] GetBaseValuesWithoutAppliedBonus()
    {
        if (StatusManager.Instance == null)
            return new[] { 3, 3, 3, 3, 3 };

        return new[]
        {
            StatusManager.Instance.Guts - appliedBonus[0],
            StatusManager.Instance.Intelligence - appliedBonus[1],
            StatusManager.Instance.Attention - appliedBonus[2],
            StatusManager.Instance.Technique - appliedBonus[3],
            StatusManager.Instance.Strength - appliedBonus[4]
        };
    }
}
