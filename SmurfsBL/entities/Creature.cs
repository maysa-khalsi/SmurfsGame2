using System.ComponentModel.DataAnnotations;

namespace Smurfs.SmurfsBL.entities;

public abstract class Creature
{
    [Key]
    public int     Idc       { get; set; }
    public string  Name      { get; set; } = "";
    public int     Health    { get; set; }
    public int     PositionX { get; set; }
    public int     PositionY { get; set; }
    public Forest? Forest    { get; set; }
}
