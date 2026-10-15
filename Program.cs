using BoardSystem;
using PlayerSystem;
using RoundSystem;
using TileSystem;
using UISystem;

var board = new Board();
var bag = new TileBag();
var dictionary = DictionarySystem.Dictionary.LoadSwedish(
  args.Contains("--strict") ? DictionarySystem.WordRules.Strict : DictionarySystem.WordRules.Lenient);

using var ui = new ConsoleUI();

var players = ui.AskPlayerNames().Select(name => new Player(name)).ToList();
foreach (var player in players)
  player.Rack.Refill(bag);

int turn = 0;
int consecutivePasses = 0;

while (true)
{
  var player = players[turn % players.Count];

  ui.ShowScoreboard(players.Select(p => new PlayerView(p.Name, p.Score)).ToList(), bag.Count);
  var action = ui.PromptTurn(new PlayerView(player.Name, player.Score), player.Rack.Tiles, SquareView.MapOf(board));

  if (action is Quit) break;

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
      consecutivePasses = 0;
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
      consecutivePasses++;
      break;

    case Pass:
      consecutivePasses++;
      break;
  }

  // the game ends when the bag and a rack are empty, or everyone passes twice in a row
  if (bag.Count == 0 && player.Rack.Tiles.Count == 0) break;
  if (consecutivePasses >= players.Count * 2) break;

  turn++;
}

// tiles left on a rack count against its owner; whoever went out collects them
foreach (var p in players)
{
  int leftover = p.Rack.Tiles.Sum(TileScore.Value);
  p.Score -= leftover;
  if (p.Rack.Tiles.Count == 0)
    p.Score += players.Sum(other => other.Rack.Tiles.Sum(TileScore.Value));
}

ui.ShowGameOver(players.OrderByDescending(p => p.Score).Select(p => new PlayerView(p.Name, p.Score)).ToList());
