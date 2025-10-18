using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenu : MonoBehaviour
{
    //Button OnClick
    public void GoToMenu()
    {
        Debug.Log("Back button pressed!");
        SceneManager.LoadScene("HomeScreen"); // Back To HomePAge
    }
}
