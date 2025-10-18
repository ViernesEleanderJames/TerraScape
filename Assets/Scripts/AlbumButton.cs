using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AlbumButton : MonoBehaviour
{
    [Header("Setup")]
    public Button albumButton;                   // Draging the button here in Inspector
    public string albumSceneName = "AlbumScene"; // Name of scene

    void Start()
    {
        // Auto assign if theres a button
        if (albumButton != null)
        {
            albumButton.onClick.AddListener(OpenAlbum);
        }
        else
        {
            Debug.LogWarning("AlbumButton: No button assigned in Inspector!");
        }
    }

    // Can also assign onclick in the inspector
    public void OpenAlbum()
    {
        Debug.Log("Opening Album Scene...");
        SceneManager.LoadScene(albumSceneName);
    }
}
