namespace RoundSystem;

// The "end word" option: before the game every player suggests a word, the
// letters of all suggestions are pooled, and the word list word that matches
// the pool best becomes the end word. Whoever plays it ends the game.
static class EndWord
{
  public const int MinLength = 5;
  public const int MaxLength = 7;

  public static string? Validate(DictionarySystem.Dictionary dictionary, string suggestion)
  {
    suggestion = suggestion.Trim();

    if (suggestion.Length is < MinLength or > MaxLength)
      return $"The word must have {MinLength}-{MaxLength} letters.";

    if (!dictionary.Contains(suggestion))
      return "That is not in the word list.";

    return null;
  }

  // The candidate that the suggestions explain best *together*. For each
  // candidate, "matched" is how many of its letters are found in the pooled
  // letters of all suggestions (a letter counts as many times as it occurs),
  // and "best single" is how many of its letters the best single suggestion
  // covers alone. Candidates are ranked by matched - best single (the letters
  // only the combination explains, so a word copied from one player scores 0),
  // then by matched, then by fewest left-over letters. Remaining ties are
  // decided at random.
  public static string Pick(DictionarySystem.Dictionary dictionary, IEnumerable<string> suggestions, Random? random = null)
  {
    random ??= Random.Shared;

    var perPlayer = suggestions.Select(Count).ToList();
    var pool = new Dictionary<char, int>();
    foreach (var counts in perPlayer)
      foreach (var (letter, n) in counts)
        pool[letter] = pool.GetValueOrDefault(letter) + n;

    string best = "";
    (int Combined, int Matched, int Fewer) bestScore = (-1, -1, 0);
    int ties = 0;

    foreach (var word in dictionary.Words)
    {
      if (word.Length is < MinLength or > MaxLength) continue;

      var counts = Count(word);
      int matched = Overlap(counts, pool);
      int bestSingle = perPlayer.Max(p => Overlap(counts, p));
      var score = (matched - bestSingle, matched, -(word.Length - matched));

      int comparison = score.CompareTo(bestScore);
      if (comparison > 0)
      {
        (best, bestScore, ties) = (word, score, 1);
      }
      else if (comparison == 0 && random.Next(++ties) == 0)
      {
        best = word;
      }
    }

    return best;
  }

  private static Dictionary<char, int> Count(string word) =>
    word.ToLowerInvariant().GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());

  // number of letters two words have in common, counting repeats
  private static int Overlap(Dictionary<char, int> a, Dictionary<char, int> b) =>
    a.Sum(letter => Math.Min(letter.Value, b.GetValueOrDefault(letter.Key)));
}
