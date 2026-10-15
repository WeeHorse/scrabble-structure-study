namespace TileSystem;

// A blank is a tile with IsBlank set and no points. Once it is played it
// gets the letter the player chose (see AsLetter); in a rack it is '?'.
record Tile(char Letter, int Points, bool IsBlank = false)
{
  public static Tile Blank => new('?', 0, true);

  public Tile AsLetter(char letter) => this with { Letter = letter };
}
