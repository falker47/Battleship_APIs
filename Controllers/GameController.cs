using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Battleship_APIs.Models;
using Battleship_APIs.Controllers;

namespace Battleship_APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly BattleshipDbContext _context;

        public GameController(BattleshipDbContext context)
        {
            _context = context;
        }

        

        
    }
}
