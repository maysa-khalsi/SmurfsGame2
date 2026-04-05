using Smurfs.SmurfsBL.entities;

namespace SmurfsGame.Engine;

public class GameState
{
    public Schtroumpf     Smurf   { get; set; } = null!;
    public List<Creature> Enemies { get; set; } = new();
    public List<Item>     Items   { get; set; } = new();
    public Forest         Forest  { get; set; } = null!;
    public Maze           Maze    { get; set; } = null!;

    public bool IsGameOver { get; set; }
    public bool IsGameWon  { get; set; }
    public int  Score      { get; set; }
}
