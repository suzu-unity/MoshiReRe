using System.Globalization;
using System.Security;
using Naninovel.UI;
using TMPro;
using UnityEngine;

namespace MoshiReRe.DialoguePresentation.CharacterStamp
{
    /// <summary>
    /// Renders the first text element of a speaker name in the stamp accent color.
    /// </summary>
    public sealed class StampAuthorNamePanel : AuthorNamePanel
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Color accentColor = new Color(0.40f, 0.86f, 1f, 1f);

        private string value;
        private Color bodyColor = Color.white;

        public override string Text
        {
            get => value;
            set
            {
                this.value = value ?? string.Empty;
                Refresh();
            }
        }

        public override Color TextColor
        {
            get => bodyColor;
            set
            {
                bodyColor = value;
                Refresh();
            }
        }

        private void Refresh ()
        {
            if (!text) return;

            text.color = bodyColor;
            if (string.IsNullOrEmpty(value))
            {
                text.text = string.Empty;
                return;
            }

            var enumerator = StringInfo.GetTextElementEnumerator(value);
            enumerator.MoveNext();
            var first = (string)enumerator.Current;
            var restIndex = enumerator.ElementIndex + first.Length;
            var rest = value.Substring(restIndex);
            var accent = ColorUtility.ToHtmlStringRGBA(accentColor);
            text.text = $"<color=#{accent}>{SecurityElement.Escape(first)}</color>{SecurityElement.Escape(rest)}";
        }
    }
}
