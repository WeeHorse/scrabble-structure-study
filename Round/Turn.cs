using BoardSystem;
using DictionarySystem;
using PlayerSystem;
using ScoreSystem;
using TileSystem;
using WordSystem;

namespace RoundSystem;

record TurnResult(string? Error, int Points, IReadOnlyList<Word> Words)
{
  public bool IsValid => Error == null;
}

class Turn
{
  // Checks a move against the rules and the dictionary and scores it.
  // Nothing is placed; the caller applies the move if the result is valid.
  public static TurnResult Evaluate(
    Board board, DictionarySystem.Dictionary dictionary, IReadOnlyList<(int X, int Y, Tile Tile)> placements)
  {
    var (words, error) = WordFinder.Find(board, placements);
    if (error != null)
      return new TurnResult(error, 0, []);

    var unknown = words.Select(w => w.Text).Where(t => !dictionary.Contains(t)).ToList();
    if (unknown.Count > 0)
      return new TurnResult($"Not in the word list: {string.Join(", ", unknown)}", 0, words);

    int points = words.Sum(w => WordScore.Score(w, board));
    if (placements.Count == PlayerRack.Capacity)
      points += Score.BingoBonus;

    return new TurnResult(null, points, words);
  }
}
