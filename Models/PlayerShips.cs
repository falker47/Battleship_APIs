
namespace Battleship_APIs.Models
{
    public class PlayerShips
    {
        public byte PlayerId { get; set; }
        public List<List<Position>> PlayerShipsPosition { get; set; }
    }
}
