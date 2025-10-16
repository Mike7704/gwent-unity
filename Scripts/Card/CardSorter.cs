using System.Collections.Generic;

public static class CardSorter
{
    private static readonly string[] rangeOrder = { "melee", "agile", "ranged", "siege" };

    /// <summary>
    /// Sorts a list of cards by type, strength, range, and ID.
    /// </summary>
    public static void Sort(List<CardData> cards)
    {
        if (cards == null || cards.Count == 0)
            return;

        cards.Sort((a, b) =>
        {
            bool aIsSpecial = a.type == "special";
            bool bIsSpecial = b.type == "special";

            bool aIsFactionSpecial = aIsSpecial && a.faction == "Special";
            bool bIsFactionSpecial = bIsSpecial && b.faction == "Special";

            // Special cards always last
            if (aIsFactionSpecial && !bIsFactionSpecial) return 1;
            if (bIsFactionSpecial && !aIsFactionSpecial) return -1;

            // Faction-specific specials come after regular cards
            if (aIsSpecial && !bIsSpecial) return 1;
            if (bIsSpecial && !aIsSpecial) return -1;

            // Sort specials by ID
            if (aIsSpecial && bIsSpecial)
                return a.id.CompareTo(b.id);

            // Regular cards sorted by strength, then range, then ID
            int strengthCompare = b.strength.CompareTo(a.strength);
            if (strengthCompare != 0) return strengthCompare;

            int aRangeIndex = System.Array.IndexOf(rangeOrder, a.range);
            int bRangeIndex = System.Array.IndexOf(rangeOrder, b.range);
            int rangeCompare = aRangeIndex.CompareTo(bRangeIndex);
            if (rangeCompare != 0) return rangeCompare;

            return a.id.CompareTo(b.id);
        });
    }
}
