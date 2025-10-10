using UnityEngine;
using UnityEngine.SceneManagement;

public class DeckMenu : MonoBehaviour
{
    // Called when Back button is clicked
    public void BackToMainMenu()
    {
        Debug.Log("Main Menu");
        SceneManager.LoadScene("MainMenu");
    }
}
