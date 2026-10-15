using System.Runtime.InteropServices;

namespace TileSystem;

// the pool of tiles left to draw from (standard Swedish distribution, 100 tiles)
class TileBag
{
  private static readonly (char Letter, int Count, int Points)[] Distribution =
  [
    ('A', 8, 1), ('B', 2, 4), ('C', 1, 8), ('D', 5, 1), ('E', 7, 1), ('F', 2, 3),
    ('G', 3, 2), ('H', 2, 3), ('I', 5, 1), ('J', 1, 7), ('K', 3, 2), ('L', 5, 1),
    ('M', 3, 2), ('N', 6, 1), ('O', 5, 2), ('P', 2, 4), ('R', 8, 1), ('S', 8, 1),
    ('T', 8, 1), ('U', 3, 4), ('V', 2, 3), ('X', 1, 8), ('Y', 1, 7), ('Z', 1, 10),
    ('Å', 2, 4), ('Ä', 2, 4), ('Ö', 2, 4)
  ];

  private const int BlankCount = 2;

  private readonly List<Tile> tiles = [];
  private readonly Random random = new();

  public TileBag()
  {
    foreach (var (letter, count, points) in Distribution)
      for (int i = 0; i < count; i++)
        tiles.Add(new Tile(letter, points));

    for (int i = 0; i < BlankCount; i++)
      tiles.Add(Tile.Blank);

    random.Shuffle(CollectionsMarshal.AsSpan(tiles));
  }

  public int Count => tiles.Count;

  // Draws up to n tiles (fewer if the bag runs out).
  public List<Tile> Draw(int n)
  {
    n = Math.Min(n, tiles.Count);
    var drawn = tiles.GetRange(tiles.Count - n, n);
    tiles.RemoveRange(tiles.Count - n, n);
    return drawn;
  }

  // Puts tiles back (used when exchanging) and reshuffles.
  public void Return(IEnumerable<Tile> returned)
  {
    tiles.AddRange(returned);
    random.Shuffle(CollectionsMarshal.AsSpan(tiles));
  }
}
