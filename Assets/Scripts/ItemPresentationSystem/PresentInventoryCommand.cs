using MoshiReRe.Exploration;
using Naninovel;
using UnityEngine;

namespace MoshiReRe.ItemPresentation
{
    /// <summary>
    /// Opens the acquired-item modal and writes a compact result code to a Naninovel variable.
    /// The optional required variable prevents an old item in a legacy save from bypassing
    /// the authored exploration condition.
    /// </summary>
    [Command.CommandAlias("presentInventory")]
    public sealed class PresentInventoryCommand : Command
    {
        [Command.ParameterAlias("result")]
        public StringParameter ResultVariable;

        [Command.ParameterAlias("success")]
        public StringParameter SuccessItemId;

        [Command.ParameterAlias("requiredVariable")]
        public StringParameter RequiredVariable;

        [Command.ParameterAlias("requiredValue")]
        public StringParameter RequiredValue;

        public override async UniTask Execute(AsyncToken asyncToken = default)
        {
            if (!Engine.Initialized)
                return;

            var database = ItemPresentationUI.ResolveLoadedDatabase();
            var outcome = await ItemPresentationUI.PresentAsync(database, asyncToken);
            var successId = Assigned(SuccessItemId)
                ? ItemPresentationFlow.NormalizeId(SuccessItemId.Value)
                : string.Empty;
            var requiredVariableName = Assigned(RequiredVariable)
                ? RequiredVariable.Value
                : string.Empty;
            var requiredValue = Assigned(RequiredValue)
                ? RequiredValue.Value
                : "true";
            var requiredConditionMet = IsRequiredConditionMet(requiredVariableName, requiredValue);
            var resultCode = ItemPresentationFlow.ResolveResultCode(outcome, successId, requiredConditionMet);

            var resultVariable = Assigned(ResultVariable)
                ? ResultVariable.Value
                : "itemPresentationResult";
            if (string.IsNullOrWhiteSpace(resultVariable))
                return;

            if (Engine.TryGetService<ICustomVariableManager>(out var variables) && variables != null)
                variables.SetVariableValue(resultVariable, new CustomVariableValue(resultCode));
            else
                Debug.LogWarning("[presentInventory] Custom variable manager is unavailable.");
        }

        private static bool IsRequiredConditionMet(string variableName, string expectedValue)
        {
            if (string.IsNullOrWhiteSpace(variableName))
                return true;
            if (!Engine.Initialized || !Engine.TryGetService<ICustomVariableManager>(out var variables) || variables == null)
                return false;
            if (!variables.VariableExists(variableName))
                return false;

            var currentValue = variables.GetVariableValue(variableName).ToString();
            return NaninovelDialogueInteractable.DoCustomVariableValuesMatch(currentValue, expectedValue);
        }
    }
}
