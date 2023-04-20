using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Battleship_APIs.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
            var players = await _context.Players.ToListAsync();
            var verifiedPlayers = new List<Player>();

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].Name == null)
                {
                    return BadRequest("Cannot continue with empty name");
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
            var player = await _context.Players.FindAsync(id);

            if (player == null || player.Name == null)
            {
                return NotFound();
            }
            return Ok(player);
        }

        [HttpPost("createGame")]
        public async Task<ActionResult<Player>> PostPlayer(List<NewGamePlayer> newGamePlayerList)
        {
            await this.resetGame();

            var newPlayer = await _context.Players.ToListAsync();
            for (byte i = 0; i < (newGamePlayerList.Count); i++)
            {
                newPlayer[i].Name = newGamePlayerList[i].Name;
                newPlayer[i].Team = newGamePlayerList[i].Team;
                _context.SaveChanges();
            }

            return Ok("Successfully added");
        }

        // --------------  Game mechanics   -------------- //

        [HttpGet("shot/{id}/{xAxis}/{yAxis}")]
        public async Task<ActionResult<Player>> Shot(byte id, byte xAxis, byte yAxis)
        {
            Player attackingPlayer = await GetObjectPlayer(id);

            int attackingTeam = attackingPlayer.Team;

            var ResponseMessage = "";

            if (attackingPlayer != null && xAxis < 31 && yAxis < 31)
            {
                List<Player> attackedPlayers = await _context.Players.Where(attackedPlayer => attackedPlayer.Team != attackingTeam && attackedPlayer.Name != null).ToListAsync();

                foreach (Player attackedPlayer in attackedPlayers)
                {
                    byte attackedGridId = attackedPlayer.UserGridId;

                    Cell attackedCell = _context.Cells.Where(aC => aC.GridId == attackedGridId && aC.Xaxis == xAxis && aC.Yaxis == yAxis).First();

                    if (attackedCell != null && attackedCell.ShipId != null)
                    {
                        //HIT!

                        Ship attackedShip = Hit(attackingPlayer, attackedCell, attackedPlayer);

                        ResponseMessage += $"The ship of {attackedPlayer.Name} has been hit! Ship id:{attackedCell.ShipId} with {attackedShip.Hp} HP left; "; m
                    }

                    //MISS!
                    else
                    Miss(attackingPlayer, attackedCell, attackedPlayer);
                    ResponseMessage += $"The ship of {attackedPlayer.Name} has been missed!;";
                    
                }

            } 
            
            return Ok(ResponseMessage);

        }

        private void Miss(Player attackingPlayer, Cell attackedCell, Player attackedPlayer)
        {
            //Attacker

            Grid gridToEdit = _context.Grids.Where(eG => eG.Id == attackingPlayer.ShotGridId).First();

            Cell cellToEdit = _context.Cells.Where(eC => eC.GridId == gridToEdit.Id && eC.Xaxis == attackedCell.Xaxis && eC.Yaxis == attackedCell.Yaxis).First();

            cellToEdit.State = 1;

            _context.SaveChanges();

        }

        private Ship Hit(Player attackingPlayer, Cell attackedCell, Player attackedPlayer)
        {
            //Attacker

            Grid gridToEdit = _context.Grids.Where(eG => eG.Id == attackingPlayer.ShotGridId).First();

            Cell cellToEdit = _context.Cells.Where(eC => eC.GridId == gridToEdit.Id && eC.Xaxis == attackedCell.Xaxis && eC.Yaxis == attackedCell.Yaxis).First();

            cellToEdit.State = 3;

            //Attacked

            Ship attackedShip = _context.Ships.Where(aS => aS.Id == attackedCell.ShipId).First();

            attackedShip.Hp--;

            attackedCell.State = 3;

            _context.SaveChanges();

            return attackedShip;

        }




        private async Task<ActionResult> resetGame()
        {
            await this.resetPlayers();
            await this.resetPlayersGrids();
            return Ok();
        }

        private async Task<ActionResult> resetPlayers()
        {
            var players = await _context.Players.ToListAsync();
            players.ForEach(p =>
            {
                p.Name = null;
            });
            _context.SaveChanges();
            return Ok();
        }

        private async Task<ActionResult<Cell>> resetPlayersGrids()
        {
            for (byte id = 0; id < 12; id++)
            {
                var cells = await _context.Cells.Where(cell => cell.GridId == id).ToListAsync();
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
