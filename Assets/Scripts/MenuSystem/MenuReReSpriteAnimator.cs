using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class MenuReReSpriteAnimator : MonoBehaviour
{
    [System.Serializable]
    private class AnimationSet
    {
        public string resourceFolder;
        public float framesPerSecond = 10f;
        public float weight = 1f;
        [HideInInspector] public Sprite[] frames;
    }

    [Header("Target")]
    [SerializeField] private Image targetImage;
    [SerializeField] private Vector2 displaySize = new Vector2(150f, 430f);

    [Header("Animation Sets")]
    [SerializeField] private AnimationSet[] animationSets =
    {
        new AnimationSet { resourceFolder = "MenuReReAnimations/leg_kick", framesPerSecond = 10f, weight = 2f },
        new AnimationSet { resourceFolder = "MenuReReAnimations/smile", framesPerSecond = 8f, weight = 1f },
        new AnimationSet { resourceFolder = "MenuReReAnimations/hair_accessory", framesPerSecond = 8f, weight = 1f },
        new AnimationSet { resourceFolder = "MenuReReAnimations/yawn", framesPerSecond = 7f, weight = 0.7f },
        new AnimationSet { resourceFolder = "MenuReReAnimations/talk", framesPerSecond = 10f, weight = 1.2f }
    };

    [Header("Placement")]
    [SerializeField] private Vector2 idleDelayRange = new Vector2(0.35f, 2f);
    [SerializeField] private Vector2Int loopsPerVisitRange = new Vector2Int(1, 3);
    [SerializeField] private Rect normalizedPlacementArea = new Rect(0.08f, 0.20f, 0.84f, 0.62f);
    [SerializeField] private float edgePadding = 18f;
    [SerializeField] private bool hideBetweenVisits = false;

    private readonly List<Sprite> runtimeSprites = new List<Sprite>();
    private RectTransform rectTransform;
    private Coroutine routine;

    private void Awake()
    {
        rectTransform = transform as RectTransform;

        if (!targetImage)
            targetImage = GetComponent<Image>();

        if (targetImage)
        {
            targetImage.raycastTarget = false;
            targetImage.preserveAspect = true;
        }

        rectTransform.sizeDelta = displaySize;
        LoadFrames();
        ShowFirstFrame();
    }

    private void OnEnable()
    {
        if (routine == null)
            routine = StartCoroutine(PlayRoutine());
    }

    private void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private void OnDestroy()
    {
        foreach (var sprite in runtimeSprites)
        {
            if (sprite)
                Destroy(sprite);
        }

        runtimeSprites.Clear();
    }

    private IEnumerator PlayRoutine()
    {
        while (isActiveAndEnabled)
        {
            if (hideBetweenVisits && targetImage)
                targetImage.enabled = false;

            yield return new WaitForSeconds(Random.Range(idleDelayRange.x, idleDelayRange.y));

            var set = PickAnimationSet();
            if (set == null || set.frames == null || set.frames.Length == 0)
                continue;

            MoveToRandomPlacement();

            if (targetImage)
                targetImage.enabled = true;

            int loops = Random.Range(loopsPerVisitRange.x, loopsPerVisitRange.y + 1);
            float interval = 1f / Mathf.Max(1f, set.framesPerSecond);

            for (int loop = 0; loop < loops; loop++)
            {
                for (int i = 0; i < set.frames.Length; i++)
                {
                    if (targetImage)
                        targetImage.sprite = set.frames[i];

                    yield return new WaitForSeconds(interval);
                }
            }
        }
    }

    private void LoadFrames()
    {
        foreach (var set in animationSets)
        {
            if (set == null || string.IsNullOrWhiteSpace(set.resourceFolder))
                continue;

            var textures = Resources.LoadAll<Texture2D>(set.resourceFolder);
            if (textures == null || textures.Length == 0)
                continue;

            System.Array.Sort(textures, (a, b) => string.CompareOrdinal(a.name, b.name));

            set.frames = new Sprite[textures.Length];
            for (int i = 0; i < textures.Length; i++)
            {
                var texture = textures[i];
                var sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0f),
                    100f);

                sprite.name = texture.name;
                set.frames[i] = sprite;
                runtimeSprites.Add(sprite);
            }
        }
    }

    private void ShowFirstFrame()
    {
        if (!targetImage)
            return;

        foreach (var set in animationSets)
        {
            if (set == null || set.frames == null || set.frames.Length == 0)
                continue;

            targetImage.sprite = set.frames[0];
            targetImage.enabled = true;
            return;
        }
    }

    private AnimationSet PickAnimationSet()
    {
        float totalWeight = 0f;
        foreach (var set in animationSets)
        {
            if (set != null && set.frames != null && set.frames.Length > 0)
                totalWeight += Mathf.Max(0f, set.weight);
        }

        if (totalWeight <= 0f) return null;

        float roll = Random.value * totalWeight;
        foreach (var set in animationSets)
        {
            if (set == null || set.frames == null || set.frames.Length == 0)
                continue;

            roll -= Mathf.Max(0f, set.weight);
            if (roll <= 0f)
                return set;
        }

        return null;
    }

    private void MoveToRandomPlacement()
    {
        var parent = rectTransform.parent as RectTransform;
        if (!parent) return;

        var parentRect = parent.rect;
        float minX = Mathf.Lerp(parentRect.xMin, parentRect.xMax, normalizedPlacementArea.xMin) + edgePadding;
        float maxX = Mathf.Lerp(parentRect.xMin, parentRect.xMax, normalizedPlacementArea.xMax) - edgePadding;
        float minY = Mathf.Lerp(parentRect.yMin, parentRect.yMax, normalizedPlacementArea.yMin) + edgePadding;
        float maxY = Mathf.Lerp(parentRect.yMin, parentRect.yMax, normalizedPlacementArea.yMax) - edgePadding;

        float halfWidth = displaySize.x * 0.5f;
        float halfHeight = displaySize.y * 0.5f;
        minX += halfWidth;
        maxX -= halfWidth;
        minY += halfHeight;
        maxY -= halfHeight;

        if (minX > maxX)
        {
            minX = parentRect.xMin + halfWidth;
            maxX = parentRect.xMax - halfWidth;
        }

        if (minY > maxY)
        {
            minY = parentRect.yMin + halfHeight;
            maxY = parentRect.yMax - halfHeight;
        }

        rectTransform.anchoredPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
    }
}
