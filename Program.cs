using BoardSystem;
using PlayerSystem;
using RoundSystem;
using ScoreSystem;
using TileSystem;
using UISystem;

var board = new Board();
var bag = new TileBag();
using var ui = new ConsoleUI();

var dictionary = DictionarySystem.Dictionary.LoadSwedish(ui.AskWordRules());
bool useEndWord = ui.AskEndWordOption();

var players = ui.AskPlayerNames().Select(name => new Player(name)).ToList();

var (first, drawDescription) = TurnOrder.DrawForFirst(bag, players);
int firstIndex = players.IndexOf(first);
players = players.Skip(firstIndex).Concat(players.Take(firstIndex)).ToList();

foreach (var player in players)
  player.Rack.Refill(bag);

// optional preplay: the players' suggestions decide a word that ends the game
string? endWord = null;
if (useEndWord)
{
  var suggestions = new List<(string Player, string Word)>();
  foreach (var player in players)
  {
    var view = new PlayerView(player.Name, player.Score);
    ui.AwaitPlayer(view);

    string? error = null;
    string suggestion;
    do
    {
      suggestion = ui.PromptEndWordSuggestion(view, player.Rack.Tiles, error);
      error = EndWord.Validate(dictionary, suggestion);
    } while (error != null);

    suggestions.Add((player.Name, suggestion));
  }

  endWord = EndWord.Pick(dictionary, suggestions.Select(s => s.Word));
  ui.RevealEndWord(endWord, Score.EndWordBonus, suggestions);
}

ui.ShowMessage($"Draw for first turn: {drawDescription}");

int turn = 0;
int scorelessTurns = 0;
bool quit = false;
string? endWordPlayer = null;
bool handover = true;

while (true)
{
  var player = players[turn % players.Count];

  if (handover)
    ui.AwaitPlayer(new PlayerView(player.Name, player.Score));
  handover = false;

  ui.ShowScoreboard(players.Select(p => new PlayerView(p.Name, p.Score)).ToList(), bag.Count);
  var action = ui.PromptTurn(new PlayerView(player.Name, player.Score), player.Rack.Tiles, SquareView.MapOf(board));

  if (action is Quit)
  {
    quit = true;
    break;
  }

  switch (action)
  {
    case PlaceTiles place:
      var result = Turn.Evaluate(board, dictionary, place.Placements);
      if (!result.IsValid)
      {
        ui.ShowError(result.Error!);
        continue;
      }

      foreach (var (x, y, tile) in place.Placements)
      {
        board[x, y].Tile = tile;
        player.Rack.Remove(tile);
      }
      player.Rack.Refill(bag);
      player.Score += result.Points;
      ui.ShowMessage($"{player.Name} scored {result.Points}: {string.Join(", ", result.Words.Select(w => w.Text))}");
      scorelessTurns = 0;

      if (endWord != null && result.Words.Any(w => w.Text.Equals(endWord, StringComparison.OrdinalIgnoreCase)))
      {
        player.Score += Score.EndWordBonus;
        endWordPlayer = player.Name;
      }
      break;

    case ExchangeTiles exchange:
      if (bag.Count < PlayerRack.Capacity)
      {
        ui.ShowError("Fewer than 7 tiles left in the bag; you cannot exchange.");
        continue;
      }
      foreach (var tile in exchange.Tiles)
        player.Rack.Remove(tile);
      player.Rack.Refill(bag);
      bag.Return(exchange.Tiles);
      scorelessTurns++;
      break;

    case Pass:
      scorelessTurns++;
      break;
  }

  // the game ends when the bag and a rack are empty, or after six scoreless turns in a row
  if (endWordPlayer != null) break;
  if (bag.Count == 0 && player.Rack.Tiles.Count == 0) break;
  if (scorelessTurns >= Score.ScorelessTurnLimit) break;

  turn++;
  handover = true;
}

// tiles left on a rack count against its owner; whoever went out collects them
// (a game that was quit is not settled)
if (!quit)
  foreach (var p in players)
  {
    int leftover = p.Rack.Tiles.Sum(TileScore.Value);
    p.Score -= leftover;
    if (p.Rack.Tiles.Count == 0)
      p.Score += players.Sum(other => other.Rack.Tiles.Sum(TileScore.Value));
  }

ui.ShowGameOver(
  players.OrderByDescending(p => p.Score).Select(p => new PlayerView(p.Name, p.Score)).ToList(),
  endWordPlayer == null ? null : $"{endWordPlayer} played the end word {endWord!.ToUpperInvariant()} (+{Score.EndWordBonus}) and ended the game.");
