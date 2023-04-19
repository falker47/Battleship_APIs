using System;
using System.Collections.Generic;

namespace Battleship_APIs.Models;

public partial class ShipCell
{
    public byte ShipId { get; set; }

    public Guid CellId { get; set; }

    public virtual Cell Cell { get; set; } = null!;

    public virtual Ship Ship { get; set; } = null!;
}
