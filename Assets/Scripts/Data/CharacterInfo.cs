using System;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterCategory { Oj, Itadaki, Other }

public enum CharacterInformationNodeCategory
{
    BasicInformation,
    SelfImage,
    Desire,
    Fear,
    Resources,
    Risk,
    CompanyConnection
}

public enum CharacterInformationConfidence
{
    Unknown,
    Speculation,
    Confirmed,
    Misinformation
}

[Serializable]
public class CharacterInformationNodeDefinition
{
    [Tooltip("Stable per-character identifier used by scenario and gameplay code.")]
    public string id;
    public string title;
    public CharacterInformationNodeCategory category;
    [TextArea] public string content;
    public CharacterInformationConfidence initialConfidence = CharacterInformationConfidence.Unknown;
}

[CreateAssetMenu(menuName = "Game/Character Info")]
public class CharacterInfo : ScriptableObject
{
    public string id;
    public string displayName;
    public CharacterCategory category = CharacterCategory.Other;

    public Sprite icon;
    public Sprite portrait;

    [TextArea] public string summary;
    [TextArea] public string description;

    [Header("ReRe Information Nodes")]
    [Tooltip("Authoring definitions. Runtime confidence is stored separately by CharacterInformationNodeState.")]
    public List<CharacterInformationNodeDefinition> nodes = new List<CharacterInformationNodeDefinition>();
}
