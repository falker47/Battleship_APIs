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
            var players = await _context.Players.ToListAsync();
            var verifiedPlayers = new List<Player>();

            for (int i = 0; i< players.Count; i++)
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
            var player = await _context.Players.FindAsync(id);

            if (player == null || player.Name == null)
            {
                return NotFound();
            }
            return Ok(player);
        }

        //// POST: api/Players
        //[HttpPost("setPlayers")]
        //public async Task<ActionResult<Player>> PostPlayer(Player player)
        //{
        //  if (_context.Players == null)
        //  {
        //      return Problem("Entity set 'BattleshipDbContext.Players'  is null.");
        //  }
        //    _context.Players.Add(player);
        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateException)
        //    {
        //        if (PlayerExists(player.Id))
        //        {
        //            return Conflict();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return CreatedAtAction("GetPlayer", new { id = player.Id }, player);
        //}

        private bool PlayerExists(byte id)
        {
            return (_context.Players?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
