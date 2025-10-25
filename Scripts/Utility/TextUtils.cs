using UnityEngine;

public static class TextUtils
{
    /// <summary>
    /// Capitalize the first letter of a string
    /// </summary>
    public static string CapFirstLetter(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return input.Substring(0, 1).ToUpper() + input.Substring(1);
    }

}