using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeckMenu : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentPanel;  // Content of the Scroll View

    [Header("Faction Buttons")]
    public Button NorthernRealmsButton;
    public Button NilfgaardButton;
    public Button ScoiataelButton;
    public Button MonstersButton;
    public Button SkelligeButton;

    private List<CardUI> visibleCards = new List<CardUI>();

    void Start()
    {
        NorthernRealmsButton.onClick.AddListener(() => DisplayFaction("Northern Realms"));
        NilfgaardButton.onClick.AddListener(() => DisplayFaction("Nilfgaard"));
        ScoiataelButton.onClick.AddListener(() => DisplayFaction("Scoiatael"));
        MonstersButton.onClick.AddListener(() => DisplayFaction("Monsters"));
        SkelligeButton.onClick.AddListener(() => DisplayFaction("Skellige"));

        DisplayFaction("Northern Realms");
    }

    /// <summary>
    /// Display all cards for a given faction in the scroll view.
    /// Fetches cards from the pool instead of instantiating new ones.
    /// </summary>
    public void DisplayFaction(string faction)
    {
        ReturnVisibleCards();

        var cards = CardDatabase.Instance.GetCardsByFaction(faction);

        foreach (var data in cards)
        {
            CardUI cardUI = CardPool.Instance.GetCard(contentPanel);
            if (cardUI != null)
            {
                cardUI.Setup(data); // Update visuals for this specific card
                visibleCards.Add(cardUI);
            }
            else
            {
                Debug.LogWarning("CardPool exhausted! Not enough preloaded cards for faction display.");
            }
        }
    }

    /// <summary>
    /// Return all visible cards to the pool and clear the list to free memory
    /// </summary>
    private void ReturnVisibleCards()
    {
        foreach (var card in visibleCards)
            CardPool.Instance.ReturnCard(card);
        visibleCards.Clear();
    }

    /// <summary>
    /// Called when Back button is clicked
    /// </summary>
    public void BackToMainMenu()
    {
        ReturnVisibleCards();
        StartCoroutine(LoadMainMenuAsync());
    }

    private IEnumerator LoadMainMenuAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainMenu");
        asyncLoad.allowSceneActivation = true;
        yield return asyncLoad;
    }
}