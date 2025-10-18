using System;
using System.Collections.Generic;

public static class CardSorter
{
    private static readonly string[] defaultRangeOrder = { CardDefs.Range.Melee, CardDefs.Range.Agile, CardDefs.Range.Ranged, CardDefs.Range.Siege };

    /// <summary>
    /// Sorts a list of cards by type, strength, range, and ID.
    /// </summary>
    public static void Sort(List<CardData> cards, string[] rangeOrder = null)
    {
        if (cards == null || cards.Count == 0)
            return;

        rangeOrder ??= defaultRangeOrder;

        cards.Sort((a, b) =>
        {
            bool aIsSpecial = string.Equals(a.type, CardDefs.Type.Special, StringComparison.OrdinalIgnoreCase);
            bool bIsSpecial = string.Equals(b.type, CardDefs.Type.Special, StringComparison.OrdinalIgnoreCase);

            bool aIsFactionSpecial = aIsSpecial && string.Equals(a.faction, CardDefs.Faction.Special, StringComparison.OrdinalIgnoreCase);
            bool bIsFactionSpecial = bIsSpecial && string.Equals(b.faction, CardDefs.Faction.Special, StringComparison.OrdinalIgnoreCase);

            // Special cards always last
            if (aIsFactionSpecial && !bIsFactionSpecial) return 1;
            if (bIsFactionSpecial && !aIsFactionSpecial) return -1;

            // Faction-specific specials come after regular cards
            if (aIsSpecial && !bIsSpecial) return 1;
            if (bIsSpecial && !aIsSpecial) return -1;

            // Sort specials by ID
            if (aIsSpecial && bIsSpecial)
                return a.id.CompareTo(b.id);

            // Regular cards sorted by strength
            int strengthCompare = b.strength.CompareTo(a.strength);
            if (strengthCompare != 0) return strengthCompare;

            // Then by range
            int aRangeIndex = Array.IndexOf(rangeOrder, a.range ?? "");
            int bRangeIndex = Array.IndexOf(rangeOrder, b.range ?? "");
            int rangeCompare = aRangeIndex.CompareTo(bRangeIndex);
            if (rangeCompare != 0) return rangeCompare;

            // Finally by ID
            return a.id.CompareTo(b.id);
        });
    }
}
