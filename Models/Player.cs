using System;
using System.Collections.Generic;

namespace Battleship_APIs.Models;

public partial class Player
{
    public byte Id { get; set; }

    public string? Name { get; set; }

    public byte UserGridId { get; set; }

    public byte ShotGridId { get; set; }

    public byte Team { get; set; }

    public short Points { get; set; }

    public virtual ICollection<Grid> Grids { get; set; } = new List<Grid>();

    public virtual ICollection<Ship> Ships { get; set; } = new List<Ship>();

    public virtual Grid ShotGrid { get; set; } = null!;

    public virtual Grid UserGrid { get; set; } = null!;
}
