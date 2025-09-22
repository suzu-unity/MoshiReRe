using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Databases/Character Database")]
public class CharacterDatabase : ScriptableObject
{
    public List<CharacterInfo> characters = new List<CharacterInfo>();

    public IReadOnlyList<CharacterInfo> GetAll() => characters;
}
