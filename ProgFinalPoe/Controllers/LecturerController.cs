using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgFinalPoe.Data;
using ProgFinalPoe.Models;
using ProgFinalPoe.Services;

namespace ProgFinalPoe.Controllers
{
    public class LecturerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFileStorage _fileStorage;
        private readonly SessionService _sessionService;

        public LecturerController(AppDbContext context, IFileStorage fileStorage, SessionService sessionService)
        {
            _context = context;
            _fileStorage = fileStorage;
            _sessionService = sessionService;
        }

        private bool IsLecturer()
        {
            return _sessionService.IsUserLoggedIn() && _sessionService.GetUserRole() == "Lecturer";
        }

        private bool IsLecturerOrHR()
        {
            return _sessionService.IsUserLoggedIn() &&
                  (_sessionService.GetUserRole() == "Lecturer" || _sessionService.GetUserRole() == "HR");
        }

        private Lecturer GetLoggedInLecturer()
        {
            if (!IsLecturerOrHR()) return null;

            var user = _context.Users
                .FirstOrDefault(u => u.UserId == _sessionService.GetUserId());

            if (user == null) return null;

            //Check if a Lecturer record exists for this user by email/name
            var lecturer = _context.Lecturers
                .FirstOrDefault(l => l.Email == user.Email || l.Name == $"{user.Name} {user.Surname}");

            if (lecturer == null)
            {
                //Creates a new Lecturer record
                lecturer = new Lecturer
                {
                    Name = $"{user.Name} {user.Surname}",
                    Email = user.Email,
                    Department = user.Department,
                    PhoneNumber = "Not specified"
                };
                _context.Lecturers.Add(lecturer);
                _context.SaveChanges();
            }

            return lecturer;
        }

        //lecturer logs in and gets make a claim view
        [HttpGet]
        public IActionResult MakeAClaim()
        {
            if (!IsLecturerOrHR()) return RedirectToAction("Login", "Auth");

            var lecturer = GetLoggedInLecturer();
            if (lecturer == null) return RedirectToAction("Login", "Auth");

            ViewBag.LecturerName = lecturer.Name;
            ViewBag.HourlyRate = 250;

            return View();
        }

        //Method for lecturer to make a claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeAClaim(Claim claim, IFormFile SupportingDocument)
        {
            if (!IsLecturer())
            {
                return RedirectToAction("Login", "Auth");
            }

            var lecturer = GetLoggedInLecturer();
            if (lecturer == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            //Set the lecturer ID
            claim.LecturerId = lecturer.LecturerId;
            claim.HourlyRate = 250;
            claim.InvoiceNumber = "";

            //Validation
            if (claim.HoursWorked <= 0)
            {
                ModelState.AddModelError("HoursWorked", "Hours worked must be greater than 0.");
            }

            if (string.IsNullOrEmpty(claim.Month))
            {
                ModelState.AddModelError("Month", "Month is required.");
            }

            if (string.IsNullOrEmpty(claim.Description))
            {
                ModelState.AddModelError("Description", "Description is required.");
            }

            if (SupportingDocument == null || SupportingDocument.Length == 0)
            {
                ModelState.AddModelError("SupportingDocument", "Supporting document is required.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.LecturerName = lecturer.Name;
                ViewBag.HourlyRate = 250;
                return View(claim);
            }

            try
            {
                string savedFileName = await _fileStorage.SaveFile(SupportingDocument);

                //Creates a new claim object
                var newClaim = new Claim
                {
                    LecturerId = lecturer.LecturerId,
                    Month = claim.Month,
                    HoursWorked = claim.HoursWorked,
                    HourlyRate = 250,
                    Description = claim.Description,
                    SupportingDocument = savedFileName,
                    Status = ClaimStatus.Submitted,
                    CreatedAt = DateTime.UtcNow,
                    InvoiceNumber = ""
                };
                _context.Claims.Add(newClaim);
                await _context.SaveChangesAsync();

                //Success message when claim is submitted successfully
                TempData["SuccessMessage"] = $"Claim submitted successfully! Your claim ID is {newClaim.ClaimId}. It is now awaiting coordinator review.";
                return RedirectToAction(nameof(TrackClaimStatus));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving claim: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                TempData["ErrorMessage"] = $"Error submitting claim: {ex.Message}";
                ViewBag.LecturerName = lecturer.Name;
                ViewBag.HourlyRate = 250;
                return View(claim);
            }
        }

        //Auto calculation for total amount
        [HttpPost]
        public IActionResult CalculateAmount(int hoursWorked)
        {
            if (!IsLecturerOrHR()) return Json(new { error = "Not authorized" });

            var lecturer = GetLoggedInLecturer();
            if (lecturer == null) return Json(new { error = "Lecturer not found" });

            var hourlyRate = 250;
            var amount = hoursWorked * hourlyRate;

            return Json(new
            {
                success = true,
                amount = amount.ToString("F2"),
                hourlyRate = hourlyRate,
                validation = hoursWorked <= 180 ? "valid" : "exceeds_limit"
            });
        }

        //Lecturer tracks the claims status as it goes through the process
        public async Task<IActionResult> TrackClaimStatus()
        {
            if (!IsLecturerOrHR()) return RedirectToAction("Login", "Auth");

            var lecturer = GetLoggedInLecturer();
            if (lecturer == null) return RedirectToAction("Login", "Auth");

            var claims = await _context.Claims
                .Include(c => c.FeedbackMessages)
                .Where(c => c.LecturerId == lecturer.LecturerId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View("~/Views/Claim/TrackClaimStatus.cshtml", claims);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClaim(int claimId)
        {
            if (!IsLecturer()) return RedirectToAction("Login", "Auth");

            var lecturer = GetLoggedInLecturer();
            if (lecturer == null) return RedirectToAction("Login", "Auth");

            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null) return NotFound();

            //Check if claim belongs to logged in lecturer
            if (claim.LecturerId != lecturer.LecturerId)
            {
                TempData["ErrorMessage"] = "You can only delete your own claims.";
                return RedirectToAction(nameof(TrackClaimStatus));
            }

            //Allow deletion if the claim is rejected or approved for space
            if (claim.Status != ClaimStatus.Rejected && claim.Status != ClaimStatus.Approved)
            {
                TempData["ErrorMessage"] = "Only rejected or approved claims can be deleted.";
                return RedirectToAction(nameof(TrackClaimStatus));
            }

            try
            {
                if (!string.IsNullOrEmpty(claim.SupportingDocument))
                {
                    await _fileStorage.DeleteFile(claim.SupportingDocument);
                }

                _context.Claims.Remove(claim);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Claim deleted successfully!";
                return RedirectToAction(nameof(TrackClaimStatus));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting claim: " + ex.Message;
                return RedirectToAction(nameof(TrackClaimStatus));
            }
        }

        public async Task<IActionResult> ClaimFeedback(int claimId)
        {
            if (!IsLecturerOrHR()) return RedirectToAction("Login", "Auth");

            //Get the claim with all related data
            var claim = await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.FeedbackMessages)
                .FirstOrDefaultAsync(c => c.ClaimId == claimId);

            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim not found!";
                return RedirectToAction("TrackLecturerClaims", "HR");
            }

            return View(claim);
        }

        //Method calculating the total
        [HttpPost]
        public IActionResult CalculateTotal(int hours, decimal rate)
        {
            if (!IsLecturerOrHR()) return Json(new { error = "Not authorized" });

            var total = hours * rate;
            return Json(new { total = total.ToString("C") });
        }
    }
}