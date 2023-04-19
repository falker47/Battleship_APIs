using System;
using System.Collections.Generic;

namespace Battleship_APIs.Models;

public partial class Ship
{
    public byte Id { get; set; }

    public byte PlayerId { get; set; }

    public byte Length { get; set; }

    public byte Hp { get; set; }

    public virtual Player Player { get; set; } = null!;
}
