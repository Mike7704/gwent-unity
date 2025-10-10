using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Called when Play button is clicked
    public void PlayGame()
    {
        Debug.Log("Play Game");
        SceneManager.LoadScene("GameScene");
    }

    // Called when Deck button is clicked
    public void DeckMenu()
    {
        Debug.Log("Deck Menu");
        SceneManager.LoadScene("DeckMenu");
    }

    // Called when Quit button is clicked
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}