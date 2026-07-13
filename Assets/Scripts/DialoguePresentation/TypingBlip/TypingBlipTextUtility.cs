using System;
using System.Text;

namespace MoshiReRe.DialoguePresentation.TypingBlip
{
    public static class TypingBlipTextUtility
    {
        public static string StripRichTextTags(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            var result = new StringBuilder(text.Length);
            for (var i = 0; i < text.Length; i++)
            {
                var character = text[i];
                if (character == '<')
                {
                    var tagEnd = text.IndexOf('>', i + 1);
                    if (tagEnd >= 0)
                    {
                        i = tagEnd;
                        continue;
                    }
                }

                result.Append(character);
            }

            return result.ToString();
        }

        public static bool IsSpeakableCharacter(char character)
        {
            return char.IsLetterOrDigit(character);
        }

        public static int CountSpeakableCharacters(string text)
        {
            var normalized = StripRichTextTags(text);
            var count = 0;
            for (var i = 0; i < normalized.Length; i++)
                if (IsSpeakableCharacter(normalized[i])) count++;
            return count;
        }
    }

    public static class TypingBlipRevealMath
    {
        public static int GetRevealedCharacterCount(float revealProgress, int visibleCharacterCount)
        {
            if (visibleCharacterCount <= 0) return 0;
            var clampedProgress = Math.Max(0f, Math.Min(1f, revealProgress));
            return Math.Max(0, Math.Min(visibleCharacterCount,
                (int)Math.Ceiling(clampedProgress * visibleCharacterCount)));
        }
    }

    public sealed class TypingBlipRevealState
    {
        private int sampledVisibleCharacterCount;
        private int speakableCharactersSinceBlip;

        public int Consume(string normalizedText, int revealedVisibleCharacterCount, int charactersPerBlip)
        {
            if (string.IsNullOrEmpty(normalizedText)) return 0;

            var clampedCount = Math.Max(0, Math.Min(revealedVisibleCharacterCount, normalizedText.Length));
            if (clampedCount < sampledVisibleCharacterCount)
            {
                sampledVisibleCharacterCount = 0;
                speakableCharactersSinceBlip = 0;
            }

            var threshold = Math.Max(1, charactersPerBlip);
            var blips = 0;
            for (var i = sampledVisibleCharacterCount; i < clampedCount; i++)
            {
                if (!TypingBlipTextUtility.IsSpeakableCharacter(normalizedText[i])) continue;
                speakableCharactersSinceBlip++;
                if (speakableCharactersSinceBlip < threshold) continue;

                speakableCharactersSinceBlip -= threshold;
                blips++;
            }

            sampledVisibleCharacterCount = clampedCount;
            return blips;
        }

        public void Reset()
        {
            sampledVisibleCharacterCount = 0;
            speakableCharactersSinceBlip = 0;
        }
    }

    public sealed class TypingBlipRateLimiter
    {
        private float lastPlayTime = float.NegativeInfinity;

        public bool TryAcquire(float now, float minimumInterval)
        {
            var interval = Math.Max(0f, minimumInterval);
            if (now - lastPlayTime < interval) return false;
            lastPlayTime = now;
            return true;
        }

        public void Reset()
        {
            lastPlayTime = float.NegativeInfinity;
        }
    }
}
