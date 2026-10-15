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

  // The candidate with the most letters found in the pool (a letter counts as
  // many times as it occurs in the pool). Fewer left-over letters win a tie;
  // remaining ties are decided at random.
  public static string Pick(DictionarySystem.Dictionary dictionary, IEnumerable<string> suggestions, Random? random = null)
  {
    random ??= Random.Shared;

    var pool = suggestions
      .SelectMany(s => s.ToLowerInvariant())
      .GroupBy(c => c)
      .ToDictionary(g => g.Key, g => g.Count());

    string best = "";
    int bestMatched = -1, bestUnmatched = 0, ties = 0;

    foreach (var word in dictionary.Words)
    {
      if (word.Length is < MinLength or > MaxLength) continue;

      var used = new Dictionary<char, int>();
      int matched = 0;
      foreach (char c in word)
        if (pool.TryGetValue(c, out int available) && used.GetValueOrDefault(c) < available)
        {
          used[c] = used.GetValueOrDefault(c) + 1;
          matched++;
        }

      int unmatched = word.Length - matched;

      if (matched > bestMatched || (matched == bestMatched && unmatched < bestUnmatched))
      {
        (best, bestMatched, bestUnmatched, ties) = (word, matched, unmatched, 1);
      }
      else if (matched == bestMatched && unmatched == bestUnmatched && random.Next(++ties) == 0)
      {
        best = word;
      }
    }

    return best;
  }
}
