namespace Battleship_APIs.Models.CustomClass
{
    public class PlayerShips
    {
        public byte PlayerId { get; set; }
        public List<List<Position>> PlayerShipsPosition { get; set; }
    }
}
