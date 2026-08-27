using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Reusable free-text ReRe conversation view.  References are optional: the
/// component can be placed on an existing menu root and will discover common
/// TMP input, send-button, and speech-bubble names at runtime.
/// </summary>
[DisallowMultipleComponent]
public sealed class ReReConversationUI : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button sendButton;

    [Header("Speech bubble")]
    [SerializeField] private GameObject speechBubbleRoot;
    [SerializeField] private TMP_Text speechText;
    [SerializeField] private CanvasGroup speechCanvasGroup;
    [SerializeField] private AdviceBubble adviceBubble;
    [SerializeField] private bool hideSpeechBubbleOnAwake = true;
    [SerializeField, Min(0f)] private float popDuration = 0.12f;

    [Header("Conversation data")]
    [SerializeField] private ReReResponseBank responseBank;
    [Tooltip("Optional component implementing IReReConversationContextProvider.")]
    [SerializeField] private MonoBehaviour contextProviderBehaviour;
    [Tooltip("Optional component implementing IReReSemanticRetrievalProvider. It may use an external embedding service, but it must return a local response id.")]
    [SerializeField] private MonoBehaviour semanticProviderBehaviour;

    [Header("Unity hooks")]
    [SerializeField] private ReReExpressionEvent expressionChanged = new ReReExpressionEvent();
    [SerializeField] private ReReConversationStateEvent stateChanged = new ReReConversationStateEvent();
    [SerializeField] private UnityEvent<string> responseReceived = new UnityEvent<string>();

    private ReReConversationEngine engine;
    private IReReConversationContextProvider contextProvider;
    private int turnIndex;
    private int lastSubmitFrame = int.MinValue;
    private Coroutine popRoutine;
    private ReReExpression currentExpression;
    private ReReConversationState currentState;

    public event Action<ReReResponseResult> ResponseReceived;
    public event Action<ReReExpression> ExpressionChanged;
    public event Action<ReReConversationState> StateChanged;

    public ReReConversationEngine Engine => engine;
    public ReReExpression CurrentExpression => currentExpression;
    public ReReConversationState CurrentState => currentState;
    public int TurnIndex => turnIndex;
    public string LastResponseText { get; private set; } = string.Empty;
    public ReReConversationContext CurrentContext { get; private set; } = ReReConversationContext.Empty;

    private void Awake()
    {
        AutoWireReferences();
        RebuildEngine();
        BindInput();
        SetExpression(ReReExpression.Neutral);
        SetState(ReReConversationState.Idle);

        if (hideSpeechBubbleOnAwake)
            HideSpeechBubble();
    }

    private void OnEnable()
    {
        if (engine == null)
        {
            AutoWireReferences();
            RebuildEngine();
        }
    }

    private void Update()
    {
        // TMP's submit event handles the normal Enter path.  This fallback
        // covers fields configured for multi-line input and keypad Enter.
        if (inputField == null || !inputField.isFocused)
            return;

        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && lastSubmitFrame != Time.frameCount)
            SubmitInput();
    }

    private void OnDestroy()
    {
        UnbindInput();
        if (popRoutine != null)
            StopCoroutine(popRoutine);
    }

    public void SubmitInput()
    {
        SubmitInput(inputField != null ? inputField.text : string.Empty);
    }

    public void SubmitInput(string input)
    {
        if (engine == null)
        {
            AutoWireReferences();
            RebuildEngine();
        }

        if (string.IsNullOrWhiteSpace(input))
            return;

        if (lastSubmitFrame == Time.frameCount)
            return;

        lastSubmitFrame = Time.frameCount;

        SetState(ReReConversationState.Listening);
        CurrentContext = ReadContext();
        var result = engine.Respond(input, CurrentContext, turnIndex);
        turnIndex++;

        if (!result.HasResponse)
        {
            SetState(ReReConversationState.Error);
            return;
        }

        SetState(ReReConversationState.Responding);
        LastResponseText = result.Text;
        SetExpression(result.Expression);
        ShowSpeechBubble(result.Text);
        ResponseReceived?.Invoke(result);
        responseReceived?.Invoke(result.Text);
        if (inputField)
            inputField.SetTextWithoutNotify(string.Empty);
        SetState(ReReConversationState.Idle);
    }

    public void SetContextProvider(MonoBehaviour provider)
    {
        contextProviderBehaviour = provider;
        contextProvider = provider as IReReConversationContextProvider;
        CurrentContext = ReadContext();
    }

    public void SetSemanticRetrievalProvider(MonoBehaviour provider)
    {
        semanticProviderBehaviour = provider;
        RebuildEngine();
    }

    public void SetResponseBank(ReReResponseBank bank)
    {
        responseBank = bank;
        RebuildEngine();
    }

    public void ResetConversation()
    {
        turnIndex = 0;
        LastResponseText = string.Empty;
        SetExpression(ReReExpression.Neutral);
        SetState(ReReConversationState.Idle);
        HideSpeechBubble();
    }

    public void SetExpression(ReReExpression expression)
    {
        currentExpression = expression;
        ExpressionChanged?.Invoke(expression);
        expressionChanged?.Invoke(expression);
    }

    public void SetState(ReReConversationState state)
    {
        currentState = state;
        StateChanged?.Invoke(state);
        stateChanged?.Invoke(state);
    }

    public void ShowSpeechBubble(string message)
    {
        if (adviceBubble)
        {
            adviceBubble.Show(message, false);
            return;
        }

        if (speechText)
            speechText.text = message ?? string.Empty;
        if (speechBubbleRoot)
            speechBubbleRoot.SetActive(true);
        if (speechCanvasGroup)
            speechCanvasGroup.alpha = 1f;

        if (speechBubbleRoot && popDuration > 0f)
        {
            if (popRoutine != null)
                StopCoroutine(popRoutine);
            popRoutine = StartCoroutine(PopSpeechBubble());
        }
    }

    public void HideSpeechBubble()
    {
        if (adviceBubble)
        {
            adviceBubble.Hide();
            return;
        }

        if (speechCanvasGroup)
            speechCanvasGroup.alpha = 0f;
        if (speechBubbleRoot)
            speechBubbleRoot.SetActive(false);
    }

    private IEnumerator PopSpeechBubble()
    {
        var rect = speechBubbleRoot.transform as RectTransform;
        if (!rect)
        {
            popRoutine = null;
            yield break;
        }

        rect.localScale = Vector3.one * 0.94f;
        var elapsed = 0f;
        while (elapsed < popDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            rect.localScale = Vector3.Lerp(Vector3.one * 0.94f, Vector3.one, elapsed / popDuration);
            yield return null;
        }

        rect.localScale = Vector3.one;
        popRoutine = null;
    }

    private void BindInput()
    {
        if (inputField)
        {
            inputField.onSubmit.RemoveListener(HandleInputSubmit);
            inputField.onSubmit.AddListener(HandleInputSubmit);
        }

        if (sendButton)
        {
            sendButton.onClick.RemoveListener(SubmitInput);
            sendButton.onClick.AddListener(SubmitInput);
        }
    }

    private void UnbindInput()
    {
        if (inputField)
            inputField.onSubmit.RemoveListener(HandleInputSubmit);
        if (sendButton)
            sendButton.onClick.RemoveListener(SubmitInput);
    }

    private void HandleInputSubmit(string input)
    {
        SubmitInput(input);
    }

    private void RebuildEngine()
    {
        var semanticProvider = semanticProviderBehaviour as IReReSemanticRetrievalProvider;
        engine = responseBank != null
            ? new ReReConversationEngine(responseBank.Entries, semanticProvider)
            : new ReReConversationEngine(null, semanticProvider);
    }

    private ReReConversationContext ReadContext()
    {
        if (contextProvider == null)
            contextProvider = contextProviderBehaviour as IReReConversationContextProvider;

        if (contextProvider == null)
            contextProvider = DiscoverComponent<IReReConversationContextProvider>();

        return contextProvider != null
            ? contextProvider.GetConversationContext() ?? ReReConversationContext.Empty
            : ReReConversationContext.Empty;
    }

    private void AutoWireReferences()
    {
        if (!inputField)
            inputField = FindComponent<TMP_InputField>("ReReInput", "ConversationInput", "Input", "InputField");

        if (!sendButton)
            sendButton = FindButton("ReReSend", "ConversationSend", "SendButton", "Send", "Submit");

        if (!speechBubbleRoot)
        {
            var bubble = FindTransform("ReReSpeechBubble", "SpeechBubble", "ConversationBubble", "Bubble");
            if (bubble) speechBubbleRoot = bubble.gameObject;
        }

        if (!speechText)
            speechText = FindText(speechBubbleRoot != null ? speechBubbleRoot.transform : transform,
                "ResponseText", "SpeechText", "Message", "ReReCommentText");

        if (!speechCanvasGroup && speechBubbleRoot)
            speechCanvasGroup = speechBubbleRoot.GetComponent<CanvasGroup>();
        if (!adviceBubble && speechBubbleRoot)
            adviceBubble = speechBubbleRoot.GetComponentInChildren<AdviceBubble>(true);

        if (!contextProviderBehaviour)
            contextProviderBehaviour = FindComponent<IReReConversationContextProvider>() as MonoBehaviour;
        contextProvider = contextProviderBehaviour as IReReConversationContextProvider;
        if (!semanticProviderBehaviour)
            semanticProviderBehaviour = FindComponent<IReReSemanticRetrievalProvider>() as MonoBehaviour;
    }

    private T FindComponent<T>(params string[] preferredNames) where T : class
    {
        var components = GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var preferredName in preferredNames)
        {
            foreach (var component in components)
                if (component != null && component.name == preferredName && component is T typed) return typed;
        }

        foreach (var component in components)
            if (component is T typed) return typed;

        // Providers are often hosted on a shared menu root while this view is
        // nested below it.  Include ancestors without changing the preferred
        // child/sibling lookup order above.
        var ancestors = GetComponentsInParent<MonoBehaviour>(true);
        foreach (var preferredName in preferredNames)
        {
            foreach (var component in ancestors)
                if (component != null && component.name == preferredName && component is T typed) return typed;
        }

        foreach (var component in ancestors)
            if (component is T typed) return typed;
        return null;
    }

    private T DiscoverComponent<T>() where T : class
    {
        return FindComponent<T>();
    }

    private Button FindButton(params string[] preferredNames)
    {
        var buttons = GetComponentsInChildren<Button>(true);
        foreach (var preferredName in preferredNames)
            foreach (var button in buttons)
                if (button != null && button.name.IndexOf(preferredName, StringComparison.OrdinalIgnoreCase) >= 0) return button;
        return null;
    }

    private TMP_InputField FindComponent<T>(string preferredName1, string preferredName2, string preferredName3, string preferredName4)
        where T : TMP_InputField
    {
        var fields = GetComponentsInChildren<TMP_InputField>(true);
        var names = new[] { preferredName1, preferredName2, preferredName3, preferredName4 };
        foreach (var name in names)
            foreach (var field in fields)
                if (field != null && field.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0) return field;
        return fields.Length > 0 ? fields[0] : null;
    }

    private Transform FindTransform(params string[] preferredNames)
    {
        foreach (var child in GetComponentsInChildren<Transform>(true))
            foreach (var preferredName in preferredNames)
                if (child.name.Equals(preferredName, StringComparison.OrdinalIgnoreCase)) return child;
        return null;
    }

    private static TMP_Text FindText(Transform root, params string[] preferredNames)
    {
        if (!root) return null;
        var texts = root.GetComponentsInChildren<TMP_Text>(true);
        foreach (var preferredName in preferredNames)
            foreach (var text in texts)
                if (text != null && text.name.Equals(preferredName, StringComparison.OrdinalIgnoreCase)) return text;
        return texts.Length > 0 ? texts[0] : null;
    }
}
