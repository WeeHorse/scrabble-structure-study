using BoardSystem;
using TileSystem;

namespace WordSystem;

// Works out which words a set of placed tiles forms and checks the placement
// rules (single line, no gaps, connected, covers the start square first).
// The board itself is not changed.
static class WordFinder
{
  public static (IReadOnlyList<Word> Words, string? Error) Find(
    Board board, IReadOnlyList<(int X, int Y, Tile Tile)> placements)
  {
    if (placements.Count == 0)
      return Fail("No tiles placed.");

    var added = new Dictionary<(int, int), Tile>();
    foreach (var (x, y, tile) in placements)
    {
      if (x < 0 || y < 0 || x >= Board.Size || y >= Board.Size)
        return Fail("Tile placed outside the board.");
      if (board[x, y].Tile != null || !added.TryAdd((x, y), tile))
        return Fail("A square can only hold one tile.");
    }

    Tile? At(int x, int y) =>
      x < 0 || y < 0 || x >= Board.Size || y >= Board.Size
        ? null
        : added.GetValueOrDefault((x, y)) ?? board[x, y].Tile;

    bool sameRow = placements.All(p => p.Y == placements[0].Y);
    bool sameColumn = placements.All(p => p.X == placements[0].X);
    if (!sameRow && !sameColumn)
      return Fail("Tiles must be placed in a single row or column.");

    // every square between the first and last new tile must be filled
    if (placements.Count > 1)
    {
      int dx = sameRow ? 1 : 0, dy = sameRow ? 0 : 1;
      int from = sameRow ? placements.Min(p => p.X) : placements.Min(p => p.Y);
      int to = sameRow ? placements.Max(p => p.X) : placements.Max(p => p.Y);
      for (int i = from; i <= to; i++)
      {
        int x = sameRow ? i : placements[0].X, y = sameRow ? placements[0].Y : i;
        if (At(x, y) == null)
          return Fail("Tiles must be placed without gaps.");
      }
    }

    Word? WordAt(int x, int y, bool horizontal)
    {
      int dx = horizontal ? 1 : 0, dy = horizontal ? 0 : 1;
      while (At(x - dx, y - dy) != null) (x, y) = (x - dx, y - dy);

      var tiles = new List<PlacedTile>();
      while (At(x, y) is { } tile)
      {
        tiles.Add(new PlacedTile(x, y, tile, added.ContainsKey((x, y))));
        (x, y) = (x + dx, y + dy);
      }
      return tiles.Count >= 2 ? new Word(tiles) : null;
    }

    var words = new List<Word>();
    var first = placements[0];

    if (placements.Count == 1)
    {
      words.AddRange(new[] { WordAt(first.X, first.Y, true), WordAt(first.X, first.Y, false) }.OfType<Word>());
    }
    else
    {
      if (WordAt(first.X, first.Y, sameRow) is { } main) words.Add(main);
      foreach (var p in placements)
        if (WordAt(p.X, p.Y, !sameRow) is { } cross) words.Add(cross);
    }

    if (words.Count == 0)
      return Fail("A word needs at least two letters.");

    bool boardIsEmpty = !board.HasTiles;
    if (boardIsEmpty)
    {
      if (!added.ContainsKey(board.Start))
        return Fail("The first word must cover the star in the middle.");
    }
    else if (!words.Any(w => w.Tiles.Any(t => !t.IsNew)))
    {
      return Fail("The word must connect to tiles already on the board.");
    }

    return (words, null);
  }

  private static (IReadOnlyList<Word>, string?) Fail(string error) => ([], error);
}
