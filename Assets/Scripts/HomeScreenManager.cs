using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeScreenManager : MonoBehaviour
{
    public string homeSceneName = "HomeScreen";       // Your Home Screen scene
    public string landscapingSceneName = "ARCamera";  // Landscaping AR scene
    public string measurementSceneName = "ARMeasurement"; // Measurement AR scene

    public void LoadLandscapingScene()
    {
        if (string.IsNullOrEmpty(landscapingSceneName))
        {
            Debug.LogError("Landscaping Scene Name is not set in the Inspector.");
            return;
        }
        SceneManager.LoadScene(landscapingSceneName);
    }

    public void LoadMeasurementScene()
    {
        if (string.IsNullOrEmpty(measurementSceneName))
        {
            Debug.LogError("Measurement Scene Name is not set in the Inspector.");
            return;
        }
        SceneManager.LoadScene(measurementSceneName);
    }

    public void LoadHomeScreen()
    {
        if (string.IsNullOrEmpty(homeSceneName))
        {
            Debug.LogError("Home Scene Name is not set in the Inspector.");
            return;
        }
        SceneManager.LoadScene(homeSceneName);
    }
}
