using TileSystem;

namespace UISystem;

// What a player decided to do with their turn.
abstract record TurnAction;

// Tiles to put on the board. A blank carries the letter the player chose for it.
record PlaceTiles(IReadOnlyList<(int X, int Y, Tile Tile)> Placements) : TurnAction;
record ExchangeTiles(IReadOnlyList<Tile> Tiles) : TurnAction;
record Pass : TurnAction;
record Quit : TurnAction;
