using Newtonsoft.Json;

namespace DictionarySystem;

// Strict: only base forms (the keys) are valid. Lenient: inflected forms (the values) are valid too.
enum WordRules
{
  Strict,
  Lenient
}

// set of valid words, used to validate a Word
class Dictionary
{
  private readonly HashSet<string> words = [];

  public Dictionary(string path, WordRules rules = WordRules.Lenient)
  {
    var wordlist = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText(path))
      ?? new Dictionary<string, List<string>>();

    foreach (var (baseForm, inflections) in wordlist)
    {
      words.Add(baseForm.ToLowerInvariant());

      if (rules == WordRules.Lenient)
        foreach (var inflection in inflections)
          words.Add(inflection.ToLowerInvariant());
    }
  }

  public static Dictionary LoadSwedish(WordRules rules = WordRules.Lenient) =>
    new(Path.Combine(AppContext.BaseDirectory, "wordlists", "swedish.json"), rules);

  public int Count => words.Count;

  public bool Contains(string word) => words.Contains(word.ToLowerInvariant());
}
