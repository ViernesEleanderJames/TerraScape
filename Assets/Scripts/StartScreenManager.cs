using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management

public class StartScreenManager : MonoBehaviour
{
    // Assign the name of your main AR scene in the Inspector
    public string arSceneName = "ARCamera"; // <<< --- IMPORTANT: Change this to your actual AR scene name!

    public void StartARExperience()
    {
        // Check if the scene name is provided
        if (string.IsNullOrEmpty(arSceneName))
        {
            Debug.LogError("AR Scene Name is not set in the StartScreenManager. Please set it in the Inspector.");
            return;
        }

        // Load the specified AR scene
        SceneManager.LoadScene(arSceneName);
    }
}