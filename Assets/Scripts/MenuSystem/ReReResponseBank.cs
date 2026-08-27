using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optional authoring asset for a conversation response bank.  Leaving this
/// field unassigned on <see cref="ReReConversationUI"/> uses the offline bank.
/// </summary>
[CreateAssetMenu(menuName = "Game/ReRe/Conversation Response Bank")]
public sealed class ReReResponseBank : ScriptableObject
{
    [SerializeField] private List<ReReResponseEntry> entries = new List<ReReResponseEntry>();

    public IReadOnlyList<ReReResponseEntry> Entries => entries ?? (IReadOnlyList<ReReResponseEntry>)new List<ReReResponseEntry>();
}
