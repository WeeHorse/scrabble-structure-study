using System.Text;
using ScoreSystem;
using TileSystem;

namespace UISystem;

// Console front end. Controls while building a move:
//   arrows move the cursor, letters place a tile from the rack, ? places a blank,
//   Tab switches typing direction, Backspace takes back the last tile,
//   Ctrl+R takes back all, Enter plays, Ctrl+P passes, Ctrl+E exchanges, Esc quits.
class ConsoleUI : IGameUI, IDisposable
{
  private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ";

  private record Pending(int X, int Y, Tile Tile, int RackIndex);

  // shown on every redraw
  private IReadOnlyList<PlayerView> players = [];
  private int tilesInBag;
  private string? message;
  private bool messageIsError;

  // state of the turn being built
  private PlayerView? current;
  private IReadOnlyList<Tile> rack = [];
  private SquareView[,] map = new SquareView[0, 0];
  private readonly List<Pending> pending = [];
  private HashSet<int>? exchangeSelection;
  private int cursorX = 7;
  private int cursorY = 7;
  private bool horizontal = true;

  public ConsoleUI()
  {
    Console.OutputEncoding = Encoding.UTF8;
    Console.InputEncoding = Encoding.UTF8;
    Console.CursorVisible = false;
  }

  public void Dispose()
  {
    Console.ResetColor();
    Console.CursorVisible = true;
  }

  public IReadOnlyList<string> AskPlayerNames()
  {
    Console.CursorVisible = true;
    Console.Clear();
    Console.WriteLine("SCRABBLE");
    Console.WriteLine();

    int count = 0;
    while (count is < 2 or > 4)
    {
      Console.Write("Number of players (2-4): ");
      int.TryParse(Console.ReadLine(), out count);
    }

    var names = new List<string>();
    for (int i = 1; i <= count; i++)
    {
      Console.Write($"Name of player {i}: ");
      string? name = Console.ReadLine()?.Trim();
      names.Add(string.IsNullOrEmpty(name) ? $"Player {i}" : name);
    }

    Console.CursorVisible = false;
    return names;
  }

  public void ShowScoreboard(IReadOnlyList<PlayerView> players, int tilesInBag)
  {
    this.players = players;
    this.tilesInBag = tilesInBag;
  }

  public void ShowMessage(string text) => (message, messageIsError) = (text, false);

  public void ShowError(string text) => (message, messageIsError) = (text, true);

  public void ShowGameOver(IReadOnlyList<PlayerView> ranking)
  {
    Console.ResetColor();
    Console.Clear();
    Console.WriteLine("GAME OVER");
    Console.WriteLine();
    for (int i = 0; i < ranking.Count; i++)
      Console.WriteLine($"{i + 1}. {ranking[i].Name,-20} {ranking[i].Score,5}");
    Console.WriteLine();
    Console.WriteLine("Press any key to exit.");
    Console.ReadKey(true);
  }

  public TurnAction PromptTurn(PlayerView currentPlayer, IReadOnlyList<Tile> playerRack, SquareView[,] board)
  {
    current = currentPlayer;
    rack = playerRack;
    map = board;
    pending.Clear();
    exchangeSelection = null;

    while (true)
    {
      Render();
      message = null;

      var key = Console.ReadKey(true);

      if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
      {
        switch (key.Key)
        {
          case ConsoleKey.P:
            return new Pass();
          case ConsoleKey.R:
            pending.Clear();
            break;
          case ConsoleKey.E:
            var exchange = PromptExchange();
            if (exchange != null) return exchange;
            break;
        }
        continue;
      }

      switch (key.Key)
      {
        case ConsoleKey.Escape:
          if (Confirm("Quit the game? (Y/N)")) return new Quit();
          break;

        case ConsoleKey.LeftArrow: cursorX = Math.Max(0, cursorX - 1); break;
        case ConsoleKey.RightArrow: cursorX = Math.Min(Board14, cursorX + 1); break;
        case ConsoleKey.UpArrow: cursorY = Math.Max(0, cursorY - 1); break;
        case ConsoleKey.DownArrow: cursorY = Math.Min(Board14, cursorY + 1); break;

        case ConsoleKey.Tab:
          horizontal = !horizontal;
          break;

        case ConsoleKey.Backspace:
        case ConsoleKey.Delete:
          if (pending.Count > 0)
          {
            var last = pending[^1];
            pending.RemoveAt(pending.Count - 1);
            (cursorX, cursorY) = (last.X, last.Y);
          }
          break;

        case ConsoleKey.Enter:
          if (pending.Count == 0)
            ShowError("Place at least one tile, or press Ctrl+P to pass.");
          else
            return new PlaceTiles(pending.Select(p => (p.X, p.Y, p.Tile)).ToList());
          break;

        default:
          if (key.KeyChar is '?' or '*')
            PlaceBlank();
          else
            PlaceLetter(char.ToUpperInvariant(key.KeyChar));
          break;
      }
    }
  }

  private int Board14 => map.GetLength(0) - 1;

  private bool IsFree(int x, int y) =>
    map[y, x].Letter == null && !pending.Any(p => p.X == x && p.Y == y);

  private void PlaceLetter(char letter)
  {
    if (!Letters.Contains(letter)) return;

    int index = FindRackIndex(t => !t.IsBlank && t.Letter == letter);
    if (index < 0)
    {
      ShowError($"You have no {letter} in your rack. Use ? for a blank.");
      return;
    }

    Place(rack[index], index);
  }

  private void PlaceBlank()
  {
    int index = FindRackIndex(t => t.IsBlank);
    if (index < 0)
    {
      ShowError("You have no blank tile.");
      return;
    }

    if (!CanPlaceAtCursor()) return;

    ShowMessage("Blank tile: press the letter it should stand for (Esc cancels).");
    Render();
    message = null;

    char letter = char.ToUpperInvariant(Console.ReadKey(true).KeyChar);
    if (Letters.Contains(letter))
      Place(rack[index].AsLetter(letter), index);
  }

  private int FindRackIndex(Func<Tile, bool> match)
  {
    for (int i = 0; i < rack.Count; i++)
      if (!pending.Any(p => p.RackIndex == i) && match(rack[i]))
        return i;
    return -1;
  }

  private bool CanPlaceAtCursor()
  {
    if (!IsFree(cursorX, cursorY))
    {
      ShowError("That square is taken.");
      return false;
    }

    // all tiles of one move must lie in one row or one column
    bool sameRow = pending.All(p => p.Y == cursorY);
    bool sameColumn = pending.All(p => p.X == cursorX);
    if (!sameRow && !sameColumn)
    {
      ShowError("Tiles must be placed in a single row or column.");
      return false;
    }

    return true;
  }

  private void Place(Tile tile, int rackIndex)
  {
    if (!CanPlaceAtCursor()) return;

    pending.Add(new Pending(cursorX, cursorY, tile, rackIndex));

    if (pending.Count >= 2)
      horizontal = pending[0].Y == pending[1].Y;

    // move on to the next free square in the typing direction
    int dx = horizontal ? 1 : 0;
    int dy = horizontal ? 0 : 1;
    int x = cursorX + dx, y = cursorY + dy;
    while (x <= Board14 && y <= Board14 && !IsFree(x, y)) (x, y) = (x + dx, y + dy);
    if (x <= Board14 && y <= Board14) (cursorX, cursorY) = (x, y);
  }

  private ExchangeTiles? PromptExchange()
  {
    pending.Clear();
    exchangeSelection = [];

    try
    {
      while (true)
      {
        Render();
        message = null;

        var key = Console.ReadKey(true);

        if (key.Key == ConsoleKey.Escape) return null;

        if (key.Key == ConsoleKey.Enter)
        {
          if (exchangeSelection.Count == 0)
            ShowError("Select at least one tile to exchange.");
          else
            return new ExchangeTiles(exchangeSelection.Order().Select(i => rack[i]).ToList());
        }
        else if (key.KeyChar is >= '1' and <= '9')
        {
          int index = key.KeyChar - '1';
          if (index < rack.Count && !exchangeSelection.Remove(index))
            exchangeSelection.Add(index);
        }
      }
    }
    finally
    {
      exchangeSelection = null;
    }
  }

  private bool Confirm(string question)
  {
    ShowMessage(question);
    Render();
    message = null;
    return Console.ReadKey(true).Key == ConsoleKey.Y;
  }

  // ---- rendering ----

  private void Render()
  {
    Console.ResetColor();
    Console.Clear();

    Console.WriteLine("SCRABBLE");
    Console.WriteLine();

    Console.Write("   ");
    for (int x = 0; x <= Board14; x++)
      Console.Write($" {(char)('A' + x)} ");
    Console.WriteLine();

    for (int y = 0; y <= Board14; y++)
    {
      Console.Write($"{y + 1,2} ");
      for (int x = 0; x <= Board14; x++)
        DrawSquare(x, y);
      Console.WriteLine();
    }

    Console.WriteLine();
    DrawScoreboard();
    Console.WriteLine();
    DrawRack();
    Console.WriteLine();

    if (message != null)
    {
      Console.ForegroundColor = messageIsError ? ConsoleColor.Red : ConsoleColor.Yellow;
      Console.WriteLine(message);
      Console.ResetColor();
    }
    else
      Console.WriteLine();

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine(exchangeSelection != null
      ? "1-7 = select tiles to exchange | Enter = exchange | Esc = cancel"
      : "Arrows = move | Letter = place | ? = blank | Tab = direction | Backspace = undo | Enter = play");
    if (exchangeSelection == null)
      Console.WriteLine("Ctrl+R = take back all | Ctrl+P = pass | Ctrl+E = exchange | Esc = quit");
    Console.ResetColor();
  }

  private void DrawSquare(int x, int y)
  {
    var square = map[y, x];
    var placed = pending.FirstOrDefault(p => p.X == x && p.Y == y);
    bool cursor = x == cursorX && y == cursorY && exchangeSelection == null;

    char? letter = placed?.Tile.Letter ?? square.Letter;
    string text;
    ConsoleColor background, foreground;

    if (letter != null)
    {
      text = $" {letter} ";
      bool blank = placed?.Tile.IsBlank ?? square.IsBlank;
      background = placed != null ? ConsoleColor.Yellow : ConsoleColor.Gray;
      foreground = blank ? ConsoleColor.DarkBlue : ConsoleColor.Black;
    }
    else
    {
      (text, background) = square switch
      {
        { IsStart: true } => (" ★ ", ConsoleColor.Magenta),
        { Score: ScoreType.TripleWord } => ("TW ", ConsoleColor.Red),
        { Score: ScoreType.DoubleWord } => ("DW ", ConsoleColor.Magenta),
        { Score: ScoreType.TripleLetter } => ("TL ", ConsoleColor.DarkBlue),
        { Score: ScoreType.DoubleLetter } => ("DL ", ConsoleColor.DarkCyan),
        _ => (" · ", ConsoleColor.DarkGreen)
      };
      foreground = ConsoleColor.White;
    }

    if (cursor)
    {
      text = $"[{(letter?.ToString() ?? (horizontal ? "→" : "↓"))}]";
      foreground = letter != null ? foreground : ConsoleColor.White;
    }

    Console.BackgroundColor = background;
    Console.ForegroundColor = foreground;
    Console.Write(text);
    Console.ResetColor();
  }

  private void DrawScoreboard()
  {
    foreach (var player in players)
    {
      bool isCurrent = player.Name == current?.Name;
      Console.ForegroundColor = isCurrent ? ConsoleColor.White : ConsoleColor.DarkGray;
      Console.Write($"{(isCurrent ? "▶" : " ")} {player.Name} {player.Score}   ");
    }
    Console.ResetColor();
    Console.WriteLine($"| Tiles in bag: {tilesInBag}");
  }

  private void DrawRack()
  {
    Console.Write("Rack: ");
    for (int i = 0; i < rack.Count; i++)
    {
      bool used = pending.Any(p => p.RackIndex == i);
      bool selected = exchangeSelection?.Contains(i) ?? false;

      Console.BackgroundColor = used ? ConsoleColor.DarkGray : selected ? ConsoleColor.Cyan : ConsoleColor.Gray;
      Console.ForegroundColor = ConsoleColor.Black;
      Console.Write($" {rack[i].Letter}{rack[i].Points,-2}".PadRight(5));
      Console.ResetColor();
      Console.Write(" ");
    }
    Console.WriteLine();

    if (exchangeSelection != null)
    {
      Console.Write("      ");
      for (int i = 0; i < rack.Count; i++)
        Console.Write($"  {i + 1}   ");
      Console.WriteLine();
    }
  }
}
