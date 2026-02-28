using System;
using System.Collections.Generic;
using System.Text;

public static class NameModeration
{
    public static string Sanitize(string input, int maxLength)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        string trimmed = input.Trim();
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        // Allow: letters, digits, space, apostrophe, hyphen.
        // Collapse multiple spaces.
        StringBuilder sb = new StringBuilder(trimmed.Length);
        bool lastWasSpace = false;

        for (int i = 0; i < trimmed.Length; i++)
        {
            char c = trimmed[i];

            bool isAllowedChar = char.IsLetterOrDigit(c) || c == ' ' || c == '\'' || c == '-';
            if (!isAllowedChar)
            {
                continue;
            }

            if (c == ' ')
            {
                if (lastWasSpace)
                {
                    continue;
                }
                lastWasSpace = true;
            }
            else
            {
                lastWasSpace = false;
            }

            sb.Append(c);

            if (maxLength > 0 && sb.Length >= maxLength)
            {
                break;
            }
        }

        return sb.ToString().Trim();
    }

    public static bool IsAllowed(string sanitizedName, IReadOnlyList<string> bannedWords, out string reason)
    {
        reason = null;

        if (string.IsNullOrWhiteSpace(sanitizedName))
        {
            reason = "Name is empty";
            return false;
        }

        // Basic anti-spam / unsafe input checks.
        string lower = sanitizedName.ToLowerInvariant();
        if (lower.Contains("http") || lower.Contains("www") || lower.Contains("@"))
        {
            reason = "Name contains a link or handle";
            return false;
        }

        if (bannedWords != null && bannedWords.Count > 0)
        {
            string normalized = NormalizeForMatch(sanitizedName);

            for (int i = 0; i < bannedWords.Count; i++)
            {
                string banned = bannedWords[i];
                if (string.IsNullOrWhiteSpace(banned))
                {
                    continue;
                }

                string bannedNorm = NormalizeForMatch(banned);
                if (bannedNorm.Length == 0)
                {
                    continue;
                }

                if (normalized.Contains(bannedNorm))
                {
                    reason = "Name contains blocked word";
                    return false;
                }
            }
        }

        return true;
    }

    private static string NormalizeForMatch(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        StringBuilder sb = new StringBuilder(value.Length);
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(char.ToLowerInvariant(c));
            }
        }
        return sb.ToString();
    }
}
