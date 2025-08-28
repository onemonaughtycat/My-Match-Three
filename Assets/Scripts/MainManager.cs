using UnityEngine;

public class MainManager : MonoBehaviour
{
    public static MainManager instance;

    [System.NonSerialized] public DifficultyScriptableObject difficulty;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
