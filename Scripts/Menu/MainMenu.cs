using UnityEngine;

/// <summary>
/// Manages main menu interactions.
/// </summary>
public class MainMenu : Singleton<MainMenu>
{
    // Called when Play button is clicked
    public void PlayGame()
    {
        GameManager.Instance.ChangeState(GameState.SinglePlayer);
    }

    // Called when Deck button is clicked
    public void DeckMenu()
    {
        GameManager.Instance.ChangeState(GameState.DeckMenu);
    }

    // Called when Settings button is clicked
    public void SettingsMenu()
    {
        GameManager.Instance.ChangeState(GameState.SettingsMenu);
    }

    // Called when Quit button is clicked
    public void QuitGame()
    {
        Application.Quit();
    }
}