using UnityEngine;

public class PersistentCanvas : MonoBehaviour
{
    private static PersistentCanvas instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            // Another PersistentCanvas already exists, destroy this duplicate
            Destroy(gameObject);
            return;
        }

        // Make this instance persistent
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
