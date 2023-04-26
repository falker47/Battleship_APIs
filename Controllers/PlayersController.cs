using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Battleship_APIs.Models;
using Battleship_APIs.Models.CustomClass;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Cors;

namespace Battleship_APIs.Controllers
{
    [EnableCors("Policy")]
    [Route("api/[controller]")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        private readonly BattleshipDbContext _context;
        private PostResponse response = new PostResponse();

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

        [HttpGet("getGridByPlayerId/{id}/{gridSize}/{userGridTRUE_shotGridFALSE}")]
        public async Task<ActionResult<Grid>> GetGrid(byte id, byte gridSize, bool userGridTRUE_shotGridFALSE)
        {
            if (gridSize < 31 && gridSize > 0)
            {
                CellMatrix matrix = new CellMatrix();
                Player player = await GetDBPlayer(id);
                if (userGridTRUE_shotGridFALSE)
                {
                    matrix.GridId = player.UserGridId;
                }
                else if (!userGridTRUE_shotGridFALSE)
                {
                    matrix.GridId = player.ShotGridId;
                }
                matrix.Cells = new Cell[gridSize, gridSize];

                List<Cell> cells = await _context.Cells.Where(cell => cell.GridId == matrix.GridId && cell.Xaxis <= gridSize && cell.Yaxis <= gridSize).ToListAsync();
                cells.ForEach(cell =>
                {
                    matrix.Cells[cell.Xaxis - 1, cell.Yaxis - 1] = cell;
                });
                return Ok(JsonConvert.SerializeObject(matrix));
            }
            else
            {
                return NotFound();
            }
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

        [HttpPost("postCreateGame")]
        public async Task<ActionResult<Player>> PostPlayer(List<NewGamePlayer> newGamePlayerList)
        {
            await this.ResetGame();

            List<Player> newPlayers = await _context.Players.ToListAsync();
            for (byte i = 0; i < (newGamePlayerList.Count); i++)
            {
                newPlayers[i].Name = newGamePlayerList[i].Name;
                newPlayers[i].Team = newGamePlayerList[i].Team;
                _context.SaveChanges();
            }
            this.response.log = "Players successfully saved";
            return Ok(this.response);
        }

        [HttpPost("postPlaceShips")]
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
            this.response.log = "Ships successfully placed";
            return Ok(this.response);
        }

        [HttpPost("postShot")]
        public async Task<ActionResult<Player>> Shot(PlayerShot playerShot)
        {
            Player attackingPlayer = await GetDBPlayer(playerShot.id);
            int attackingTeam = attackingPlayer.Team;
            var responseMessage = "";
            if (playerShot.xAxis < 31 && playerShot.yAxis < 31 && playerShot.xAxis > 0 && playerShot.yAxis > 0)
            {
                List<Player> attackedPlayers = await _context.Players.Where(attackedPlayer => attackedPlayer.Team != attackingTeam && attackedPlayer.Name != null).ToListAsync();
                foreach (Player attackedPlayer in attackedPlayers)
                {
                    byte attackedGridId = attackedPlayer.UserGridId;
                    Cell attackedCell = _context.Cells.Where(aC => aC.GridId == attackedGridId && aC.Xaxis == playerShot.xAxis && aC.Yaxis == playerShot.yAxis).First();
                    if (attackedCell.State == 1)
                    {
                        Ship attackedShip = Hit(attackingPlayer, attackedCell, attackedPlayer);
                        responseMessage += $"{attackingPlayer.Name} - You hit a ship";
                        if (attackedShip.Hp == 0)
                        {
                            responseMessage += ", and it sinked!;";
                        }
                        else
                        {
                            responseMessage += "!;";
                        }
                    }
                    else
                    {
                        Miss(attackingPlayer, attackedCell, attackedPlayer);
                        responseMessage += $"{attackingPlayer.Name} - You either missed or hit an already hit spot!;";
                    }
                }
                this.response.log = responseMessage;
                return Ok(response);
            }
            else
            {
                this.response.log = "Invalid cell";
                return BadRequest(response);
            }
        }

        private void Miss(Player attackingPlayer, Cell attackedCell, Player attackedPlayer)
        {
            Grid gridToEdit = _context.Grids.Where(eG => eG.Id == attackingPlayer.ShotGridId).First();
            Cell cellToEdit = _context.Cells.Where(eC => eC.GridId == gridToEdit.Id && eC.Xaxis == attackedCell.Xaxis && eC.Yaxis == attackedCell.Yaxis).First();
            if (cellToEdit.State == 2)
            {
                return;
            }
            cellToEdit.State = 3;
            _context.SaveChanges();
        }

        private Ship Hit(Player attackingPlayer, Cell attackedCell, Player attackedPlayer)
        {
            //Attacker

            Grid gridToEdit = _context.Grids.Where(eG => eG.Id == attackingPlayer.ShotGridId).First();
            Cell cellToEdit = _context.Cells.Where(eC => eC.GridId == gridToEdit.Id && eC.Xaxis == attackedCell.Xaxis && eC.Yaxis == attackedCell.Yaxis).First();
            Ship attackedShip = _context.Ships.Where(aS => aS.Id == attackedCell.ShipId).First();
            attackedShip.Hp--;
            attackingPlayer.Points += 10;
            if (attackedShip.Hp == 0)
            {
                attackingPlayer.Points += 5;
            }
            cellToEdit.State = 2;

            //Attacked

            attackedCell.State = 2;
            _context.SaveChanges();
            return attackedShip;
        }

        private async Task<ActionResult> ResetGame()
        {
            await this.ResetPlayers();
            await this.ResetPlayersGrids();
            return Ok();
        }

        private async Task<ActionResult> ResetPlayers()
        {
            try
            {
                List<Player> players = await _context.Players.ToListAsync();
                players.ForEach(p =>
                {
                    p.Name = null;
                    p.Points = 0;
                });
                _context.SaveChanges();
            }
            catch
            {
                throw new ArgumentNullException("Players not found");
            }
            return Ok();
        }

        private async Task<ActionResult<Cell>> ResetPlayersGrids()
        {
            try
            {
                for (byte id = 0; id < 12; id++)
                {
                    List<Cell> cells = await _context.Cells.Where(cell => cell.GridId == id).ToListAsync();
                    cells.ForEach(cell => { cell.State = 0; cell.ShipId = null; });
                    _context.SaveChanges();
                }
            }
            catch
            {
                throw new ArgumentNullException("Cells not found");
            }
            return Ok();
        }

        private async Task<Player> GetDBPlayer(byte id)
        {
            try
            {
                if (_context.Players == null)
                {
                    throw new ArgumentNullException("Player not found");
                }
                var player = await _context.Players.FindAsync(id);
                if (player == null || player.Name == null)
                {
                    throw new ArgumentNullException("Player not found, or without a name");
                }
                return (player);
            }
            catch
            {
                throw;
            }
        }
    }
}
