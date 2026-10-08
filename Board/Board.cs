using ScoreSystem;

namespace BoardSystem;

class Board
{

  List<List<Square>> structure = [
    [new Square(),                       new Square(ScoreType.DoubleLetter), new Square()],
    [new Square(ScoreType.TripleWord),   new Square(),                       new Square(ScoreType.TripleWord)],
    [new Square(),                       new Square(ScoreType.DoubleLetter), new Square()]
  ];
  
  
}
