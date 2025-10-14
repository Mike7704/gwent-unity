using UnityEngine;
using System.Collections;

public class InitialiseCards : MonoBehaviour
{
    private IEnumerator Start()
    {
        // Wait for all singletons to be ready
        yield return new WaitUntil(() =>
            CardDatabase.Instance != null &&
            CardPool.Instance != null &&
            DeckManager.Instance != null
        );

        // Initialise in the correct order
        if (CardDatabase.Instance.allCards == null || CardDatabase.Instance.allCards.Count == 0)
        {
            CardDatabase.Instance.LoadAllCards();
        }

        CardPool.Instance.PreloadAllCards(CardDatabase.Instance.allCards);
        DeckManager.Instance.LoadDeck();

        Debug.Log("All cards and decks successfully initialised.");
    }
}
