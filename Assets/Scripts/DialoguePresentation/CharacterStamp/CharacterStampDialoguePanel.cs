using Naninovel;
using Naninovel.UI;
using UnityEngine;

namespace MoshiReRe.DialoguePresentation.CharacterStamp
{
    /// <summary>
    /// Keeps Naninovel's standard reveal flow while forwarding the active author to the stamp view.
    /// </summary>
    public sealed class CharacterStampDialoguePanel : RevealableTextPrinterPanel
    {
        [SerializeField] private CharacterStampPresenter characterStamp;

        protected override void SetMessageAuthor (MessageAuthor author)
        {
            base.SetMessageAuthor(author);

            if (!characterStamp) return;

            string label;
            if (author.Label.IsEmpty)
                label = CharacterManager.GetAuthorName(author.Id);
            else
                label = author.Label;
            characterStamp.SetAuthor(author.Id, label);
        }
    }
}
