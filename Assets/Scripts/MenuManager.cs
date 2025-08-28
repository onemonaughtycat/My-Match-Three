using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private DifficultyScriptableObject easyDifficulty;
    [SerializeField] private DifficultyScriptableObject hardDifficulty;

    public void StartGame()
    {
        if (MainManager.instance != null)
        {
            MainManager.instance.difficulty = easyDifficulty;
        }

        SceneManager.LoadScene(1);
    }
 
    public void StartGameInHardMode()
    {
        if (MainManager.instance != null)
        {
            MainManager.instance.difficulty = hardDifficulty;
        }

        SceneManager.LoadScene(1);
    }

    public void Exit()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}
