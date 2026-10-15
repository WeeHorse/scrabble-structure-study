using BoardSystem;
using TileSystem;

namespace WordSystem;

static class WordScore
{
  // Multipliers only count for tiles placed this turn, never for tiles that
  // were already on the board.
  public static int Score(Word word, Board board)
  {
    int sum = 0;
    int wordMultiplier = 1;

    foreach (var placed in word.Tiles)
    {
      int value = TileScore.Value(placed.Tile);
      if (placed.IsNew)
      {
        var type = board[placed.X, placed.Y].ScoreType;
        value *= SquareScore.LetterMultiplier(type);
        wordMultiplier *= SquareScore.WordMultiplier(type);
      }
      sum += value;
    }

    return sum * wordMultiplier;
  }
}
