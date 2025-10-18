using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoToAbout : MonoBehaviour
{
    public Button aboutButton; // assign Inspector
    public string aboutSceneName = "About Us"; // Name of scene where it going

    void Start()
    {
        if (aboutButton != null)
        {
            aboutButton.onClick.AddListener(OpenAbout); // Opening the aboutus when you click
        }
        else
        {
            Debug.LogWarning("About button not set in Inspector!");
        }
    }

    public void OpenAbout()
    {
        SceneManager.LoadScene(aboutSceneName); // load About scene
    }
}
