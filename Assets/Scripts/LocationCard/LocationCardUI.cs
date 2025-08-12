using UnityEngine;
using TMPro;
using System.Collections;

public class LocationCardUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TextMeshProUGUI titleText;  // 白文字にしておく
    [SerializeField] private CanvasGroup canvasGroup;    // 透明度管理

    [Header("Timings")]
    [SerializeField, Min(0f)] private float fadeDuration = 0.25f;
    [SerializeField, Min(0f)] private float charInterval = 0.03f; // 1文字ごとの待ち時間（Inspectorで上書き可）
    [SerializeField, Min(0f)] private float holdSeconds  = 1.2f;  // 全表示後に止める秒数

    [Header("Type SFX (optional)")]
    [SerializeField] private AudioSource typeAudioSource; // 同じオブジェクトに付けるのが楽
    [SerializeField] private AudioClip typeSound;
    [SerializeField, Range(0f, 1f)] private float typeVolume = 0.5f;
    [SerializeField] private Vector2 typePitchRange = new Vector2(0.95f, 1.05f); // 文字ごとに少しだけピッチを揺らす

    public float FadeDuration => fadeDuration;
    public float CharInterval => charInterval;
    public float HoldSeconds  => holdSeconds;

    Coroutine playingCo;

    void Awake ()
    {
        // 推奨の初期設定（警告対応）
        if (titleText)
        {
            titleText.text = "";
            titleText.maxVisibleCharacters = 0;
            // これに差し替え
            titleText.textWrappingMode = TextWrappingModes.NoWrap;
            titleText.overflowMode     = TextOverflowModes.Overflow;     // 隠れないように
        }
        if (canvasGroup) canvasGroup.alpha = 0f;
    }

    /// <summary>外部から呼ぶ用。例: ui.Play("新宿・南口", 1.5f);</summary>
    public Coroutine Play (string text, float? holdOverride = null)
    {
        if (playingCo != null) StopCoroutine(playingCo);
        playingCo = StartCoroutine(PlayRoutine(text, holdOverride ?? holdSeconds));
        return playingCo;
    }

    public IEnumerator PlayRoutine (string text, float hold)
    {
        // 初期化
        canvasGroup.alpha = 0f;
        titleText.text = text ?? "";
        titleText.ForceMeshUpdate();
        titleText.maxVisibleCharacters = 0;

        // フェードイン
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // タイプライター（1文字ずつ表示）
        int total = titleText.textInfo.characterCount;
        for (int i = 0; i < total; i++)
        {
            titleText.maxVisibleCharacters = i + 1;

            // 文字ごとSE（空白は鳴らさない）
            if (typeSound && typeAudioSource)
            {
                char c = SafeCharAt(titleText.text, i);
                if (!char.IsWhiteSpace(c))
                {
                    float pitch = Random.Range(typePitchRange.x, typePitchRange.y);
                    typeAudioSource.pitch = pitch;
                    typeAudioSource.PlayOneShot(typeSound, typeVolume);
                }
            }

            yield return new WaitForSeconds(charInterval);
        }

        // ホールド
        yield return new WaitForSeconds(hold);

        // フェードアウト
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        // 片付け
        titleText.text = "";
        titleText.maxVisibleCharacters = 0;
        playingCo = null;
    }

    private char SafeCharAt (string s, int index)
    {
        if (string.IsNullOrEmpty(s) || index < 0 || index >= s.Length) return ' ';
        return s[index];
    }
}