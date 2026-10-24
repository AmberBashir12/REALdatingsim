public static class TextTemplate
{
    // Usage in your StoryScene / ChooseScene text fields:
    // "Hello {playerName}!" or "{player}".
    public static string Resolve(string raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return string.Empty;
        }

        string playerName = PlayerProfile.DisplayName;
        string they = PlayerProfile.PronounThey;
        string their = PlayerProfile.PronounTheir;
        string theirs = PlayerProfile.PronounTheirs;

        // Keep it simple: a couple of common tokens.
        return raw
            .Replace("{playerName}", playerName)
            .Replace("{PlayerName}", playerName)
            .Replace("{player}", playerName)
            .Replace("{Player}", playerName)
            .Replace("{they}", they)
            .Replace("{They}", Capitalize(they))
            .Replace("{their}", their)
                .Replace("{Their}", Capitalize(their))
                .Replace("{theirs}", theirs)
                .Replace("{Theirs}", Capitalize(theirs));
    }

    private static string Capitalize(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Length == 1)
        {
            return value.ToUpperInvariant();
        }

        return char.ToUpperInvariant(value[0]) + value.Substring(1);
    }
}
