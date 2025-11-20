using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgFinalPoe.Data;
using ProgFinalPoe.Models;
using ProgFinalPoe.Services;

namespace ProgFinalPoe.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly SessionService _sessionService;

        public AdminController(AppDbContext context, SessionService sessionService)
        {
            _context = context;
            _sessionService = sessionService;
        }

        //Authentication methods
        private bool IsCoordinator()
        {
            return _sessionService.IsUserLoggedIn() && _sessionService.GetUserRole() == "Coordinator";
        }

        private bool IsManager()
        {
            return _sessionService.IsUserLoggedIn() && _sessionService.GetUserRole() == "Manager";
        }

        private bool IsCoordinatorOrHR()
        {
            return _sessionService.IsUserLoggedIn() &&
                  (_sessionService.GetUserRole() == "Coordinator" || _sessionService.GetUserRole() == "HR");
        }

        private bool IsManagerOrHR()
        {
            return _sessionService.IsUserLoggedIn() &&
                  (_sessionService.GetUserRole() == "Manager" || _sessionService.GetUserRole() == "HR");
        }

        public async Task<IActionResult> ReviewClaims()
        {
            if (!IsCoordinatorOrHR()) return RedirectToAction("Login", "Auth");

            var claims = await _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.Status == ClaimStatus.Submitted)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return View(claims);
        }

        //Coordinator forwards to manager
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForwardClaim(int id, string coordinatorMessage)
        {
            if (!IsCoordinator()) return RedirectToAction("Login", "Auth");

            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.Status = ClaimStatus.Forwarded;

            _context.Feedbacks.Add(new Feedback
            {
                ClaimId = id,
                Role = "Coordinator",
                Message = string.IsNullOrWhiteSpace(coordinatorMessage) ? "Forwarded to manager for final approval" : coordinatorMessage,
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Claim forwarded to manager successfully!";
            return RedirectToAction(nameof(ReviewClaims));
        }

        //Coordinator rejects claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectByCoordinator(int id, string coordinatorMessage)
        {
            if (!IsCoordinator()) return RedirectToAction("Login", "Auth");

            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.Status = ClaimStatus.Rejected;

            _context.Feedbacks.Add(new Feedback
            {
                ClaimId = id,
                Role = "Coordinator",
                Message = string.IsNullOrWhiteSpace(coordinatorMessage) ? "Rejected by coordinator" : coordinatorMessage,
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Claim rejected successfully!";
            return RedirectToAction(nameof(ReviewClaims));
        }

        //Manager sees only forwarded claims
        public async Task<IActionResult> VerifyClaims()
        {
            if (!IsManagerOrHR()) return RedirectToAction("Login", "Auth");

            var claims = await _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.Status == ClaimStatus.Forwarded)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return View(claims);
        }

        //Manager approves claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveClaim(int id, string managerMessage)
        {
            if (!IsManager()) return RedirectToAction("Login", "Auth");

            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.Status = ClaimStatus.Approved;
            claim.ApprovedAt = DateTime.UtcNow;

            _context.Feedbacks.Add(new Feedback
            {
                ClaimId = id,
                Role = "Manager",
                Message = string.IsNullOrWhiteSpace(managerMessage) ? "Approved by manager" : managerMessage,
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Claim approved successfully! It is now ready for HR invoicing.";
            return RedirectToAction(nameof(VerifyClaims));
        }

        //Manager rejects claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectByManager(int id, string managerMessage)
        {
            if (!IsManager()) return RedirectToAction("Login", "Auth");

            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.Status = ClaimStatus.Rejected;

            _context.Feedbacks.Add(new Feedback
            {
                ClaimId = id,
                Role = "Manager",
                Message = string.IsNullOrWhiteSpace(managerMessage) ? "Rejected by manager" : managerMessage,
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Claim rejected successfully!";
            return RedirectToAction(nameof(VerifyClaims));
        }
    }
}