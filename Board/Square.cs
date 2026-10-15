namespace BoardSystem;

using ScoreSystem;
using TileSystem;

class Square
{

  public ScoreType ScoreType { get; }
  public bool IsStart { get; }
  public Tile? Tile { get; set; }

  public Square(ScoreType scoreType = ScoreType.Normal, bool isStart = false)
  {
    ScoreType = scoreType;
    IsStart = isStart;
  }

}
