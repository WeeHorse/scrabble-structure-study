namespace PlayerSystem;

class Player
{
  public string Name { get; }
  public PlayerRack Rack { get; } = new();
  public int Score { get; set; }

  public Player(string name)
  {
    Name = name;
  }
}
