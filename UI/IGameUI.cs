using DictionarySystem;
using TileSystem;

namespace UISystem;

// The game loop talks to the player only through this interface. The UI collects
// what the player wants to do; checking whether it is allowed is up to the game.
interface IGameUI
{
  WordRules AskWordRules();

  IReadOnlyList<string> AskPlayerNames();

  void ShowScoreboard(IReadOnlyList<PlayerView> players, int tilesInBag);

  // Hides the board and racks until the next player is at the keyboard.
  void AwaitPlayer(PlayerView next);

  // Lets the current player build a move on the board. Blocks until they
  // place, exchange, pass or quit.
  TurnAction PromptTurn(PlayerView current, IReadOnlyList<Tile> rack, SquareView[,] map);

  void ShowMessage(string message);
  void ShowError(string message);

  void ShowGameOver(IReadOnlyList<PlayerView> ranking);
}
