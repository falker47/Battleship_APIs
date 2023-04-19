using System;
using System.Collections.Generic;

namespace Battleship_APIs.Models;

public partial class Cell
{
    public Guid Id { get; set; }

    public byte GridId { get; set; }

    public byte Xaxis { get; set; }

    public byte Yaxis { get; set; }

    public byte State { get; set; }

    public virtual Grid Grid { get; set; } = null!;
}
