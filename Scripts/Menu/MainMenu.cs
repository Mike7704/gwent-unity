using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Called when Play button is clicked
    public void PlayGame()
    {
        Debug.Log("Game");
        SceneManager.LoadScene("GameScene");
    }

    // Called when Deck button is clicked
    public void DeckMenu()
    {
        Debug.Log("Deck");
        SceneManager.LoadScene("DeckScene");
    }

    // Called when Quit button is clicked
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}