using UnityEngine;
using UnityEngine.UI;

public class QuitApp : MonoBehaviour
{
    public Button quitButton;   // Assign your UI button in Inspector

    void Start()
    {
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(Quit);
        }
        else
        {
            Debug.LogWarning("Quit button not assigned in Inspector!");
        }
    }

    public void Quit()
    {
        Debug.Log("Application quitting...");

        // This works only in build, not in Editor
        Application.Quit();
    }
}
