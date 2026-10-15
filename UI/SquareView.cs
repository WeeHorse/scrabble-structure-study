using BoardSystem;
using ScoreSystem;

namespace UISystem;

// What the UI needs to know about one square: its multiplier, whether it is the
// start square, and the tile on it (if any).
record SquareView(ScoreType Score, bool IsStart, char? Letter, int Points, bool IsBlank)
{
  // The map the UI renders, indexed [y, x].
  public static SquareView[,] MapOf(Board board)
  {
    var map = new SquareView[Board.Size, Board.Size];
    for (int y = 0; y < Board.Size; y++)
      for (int x = 0; x < Board.Size; x++)
      {
        var square = board[x, y];
        map[y, x] = new SquareView(
          square.ScoreType, square.IsStart,
          square.Tile?.Letter, square.Tile?.Points ?? 0, square.Tile?.IsBlank ?? false);
      }
    return map;
  }
}

record PlayerView(string Name, int Score);
