using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToWelcome : MonoBehaviour
{
    [Tooltip("Exact name of the Welcome scene to load.")]
    public string welcomeSceneName = "WelcomeScene";

    // Call this from Button OnClick
    public void GoToWelcome()
    {
        // check if scene is in Build Settings
        if (Application.CanStreamedLevelBeLoaded(welcomeSceneName))
        {
            SceneManager.LoadScene(welcomeSceneName);
        }
        else
        {
            Debug.LogError($"Scene '{welcomeSceneName}' is not added to Build Settings. Please check the name.");
        }
    }
}
