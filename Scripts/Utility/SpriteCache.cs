using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpriteCache
{
    private static readonly Dictionary<string, Sprite> cache = new();

    // Load a sprite from Resources with caching
    public static Sprite Load(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        // Remove .png if included
        if (path.EndsWith(".png"))
            path = path[..^4];

        // Return from cache if loaded
        if (cache.TryGetValue(path, out Sprite sprite))
            return sprite;

        // Otherwise, load from Resources and store it
        sprite = Resources.Load<Sprite>(path);

        if (sprite == null)
            Debug.LogWarning($"Sprite not found: {path}");
        else
            cache[path] = sprite;

        return sprite;
    }

    // Clear the cache
    public static void Clear()
    {
        cache.Clear();
    }

    public static IEnumerator ClearAndUnload()
    {
        cache.Clear();
        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
}
