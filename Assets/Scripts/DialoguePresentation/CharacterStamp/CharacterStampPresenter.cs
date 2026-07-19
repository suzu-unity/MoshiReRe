using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoshiReRe.DialoguePresentation.CharacterStamp
{
    /// <summary>
    /// Resolves a dialogue author to the lightweight project character database and updates the stamp.
    /// </summary>
    public sealed class CharacterStampPresenter : MonoBehaviour
    {
        [SerializeField] private CharacterDatabase characterDatabase;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI numberLabel;
        [SerializeField] private string unknownNumber = "--";

        public void SetAuthor (string authorId, string authorName)
        {
            var hasAuthor = !string.IsNullOrWhiteSpace(authorId) || !string.IsNullOrWhiteSpace(authorName);
            gameObject.SetActive(hasAuthor);
            if (!hasAuthor) return;

            var character = FindCharacter(authorId, authorName, out var index);
            if (iconImage)
            {
                iconImage.sprite = character ? character.icon : null;
                iconImage.enabled = iconImage.sprite;
            }

            if (numberLabel)
                numberLabel.text = character ? (index + 1).ToString("00") : unknownNumber;
        }

        private CharacterInfo FindCharacter (string authorId, string authorName, out int index)
        {
            index = -1;
            if (!characterDatabase) return null;

            var characters = characterDatabase.GetAll();
            for (var i = 0; i < characters.Count; i++)
            {
                var character = characters[i];
                if (!character) continue;

                if (Matches(character.id, authorId) ||
                    Matches(character.displayName, authorName) ||
                    Matches(character.name, authorId))
                {
                    index = i;
                    return character;
                }
            }

            return null;
        }

        private static bool Matches (string left, string right)
        {
            return !string.IsNullOrWhiteSpace(left) &&
                   !string.IsNullOrWhiteSpace(right) &&
                   string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }
    }
}
