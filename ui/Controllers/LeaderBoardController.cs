using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using ui.Services;

namespace ui.Controllers
{
    [Route("api/[controller]")]
    public class LeaderBoardController : Controller
    {
        private ContestDataService contestDataService;

        public LeaderBoardController()
        {
            contestDataService = new ContestDataService();
        }

        [HttpGet("")]
        public async Task<IEnumerable<LeaderboardRecord>> Index()
        {
            return await contestDataService.GetLeaderBoard();
        }
    }
}