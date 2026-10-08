namespace BoardSystem;

using ScoreSystem;

class Square
{

  private ScoreType ScoreType;

  public Square(ScoreType scoreType = ScoreType.Normal)
  {
    ScoreType = scoreType;
  }

}
