using TileSystem;

namespace WordSystem;

// a tile on the board, and whether it is being placed in the current turn
record PlacedTile(int X, int Y, Tile Tile, bool IsNew);

// a run of two or more tiles in a row or column; the tiles played in a Turn
// together with the existing tiles they connect to
class Word
{
  public IReadOnlyList<PlacedTile> Tiles { get; }

  public Word(IReadOnlyList<PlacedTile> tiles)
  {
    Tiles = tiles;
  }

  public string Text => new(Tiles.Select(t => t.Tile.Letter).ToArray());
}
