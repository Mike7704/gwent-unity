using UnityEngine;
using UnityEngine.SceneManagement;

public class DeckMenu : MonoBehaviour
{
    public Transform contentPanel;  // Content of the Scroll View
    public GameObject cardPrefab;

    void Start()
    {
        DisplayFaction("Northern Realms");
    }

    void DisplayFaction(string faction)
    {
        // Clear existing cards
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Get cards for this faction
        var cards = CardDatabase.Instance.GetCardsByFaction(faction);

        foreach (var card in cards)
        {
            GameObject cardGO = Instantiate(cardPrefab, contentPanel);
            CardUI cardUI = cardGO.GetComponent<CardUI>();
            if (cardUI != null) cardUI.Setup(card);
        }
    }

    // Called when Back button is clicked
    public void BackToMainMenu()
    {
        // Clear cached sprites
        StartCoroutine(SpriteCache.ClearAndUnload());

        Debug.Log("Main Menu");
        SceneManager.LoadScene("MainMenu");
    }
}
