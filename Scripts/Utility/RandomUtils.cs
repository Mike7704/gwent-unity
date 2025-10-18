using System.Collections.Generic;
using UnityEngine;

public static class RandomUtils
{
    /// <summary>
    /// Returns a random integer between min and max (inclusive).
    /// </summary>
    public static int GetRandom(int min, int max)
    {
        return Random.Range(min, max + 1);
    }

    /// <summary>
    /// Returns a random float between min and max (inclusive).
    /// </summary>
    public static float GetRandom(float min, float max)
    {
        return Random.Range(min, max);
    }

    /// <summary>
    /// Returns a random element from a list or array.
    /// </summary>
    public static T Pick<T>(IList<T> list)
    {
        if (list == null || list.Count == 0)
            return default;
        return list[Random.Range(0, list.Count)];
    }

    /// <summary>
    /// Shuffles a list in place.
    /// </summary>
    public static void Shuffle<T>(IList<T> list)
    {
        if (list == null || list.Count <= 1) return;

        for (int i = 0; i < list.Count; i++)
        {
            int randIndex = Random.Range(i, list.Count);
            (list[i], list[randIndex]) = (list[randIndex], list[i]);
        }
    }
}
