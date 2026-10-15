using BoardSystem;
using PlayerSystem;
using TileSystem;
using UISystem;

var board = new Board();
var bag = new TileBag();

using var ui = new ConsoleUI();

var players = ui.AskPlayerNames().Select(name => new Player(name)).ToList();
foreach (var player in players)
  player.Rack.Refill(bag);

// Moves are not validated or scored yet; that belongs to the domain classes.
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
      foreach (var (x, y, tile) in place.Placements)
      {
        board[x, y].Tile = tile;
        player.Rack.Remove(tile);
      }
      player.Rack.Refill(bag);
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

ui.ShowGameOver(players.OrderByDescending(p => p.Score).Select(p => new PlayerView(p.Name, p.Score)).ToList());
