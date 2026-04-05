using System.ComponentModel.DataAnnotations;

namespace Smurfs.SmurfsBL.entities;

public class Forest
{
    [Key]
    public int Idf  { get; set; }
    public int MinX { get; set; }
    public int MaxX { get; set; }
    public int MinY { get; set; }
    public int MaxY { get; set; }

    public virtual List<Creature>? Creatures { get; set; }
    public virtual List<Item>?     Items     { get; set; }
}
