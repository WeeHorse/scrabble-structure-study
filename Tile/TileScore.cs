namespace TileSystem;

static class TileScore
{
  // a blank is worth nothing, whatever letter it stands for
  public static int Value(Tile tile) => tile.IsBlank ? 0 : tile.Points;
}
