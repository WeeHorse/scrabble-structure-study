using TileSystem;

namespace PlayerSystem;

// the tiles a Player currently holds (max 7)
// after a turn (move) it should pick new tiles up to 7
class PlayerRack
{
  public const int Capacity = 7;

  private readonly List<Tile> tiles = [];

  public IReadOnlyList<Tile> Tiles => tiles;

  public void Refill(TileBag bag) => tiles.AddRange(bag.Draw(Capacity - tiles.Count));

  // Removes the rack tile that was played as `played`; a played blank
  // (which carries its chosen letter) matches a blank in the rack.
  public bool Remove(Tile played)
  {
    int index = tiles.FindIndex(t => played.IsBlank
      ? t.IsBlank
      : !t.IsBlank && t.Letter == played.Letter);

    if (index < 0) return false;
    tiles.RemoveAt(index);
    return true;
  }
}
