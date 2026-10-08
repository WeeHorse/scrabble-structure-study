using Newtonsoft.Json;

namespace DictionarySystem;

class Dictionary
{
  // set of valid words, used to validate a Word
  public Dictionary()
  {
    ReadWordList();
  }

  private Dictionary<string, List<string>> ReadWordList()
  {

    var json = "{\"cykel\":[\"cyklar\", \"cykeln\" ]}";
    var wordlist = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json)
      ?? new Dictionary<string, List<string>>();
    return wordlist;
  }


}
