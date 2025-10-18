using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
///  Resource system to manage load game assets with caching.
/// </summary>
public class ResourceSystem : Singleton<ResourceSystem>
{
    private readonly Dictionary<string, Sprite> spriteCache = new();

    /// <summary>
    /// Load and cache a sprite from Resources.
    /// </summary>
    public Sprite LoadSprite(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        // Normalize path
        if (path.EndsWith(".png")) path = path[..^4];

        // Check cache
        if (spriteCache.TryGetValue(path, out var sprite))
            return sprite;

        // Load from Resources
        sprite = Resources.Load<Sprite>(path);

        if (sprite == null)
        {
            Debug.LogWarning($"[ResourceSystem] Sprite not found at path: {path}");
            return null;
        }

        // Cache it
        spriteCache[path] = sprite;
        return sprite;
    }

    /// <summary>
    /// Clear the sprite cache (e.g. when unloading menus).
    /// </summary>
    public void ClearSprites()
    {
        spriteCache.Clear();
    }

    /// <summary>
    /// Clear cache and unload unused assets to free memory.
    /// </summary>
    public void ClearAndUnload()
    {
        spriteCache.Clear();
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
}
