using PlayerSystem;
using TileSystem;

namespace RoundSystem;

// Decides who starts: everybody draws a tile and the one closest to A goes
// first (a blank beats every letter). Players who tie draw again.
static class TurnOrder
{
  private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ";

  public static (Player First, string Description) DrawForFirst(TileBag bag, IReadOnlyList<Player> players)
  {
    var contenders = players.ToList();
    var description = new List<string>();

    while (true)
    {
      var draws = contenders.ToDictionary(p => p, p => bag.Draw(1).Single());
      description.Add(string.Join(", ", draws.Select(d => $"{d.Key.Name} {(d.Value.IsBlank ? "blank" : d.Value.Letter)}")));
      bag.Return(draws.Values);

      int best = draws.Values.Min(Rank);
      contenders = draws.Where(d => Rank(d.Value) == best).Select(d => d.Key).ToList();

      if (contenders.Count == 1)
        return (contenders[0], $"{string.Join(" - tie, draw again: ", description)}. {contenders[0].Name} starts.");
    }
  }

  private static int Rank(Tile tile) => tile.IsBlank ? -1 : Alphabet.IndexOf(tile.Letter);
}
