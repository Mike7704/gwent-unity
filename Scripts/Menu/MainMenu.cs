using UnityEngine;

/// <summary>
/// Manages main menu interactions.
/// </summary>
public class MainMenu : Singleton<MainMenu>
{
    // Called when Play button is clicked
    private void PlayGame()
    {
        GameManager.Instance.ChangeState(GameState.SinglePlayer);
    }

    // Called when Deck button is clicked
    private void DeckMenu()
    {
        GameManager.Instance.ChangeState(GameState.DeckMenu);
    }

    // Called when Quit button is clicked
    private void QuitGame()
    {
        Application.Quit();
    }
}