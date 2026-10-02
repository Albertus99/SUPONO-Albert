using Supono.App.Characters;

namespace Supono.App.Rules
{
    /// <summary>What a character costs to unlock.</summary>
    public interface ICharacterPricing
    {
        int PriceOf(PlayableCharacter character);
    }

    /// <summary>The normal game: the catalog price.</summary>
    public sealed class CatalogPricing : ICharacterPricing
    {
        public int PriceOf(PlayableCharacter character) => character.price;
    }
}
