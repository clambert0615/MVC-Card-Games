using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Cardgames.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Cardgames.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly GamesContext _context;
       
        public HomeController(ILogger<HomeController> logger, GamesContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public IActionResult Statistics()
        {

            string id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var stats = _context.GoFish.Where(x => x.UserId == id).ToList();
            foreach (var s in stats)
            {
                ViewBag.Wins = (ViewBag.Wins ?? 0) + (s.Wins ?? 0);
                ViewBag.Losses = (ViewBag.Losses ?? 0) + (s.Losses ?? 0);
                ViewBag.Ties = (ViewBag.Ties ?? 0) + (s.Ties ?? 0);
            }

            var warstats = _context.War.Where(x => x.UserId == id).ToList();
            foreach( var s in warstats)
            {
                ViewBag.WarWins = (ViewBag.WarWins ?? 0) + (s.Wins ?? 0);
                ViewBag.WarLosses = (ViewBag.WarLosses ?? 0) + (s.Losses ?? 0);
                
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
