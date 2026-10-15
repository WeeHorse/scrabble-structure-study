using DictionarySystem;
using TileSystem;

namespace UISystem;

// The game loop talks to the player only through this interface. The UI collects
// what the player wants to do; checking whether it is allowed is up to the game.
interface IGameUI
{
  WordRules AskWordRules();

  bool AskEndWordOption();

  IReadOnlyList<string> AskPlayerNames();

  // One player's secret suggestion for the end word; the error (if any) is why the previous attempt was refused.
  string PromptEndWordSuggestion(PlayerView player, IReadOnlyList<Tile> rack, string? error);

  // Shows the end word on the scoreboard from now on (null hides it).
  void ShowEndWord(string? word, int bonus);

  void ShowScoreboard(IReadOnlyList<PlayerView> players, int tilesInBag);

  // Hides the board and racks until the next player is at the keyboard.
  void AwaitPlayer(PlayerView next);

  // Lets the current player build a move on the board. Blocks until they
  // place, exchange, pass or quit.
  TurnAction PromptTurn(PlayerView current, IReadOnlyList<Tile> rack, SquareView[,] map);

  void ShowMessage(string message);
  void ShowError(string message);

  void ShowGameOver(IReadOnlyList<PlayerView> ranking, string? note = null);
}
