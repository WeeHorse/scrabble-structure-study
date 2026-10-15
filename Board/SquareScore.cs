using ScoreSystem;

namespace BoardSystem;

// what a square does to the tile on it (letter) and to the word it is part of
static class SquareScore
{
  public static int LetterMultiplier(ScoreType type) => type switch
  {
    ScoreType.DoubleLetter => 2,
    ScoreType.TripleLetter => 3,
    _ => 1
  };

  public static int WordMultiplier(ScoreType type) => type switch
  {
    ScoreType.DoubleWord => 2,
    ScoreType.TripleWord => 3,
    _ => 1
  };
}
