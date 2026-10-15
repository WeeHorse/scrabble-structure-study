using ScoreSystem;
using TileSystem;

namespace BoardSystem;

// The standard 15x15 board. The layout is given as the top half (rows 0-7)
// and mirrored vertically; each half-row is mirrored horizontally by design.
//   T = triple word   D = double word   t = triple letter   d = double letter
//   * = start square (also a double word)
class Board
{
  public const int Size = 15;

  private static readonly string[] Template =
  [
    "T..d...T...d..T",
    ".D...t...t...D.",
    "..D...d.d...D..",
    "d..D...d...D..d",
    "....D.....D....",
    ".t...t...t...t.",
    "..d...d.d...d..",
    "T..d...*...d..T"
  ];

  private readonly Square[,] squares = new Square[Size, Size];

  public Board()
  {
    for (int y = 0; y < Size; y++)
    {
      string row = Template[y < Template.Length ? y : Size - 1 - y];
      for (int x = 0; x < Size; x++)
        squares[y, x] = Create(row[x]);
    }
  }

  public Square this[int x, int y] => squares[y, x];

  private static Square Create(char c) => c switch
  {
    'T' => new Square(ScoreType.TripleWord),
    'D' => new Square(ScoreType.DoubleWord),
    't' => new Square(ScoreType.TripleLetter),
    'd' => new Square(ScoreType.DoubleLetter),
    '*' => new Square(ScoreType.DoubleWord, isStart: true),
    _ => new Square()
  };
}
