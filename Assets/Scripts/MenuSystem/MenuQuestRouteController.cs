using UnityEngine;
using UnityEngine.UI;
using Naninovel;

/// <summary>Presentation of quest preparation; scenario code can set the current step explicitly.</summary>
public sealed class MenuQuestRouteController : MonoBehaviour
{
    public enum Step { Character, Information, Items, Map, Conversation }
    [SerializeField] private Step currentStep;
    [SerializeField] private Image[] nodes;
    [SerializeField] private Sprite[] stepSprites;
    [SerializeField] private MenuRootV2UI menuRoot;
    [SerializeField] private MapRouteLauncher conversationRoute;
    public Step CurrentStep => currentStep;
    private void OnEnable()
    {
        if (Engine.Initialized && Engine.TryGetService<ICustomVariableManager>(out var variables))
        {
            if (variables.VariableExists("menuQuestStep") && int.TryParse(variables.GetVariableValue("menuQuestStep").ToString(), out var step)) currentStep = (Step)step;
            else if (variables.VariableExists("rereContext") && variables.GetVariableValue("rereContext").ToString().Contains("papa_cafe"))
                currentStep = variables.VariableExists("papaCafeKeyFound") && variables.GetVariableValue("papaCafeKeyFound").ToString().Equals("true", System.StringComparison.OrdinalIgnoreCase) ? Step.Conversation : Step.Map;
        }
        SetStep(currentStep);
    }
    public void SetStep(Step step)
    {
        currentStep = (Step)Mathf.Clamp((int)step, 0, 4);
        if (nodes == null) return;
        for (var i = 0; i < nodes.Length; i++)
        {
            if (!nodes[i]) continue;
            if (stepSprites != null && i < stepSprites.Length) nodes[i].sprite = stepSprites[i];
            nodes[i].color = i > (int)currentStep ? new Color(.55f, .55f, .63f) : Color.white;
            nodes[i].transform.localScale = Vector3.one * (i == (int)currentStep ? 1.12f : 1f);
        }
    }
    public void Go()
    {
        if (!menuRoot) menuRoot = GetComponentInParent<MenuRootV2UI>();
        switch (currentStep)
        {
            case Step.Character: case Step.Information: menuRoot?.ShowCharacters(); break;
            case Step.Items: menuRoot?.ShowItems(); break;
            case Step.Map: menuRoot?.ShowMap(); break;
            case Step.Conversation: conversationRoute?.HandleGoSelected(3); break;
        }
    }
}
