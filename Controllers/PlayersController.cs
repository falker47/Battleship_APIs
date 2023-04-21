using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Battleship_APIs.Models;

namespace Battleship_APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        private readonly BattleshipDbContext _context;

        public PlayersController(BattleshipDbContext context)
        {
            _context = context;
        }

        [HttpGet("getPlayers")]
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
        {
            if (_context.Players == null)
            {
                return NotFound();
            }
            List<Player> players = await _context.Players.ToListAsync();
            List<Player> verifiedPlayers = new List<Player>();

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].Name == null)
                {
                    break;
                }
                verifiedPlayers.Add(players[i]);
            }
            return Ok(verifiedPlayers);
        }

        [HttpGet("getPlayer/{id}")]
        public async Task<ActionResult<Player>> GetPlayer(byte id)
        {
            if (_context.Players == null)
            {
                return NotFound();
            }
            Player player = await _context.Players.FindAsync(id);

            if (player == null || player.Name == null)
            {
                return NotFound();
            }
            return Ok(player);
        }

        [HttpGet("getShipsByPlayerId/{id}")]
        public async Task<ActionResult<Ship>> GetShipsByPlayerId(byte id)
        {
            if (_context.Ships == null)
            {
                return NotFound();
            }
            List<Ship> ships = await _context.Ships.Where(ship => ship.PlayerId == id).ToListAsync();
            return Ok(ships);
        }

        [HttpPost("createGame")]
        public async Task<ActionResult<Player>> PostPlayer(List<NewGamePlayer> newGamePlayerList)
        {
            await this.ResetGame();

            List<Player> newPlayer = await _context.Players.ToListAsync();
            for (byte i = 0; i < (newGamePlayerList.Count); i++)
            {                
                newPlayer[i].Name = newGamePlayerList[i].Name;
                newPlayer[i].Team = newGamePlayerList[i].Team;
                _context.SaveChanges();               
            }

            return Ok("Successfully added");
        }

        [HttpPost("placeShips")]
        public async Task<ActionResult<Ship>> PlaceShip(PlayerShips playerShips)
        {
            Player player = await _context.Players.Where(player => player.Id == playerShips.PlayerId).FirstAsync();
            List<Cell> playerGridCells = await _context.Cells.Where(cell => cell.GridId == player.UserGridId).ToListAsync();
            List<Ship> ships = await _context.Ships.Where(ship => ship.PlayerId == playerShips.PlayerId).ToListAsync();
            int j = 0;
            playerShips.PlayerShipsPosition.ForEach(playerShip =>
            {
                for (int i = 0; i < playerShip.Count(); i++)
                {
                    Cell positionCell = playerGridCells.Where(cell => cell.Xaxis == playerShip[i].X && cell.Yaxis == playerShip[i].Y).First();
                    positionCell.State = 1;
                    positionCell.ShipId = ships[j].Id;
                }
                ships[j].Length = (byte)playerShip.Count();
                ships[j].Hp = (byte)playerShip.Count();
                j++;
            });
            _context.SaveChanges();
            return Ok("Successfully placed ships");
        }

        private async Task<ActionResult> ResetGame()
        {
            await this.ResetPlayers();
            await this.ResetPlayersGrids();
            return Ok();
        }

        private async Task<ActionResult> ResetPlayers()
        {
            List<Player> players = await _context.Players.ToListAsync();
            players.ForEach(p =>
            {
                p.Name = null;
            });
            _context.SaveChanges();
            return Ok();
        }

        private async Task<ActionResult<Cell>> ResetPlayersGrids()
        {     
            for (byte id = 0; id < 12; id++)
            {
            List<Cell> cells = await _context.Cells.Where(cell => cell.GridId == id).ToListAsync();
            cells.ForEach(cell => { cell.State = 0; cell.ShipId = null; });
            _context.SaveChanges();
            }
            return Ok();
        }
    }
}
