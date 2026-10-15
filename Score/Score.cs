namespace ScoreSystem;

class Score
{
  public const int BingoBonus = 50;

  // for playing the end word (see EndWord)
  public const int EndWordBonus = 50;

  // the game ends after this many scoreless turns (pass or exchange) in a row
  public const int ScorelessTurnLimit = 6;
}


/*
square 
tile 
  word
    move
      (round)
      player
*/
