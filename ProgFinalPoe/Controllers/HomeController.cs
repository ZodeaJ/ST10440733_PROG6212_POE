using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProgFinalPoe.Models;
using ProgFinalPoe.Services;

namespace ProgFinalPoe.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SessionService _sessionService;

        public HomeController(ILogger<HomeController> logger, SessionService sessionService)
        {
            _logger = logger;
            _sessionService = sessionService;
        }

        public IActionResult Index()
        {
            //If user is logged in they are redirected to their dashboard
            if (_sessionService.IsUserLoggedIn())
            {
                var role = _sessionService.GetUserRole();
                return role switch
                {
                    "Lecturer" => RedirectToAction("MakeAClaim", "Lecturer"),
                    "Coordinator" => RedirectToAction("ReviewClaims", "Admin"),
                    "Manager" => RedirectToAction("VerifyClaims", "Admin"),
                    "HR" => RedirectToAction("Index", "HR"),
                    _ => RedirectToAction("Login", "Auth")
                };
            }

            //If not logged in home page is shown with login option
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