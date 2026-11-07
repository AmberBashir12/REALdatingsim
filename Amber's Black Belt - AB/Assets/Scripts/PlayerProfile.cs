using System;
using UnityEngine;

public static class PlayerProfile
{
    private const string PlayerNameKey = "playerName";
    private const string PlayerCharacterKey = "playerCharacter";
    private static string cachedName;
    private static string cachedCharacterChoice;

    public static event Action<string> NameChanged;
    public static event Action<string> CharacterChoiceChanged;

    public static string Name
    {
        get
        {
            if (cachedName == null)
            {
                cachedName = PlayerPrefs.GetString(PlayerNameKey, string.Empty);
            }

            return cachedName;
        }
        set => SetName(value);
    }

    public static string CharacterChoice
    {
        get
        {
            if (cachedCharacterChoice == null)
            {
                cachedCharacterChoice = PlayerPrefs.GetString(PlayerCharacterKey, string.Empty);
            }

            return cachedCharacterChoice;
        }
    }

    public static void SetCharacterChoice(string value)
    {
        string normalized = (value ?? string.Empty).Trim();
        cachedCharacterChoice = normalized;

        PlayerPrefs.SetString(PlayerCharacterKey, normalized);
        PlayerPrefs.Save();

        CharacterChoiceChanged?.Invoke(normalized);
    }




    public static string DisplayName
    {
        get
        {
            string name = Name;
            return string.IsNullOrWhiteSpace(name) ? "Player" : name;
        }
    }

    public static void SetName(string value)
    {
        string normalized = (value ?? string.Empty).Trim();
        cachedName = normalized;

        PlayerPrefs.SetString(PlayerNameKey, normalized);
        PlayerPrefs.Save();

        NameChanged?.Invoke(normalized);
    }

    public static string ResolveText(string raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return string.Empty;
        }

        string playerName = DisplayName;
        return raw
            .Replace("{playerName}", playerName)
            .Replace("{PlayerName}", playerName)
            .Replace("{player}", playerName)
            .Replace("{Player}", playerName);
    }
}
