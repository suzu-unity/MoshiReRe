using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuTopReReMascot : MonoBehaviour
{
    private const string DefaultHint = "今夜の準備をしよう。迷ったら、期限と情報ノードを確認してね。";

    [Serializable]
    private class MotionSet
    {
        public string id;
        public Sprite[] frames;
        public float weight = 1f;
        public float frameRate = 7f;
        public bool walkMotion;
        public bool showBubble;
        public string nextId;
        public int loopsBeforeNext = 1;
        public float holdSeconds = 1f;
    }

    [SerializeField] private Image mascotImage;
    [SerializeField] private Button mascotButton;
    [SerializeField] private RectTransform mascot;
    [SerializeField] private RectTransform bubble;
    [SerializeField] private TextMeshProUGUI bubbleText;
    [SerializeField] private MotionSet[] motionSets;
    [SerializeField] private string[] clickMotionIds;
    [SerializeField] private Vector2 fixedBottomRightPosition = new Vector2(628f, -338f);
    [SerializeField] private Vector2 bubbleOffset = new Vector2(-382f, 250f);
    [SerializeField] private float walkDistance = 118f;
    [SerializeField] private float walkSpeed = 34f;

    private MotionSet currentMotion;
    private Vector2 basePosition;
    private Vector2 walkLeftPosition;
    private Vector2 walkRightPosition;
    private Vector2 walkTarget;
    private float frameTimer;
    private int frameIndex;
    private float clickBubbleTimer;
    private int completedLoops;
    private float holdTimer;
    private bool holdingLastFrame;

    private void Awake()
    {
        if (bubbleText)
            bubbleText.text = DefaultHint;

        if (bubble)
            bubble.gameObject.SetActive(false);

        if (mascotButton)
            mascotButton.onClick.AddListener(ShowClickedBubble);
    }

    private void OnDestroy()
    {
        if (mascotButton)
            mascotButton.onClick.RemoveListener(ShowClickedBubble);
    }

    private void OnEnable()
    {
        if (currentMotion == null)
            PlaceForMenuOpen();
    }

    private void Update()
    {
        if (!mascot)
            return;

        if (currentMotion == null)
            PlaceForMenuOpen();

        if (currentMotion.walkMotion)
            UpdateWalkPosition();

        UpdateFrames();
        UpdateClickedBubble();
        UpdateBubblePosition();
    }

    public void PlaceForMenuOpen()
    {
        basePosition = fixedBottomRightPosition;
        StartMotion(PickMotionSet(), true);
    }

    private void StartMotion(MotionSet motion, bool resetPosition)
    {
        currentMotion = motion;
        frameIndex = 0;
        frameTimer = 0f;
        completedLoops = 0;
        holdTimer = 0f;
        holdingLastFrame = false;

        if (!mascot)
            return;

        if (resetPosition || currentMotion == null || !currentMotion.walkMotion)
            mascot.anchoredPosition = basePosition;

        mascot.localScale = Vector3.one;

        if (currentMotion != null && currentMotion.walkMotion)
        {
            walkRightPosition = fixedBottomRightPosition;
            walkLeftPosition = fixedBottomRightPosition + Vector2.left * walkDistance;
            walkTarget = walkLeftPosition;
            mascot.localScale = new Vector3(-1f, 1f, 1f);
        }

        if (bubble)
            bubble.gameObject.SetActive(clickBubbleTimer > 0f || currentMotion != null && currentMotion.showBubble);

        ApplyFrame(0);
    }

    private MotionSet PickMotionSet()
    {
        if (motionSets == null || motionSets.Length == 0)
            return null;

        var total = 0f;
        foreach (var set in motionSets)
        {
            if (set != null && set.frames != null && set.frames.Length > 0)
                total += Mathf.Max(0f, set.weight);
        }

        if (total <= 0f)
            return motionSets[0];

        var roll = UnityEngine.Random.value * total;
        foreach (var set in motionSets)
        {
            if (set == null || set.frames == null || set.frames.Length == 0)
                continue;

            roll -= Mathf.Max(0f, set.weight);
            if (roll <= 0f)
                return set;
        }

        return motionSets[0];
    }

    private void UpdateWalkPosition()
    {
        mascot.anchoredPosition = Vector2.MoveTowards(mascot.anchoredPosition, walkTarget, walkSpeed * Time.unscaledDeltaTime);

        if ((walkTarget - mascot.anchoredPosition).sqrMagnitude > 4f)
            return;

        var reachedLeft = walkTarget == walkLeftPosition;
        walkTarget = reachedLeft ? walkRightPosition : walkLeftPosition;
        mascot.localScale = new Vector3(reachedLeft ? 1f : -1f, 1f, 1f);
    }

    private void UpdateFrames()
    {
        if (currentMotion == null)
            return;

        var frames = currentMotion.frames;
        if (frames == null || frames.Length == 0)
            return;

        if (holdingLastFrame)
        {
            holdTimer -= Time.unscaledDeltaTime;
            if (holdTimer <= 0f)
                AdvanceMotion();
            return;
        }

        frameTimer += Time.unscaledDeltaTime;
        var rate = Mathf.Max(1f, currentMotion.frameRate);
        if (frameTimer < 1f / rate)
            return;

        frameTimer = 0f;
        frameIndex++;
        if (frameIndex >= frames.Length)
        {
            completedLoops++;
            if (completedLoops >= Mathf.Max(1, currentMotion.loopsBeforeNext))
            {
                frameIndex = frames.Length - 1;
                holdingLastFrame = true;
                holdTimer = Mathf.Max(0.15f, currentMotion.holdSeconds);
                ApplyFrame(frameIndex);
                return;
            }

            frameIndex = 0;
        }

        ApplyFrame(frameIndex);
    }

    private void AdvanceMotion()
    {
        var next = FindMotion(currentMotion != null ? currentMotion.nextId : null);
        StartMotion(next ?? PickMotionSet(), false);
    }

    private MotionSet FindMotion(string id)
    {
        if (string.IsNullOrEmpty(id) || motionSets == null)
            return null;

        foreach (var set in motionSets)
        {
            if (set != null && set.id == id && set.frames != null && set.frames.Length > 0)
                return set;
        }

        return null;
    }

    private void ApplyFrame(int index)
    {
        if (!mascotImage || currentMotion == null || currentMotion.frames == null || currentMotion.frames.Length == 0)
            return;

        mascotImage.sprite = currentMotion.frames[Mathf.Abs(index) % currentMotion.frames.Length];
    }

    private void ShowClickedBubble()
    {
        if (bubbleText)
            bubbleText.text = DefaultHint;

        var clickMotion = PickClickMotion();
        if (clickMotion != null)
            StartMotion(clickMotion, false);

        if (bubble)
            bubble.gameObject.SetActive(true);

        clickBubbleTimer = 4.2f;
    }

    private MotionSet PickClickMotion()
    {
        if (clickMotionIds == null || clickMotionIds.Length == 0)
            return FindMotion("click_talk");

        var start = UnityEngine.Random.Range(0, clickMotionIds.Length);
        for (int i = 0; i < clickMotionIds.Length; i++)
        {
            var motion = FindMotion(clickMotionIds[(start + i) % clickMotionIds.Length]);
            if (motion != null)
                return motion;
        }

        return null;
    }

    private void UpdateClickedBubble()
    {
        if (clickBubbleTimer <= 0f)
            return;

        clickBubbleTimer -= Time.unscaledDeltaTime;
        if (clickBubbleTimer <= 0f && bubble)
            bubble.gameObject.SetActive(currentMotion != null && currentMotion.showBubble);
    }

    private void UpdateBubblePosition()
    {
        if (!mascot || !bubble || !bubble.gameObject.activeSelf)
            return;

        bubble.anchoredPosition = mascot.anchoredPosition + bubbleOffset;
    }
}
