using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Battleship_APIs.Models;
using Battleship_APIs.Models.CustomClass;
using Newtonsoft.Json;

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

        [HttpGet("getGridByPlayerId/{id}/{gridSize}/{userGridTRUE_shotGridFALSE}")]
        public async Task<ActionResult<Grid>> GetGrid(byte id, byte gridSize, bool userGridTRUE_shotGridFALSE)
        {
            if (gridSize <= 30 && gridSize > 0)
            {
                CellMatrix matrix = new CellMatrix();
                Player player = await GetObjectPlayer(id);
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
                    matrix.Cells[cell.Xaxis -1, cell.Yaxis -1] = cell;
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

            List<Player> newPlayer = await _context.Players.ToListAsync();
            for (byte i = 0; i < (newGamePlayerList.Count); i++)
            {
                newPlayer[i].Name = newGamePlayerList[i].Name;
                newPlayer[i].Team = newGamePlayerList[i].Team;
                _context.SaveChanges();
            }

            return Ok("Successfully added");
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
            return Ok("Successfully placed ships");
        }

        // --------------  Game mechanics   -------------- //

        [HttpPost("postShot")]
        public async Task<ActionResult<Player>> Shot(byte id, byte xAxis, byte yAxis)
        {
            Player attackingPlayer = await GetObjectPlayer(id);
            int attackingTeam = attackingPlayer.Team;
            var ResponseMessage = "";
            if (xAxis < 31 && yAxis < 31 && xAxis > 0 && yAxis > 0)
            {
                List<Player> attackedPlayers = await _context.Players.Where(attackedPlayer => attackedPlayer.Team != attackingTeam && attackedPlayer.Name != null).ToListAsync();
                foreach (Player attackedPlayer in attackedPlayers)
                {
                    byte attackedGridId = attackedPlayer.UserGridId;
                    Cell attackedCell = _context.Cells.Where(aC => aC.GridId == attackedGridId && aC.Xaxis == xAxis && aC.Yaxis == yAxis).First();
                    if (attackedCell.State == 1)
                    {
                        Ship attackedShip = Hit(attackingPlayer, attackedCell, attackedPlayer);
                        ResponseMessage += $"{attackedPlayer.Name} - You hit a ship! Ship id:{attackedCell.ShipId} with {attackedShip.Hp} HP left; ";
                    }
                    else
                    {
                        Miss(attackingPlayer, attackedCell, attackedPlayer);
                        ResponseMessage += $"{attackedPlayer.Name} - You either missed or hit an already hit spot!;";
                    }
                }
                return Ok(ResponseMessage);
            }
            else return BadRequest("Invalid cell");
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

        //Metodo per prendere il giocatore dal db

        private async Task<Player> GetObjectPlayer(byte id)
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
