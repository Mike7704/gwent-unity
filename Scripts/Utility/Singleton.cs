using UnityEngine;

/// Generic base classes that handle how certain scripts behave when multiple copies exist across scenes.
/// They’re written using C# generics (<T>) so that they can be reused for any type of class.

/// <summary>
/// A static instance is similar to to a singleton, but instead of destroying any new instances,
/// it overrides the current instance. This is useful for resetting the instance when changing scenes.
/// </summary>
public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }
    protected virtual void Awake() => Instance = this as T;

    protected virtual void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}

/// <summary>
/// This transforms the static instance into a singleton, destroying any new instances.
/// </summary>
public abstract class Singleton<T> : StaticInstance<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        base.Awake();
    }
}

/// <summary>
/// Persistent version of the singleton, which will not be destroyed when changing scenes.
/// </summary>
public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}
