using System;
using System.Collections.Generic;

namespace Battleship_APIs.Models;

public partial class Grid
{
    public byte Id { get; set; }

    public byte? PlayerId { get; set; }

    public virtual ICollection<Cell> Cells { get; set; } = new List<Cell>();

    public virtual Player? Player { get; set; }

    public virtual ICollection<Player> PlayerShotGrids { get; set; } = new List<Player>();

    public virtual ICollection<Player> PlayerUserGrids { get; set; } = new List<Player>();
}
