using Microsoft.AspNetCore.Mvc;
using ProgFinalPoe.Data;
using ProgFinalPoe.Services;

namespace ProgFinalPoe.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly SessionService _sessionService;

        public AuthController(AppDbContext context, SessionService sessionService)
        {
            _context = context;
            _sessionService = sessionService;
        }

        //Login authentication
        [HttpGet]
        public IActionResult Login()
        {
            if (_sessionService.IsUserLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password && u.IsActive);

            if (user != null)
            {
                _sessionService.SetUserSession(user);

                //Page allocation based on role
                return user.Role switch
                {
                    "Lecturer" => RedirectToAction("MakeAClaim", "Lecturer"),
                    "Coordinator" => RedirectToAction("ReviewClaims", "Admin"),
                    "Manager" => RedirectToAction("VerifyClaims", "Admin"),
                    "HR" => RedirectToAction("Index", "HR"),
                    _ => RedirectToAction("Index", "Home")
                };
            }

            TempData["ErrorMessage"] = "Invalid username or password";
            return View();
        }

        //method to log out
        [HttpPost]
        public IActionResult Logout()
        {
            _sessionService.ClearSession();
            return RedirectToAction("Login");
        }
    }
}