using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgFinalPoe.Data;
using ProgFinalPoe.Models;
using ProgFinalPoe.Services;

namespace ProgFinalPoe.Controllers
{
    public class HRController : Controller
    {
        private readonly AppDbContext _context;
        private readonly SessionService _sessionService;

        public HRController(AppDbContext context, SessionService sessionService)
        {
            _context = context;
            _sessionService = sessionService;
        }

        //Check if user is HR
        private bool IsHR()
        {
            return _sessionService.IsUserLoggedIn() && _sessionService.GetUserRole() == "HR";
        }

        public async Task<IActionResult> Index()
        {
            if (!IsHR()) return RedirectToAction("Login", "Auth");

            var pendingInvoices = await _context.Claims
                .CountAsync(c => c.Status == ClaimStatus.Approved && string.IsNullOrEmpty(c.InvoiceNumber));

            var totalInvoices = await _context.Invoices.CountAsync();
            var pendingPayments = await _context.Invoices.CountAsync(i => !i.IsPaid);
            var totalLecturers = await _context.Lecturers.CountAsync();
            var totalUsers = await _context.Users.CountAsync();

            //Get recent approved claims for the activity section
            var recentClaims = await _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.Status == ClaimStatus.Approved)
                .OrderByDescending(c => c.ApprovedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.PendingInvoicesCount = pendingInvoices;
            ViewBag.TotalInvoicesCount = totalInvoices;
            ViewBag.PendingPaymentsCount = pendingPayments;
            ViewBag.TotalLecturersCount = totalLecturers;
            ViewBag.TotalUsersCount = totalUsers;
            ViewBag.RecentClaims = recentClaims;

            return View();
        }

        //user management by HR
        public IActionResult ManageUsers()
        {
            if (!IsHR()) return RedirectToAction("Login", "Auth");

            var users = _context.Users
                .Include(u => u.Lecturer)
                .Where(u => u.IsActive)
                .ToList();
            return View(users);
        }

        //Create User By HR
        [HttpGet]
        public IActionResult CreateUser()
        {
            if (!IsHR()) return RedirectToAction("Login", "Auth");

            ViewBag.Lecturers = _context.Users
            .Where(u => u.Role == "Lecturer" && u.IsActive)
            .ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(User user)
        {
            if (!IsHR()) return RedirectToAction("Login", "Auth");

            //Check if username already exists
            if (_context.Users.Any(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Username", "Username already exists");
                ViewBag.Lecturers = _context.Users
                    .Where(u => u.Role == "Lecturer" && u.IsActive)
                    .ToList();
                return View(user);
            }

            user.CreatedAt = DateTime.Now;
            user.IsActive = true;

            //Set hourly rate
            user.HourlyRate = user.Role == "Lecturer" ? 250 : 0;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    //If user is a lecturer, create a corresponding Lecturer record
                    if (user.Role == "Lecturer")
                    {
                        var lecturer = new Lecturer
                        {
                            Name = $"{user.Name} {user.Surname}",
                            Email = user.Email,
                            Department = user.Department,
                            PhoneNumber = "Not specified"
                        };
                        _context.Lecturers.Add(lecturer);
                        await _context.SaveChangesAsync();

                        //Update the user with the LecturerId
                        user.LecturerId = lecturer.LecturerId;
                        await _context.SaveChangesAsync();
                    }

                    TempData["Success"] = "User created successfully!";
                    return RedirectToAction("ManageUsers");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating user: {ex.Message}");
                }
            }

            ViewBag.Lecturers = _context.Users
                .Where(u => u.Role == "Lecturer" && u.IsActive)
                .ToList();
            return View(user);
        }

        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        //Edit user information/profile
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            if (!IsHR()) return RedirectToAction("Login", "Auth");

            var user = _context.Users
                .Include(u => u.Lecturer)
                .FirstOrDefault(u => u.UserId == id);
            if (user == null) return NotFound();

            ViewBag.Lecturers = _context.Lecturers.ToList();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(User updatedUser)
        {
            if (!IsHR()) return RedirectToAction("Login", "Auth");

            var user = await _context.Users.FindAsync(updatedUser.UserId);
            if (user == null) return NotFound();

            user.Name = updatedUser.Name;
            user.Surname = updatedUser.Surname;
            user.Email = updatedUser.Email;
            user.Role = updatedUser.Role;
            user.LecturerId = updatedUser.LecturerId;

            await _context.SaveChangesAsync();
            TempData["Success"] = "User updated successfully";
            return RedirectToAction("ManageUsers");
        }

        //View approved claims that need invoices
        public async Task<IActionResult> ApprovedClaims()
        {
            if (!IsHR()) return RedirectToAction("Login", "Auth");

            var claims = await _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.Status == ClaimStatus.Approved && string.IsNullOrEmpty(c.InvoiceNumber))
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return View(claims);
        }

        //Invoice creation
        public async Task<IActionResult> CreateInvoice(int id)
        {
            if (!IsHR()) return RedirectToAction("Login", "Auth");

            var claim = await _context.Claims
                .Include(c => c.Lecturer)
                .FirstOrDefaultAsync(c => c.ClaimId == id);

            if (claim == null)
            {
                TempData["Error"] = "Claim not found!";
                return RedirectToAction("ApprovedClaims");
            }

            //Invoice number creation
            var invoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{claim.ClaimId}";

            claim.InvoiceNumber = invoiceNumber;

            //Invoice record
            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                ClaimId = claim.ClaimId,
                LecturerId = (int)claim.LecturerId,
                Amount = claim.Amount,
                GeneratedDate = DateTime.Now
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Invoice {invoiceNumber} created successfully!";
            return RedirectToAction("ApprovedClaims");
        }

        //View all invoices
        public async Task<IActionResult> Invoices()
        {
            if (!IsHR()) return RedirectToAction("Login", "Auth");

            var invoices = await _context.Invoices
                .Include(i => i.Claim)
                .Include(i => i.Lecturer)
                .OrderByDescending(i => i.GeneratedDate)
                .ToListAsync();

            return View(invoices);
        }

        //State invoice as paid
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            if (!IsHR()) return RedirectToAction("Login", "Auth");

            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice != null)
            {
                invoice.IsPaid = true;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Invoice marked as paid!";
            }
            else
            {
                TempData["Error"] = "Invoice not found!";
            }

            return RedirectToAction("Invoices");
        }

        //Delete invoice option added for space
        [HttpPost]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            if (!IsHR()) return RedirectToAction("Login", "Auth");

            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                TempData["Error"] = "Invoice not found!";
                return RedirectToAction("Invoices");
            }

            try
            {
                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Invoice deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting invoice: " + ex.Message;
            }

            return RedirectToAction("Invoices");
        }

        //Delete User method
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!IsHR()) return RedirectToAction("Login", "Auth");

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found!";
                return RedirectToAction("ManageUsers");
            }

            if (user.UserId == _sessionService.GetUserId())
            {
                TempData["Error"] = "You cannot delete your own account!";
                return RedirectToAction("ManageUsers");
            }

            try
            {
                user.IsActive = false;
                await _context.SaveChangesAsync();

                TempData["Success"] = "User deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting user: " + ex.Message;
            }

            return RedirectToAction("ManageUsers");
        }

        // Method to get all lecturers for dropdown
        [HttpGet]
        public async Task<IActionResult> GetLecturers()
        {
            if (!IsHR()) return Json(new { error = "Not authorized" });

            var lecturers = await _context.Users
                .Where(u => u.Role == "Lecturer" && u.IsActive)
                .Select(u => new { u.UserId, u.Name, u.Surname })
                .ToListAsync();

            return Json(lecturers);
        }

        public async Task<IActionResult> TrackLecturerClaims(int? lecturerId = null)
        {
            if (!IsHR()) return RedirectToAction("Login", "Auth");

            //Get all lecturers for dropdown
            var lecturers = await _context.Users
                .Where(u => u.Role == "Lecturer" && u.IsActive)
                .OrderBy(u => u.Name)
                .ToListAsync();

            ViewBag.Lecturers = lecturers;

            //If no lecturer selected show empty results or first lecturer
            if (!lecturerId.HasValue && lecturers.Any())
            {
                lecturerId = lecturers.First().UserId;
            }

            List<Claim> claims = new List<Claim>();

            if (lecturerId.HasValue)
            {
                var selectedLecturerUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == lecturerId && u.Role == "Lecturer");

                if (selectedLecturerUser != null)
                {
                    ViewBag.SelectedLecturer = selectedLecturerUser;

                    //Checks if lecturer has a LecturerId
                    Console.WriteLine($"Selected Lecturer UserId: {selectedLecturerUser.UserId}");
                    Console.WriteLine($"Selected Lecturer LecturerId: {selectedLecturerUser.LecturerId}");

                    //Get claims for the selected lecturer
                    var lecturerRecord = await _context.Lecturers
                        .FirstOrDefaultAsync(l => l.LecturerId == selectedLecturerUser.LecturerId);

                    if (lecturerRecord != null)
                    {
                        Console.WriteLine($"Found lecturer record with ID: {lecturerRecord.LecturerId}");

                        //Get claims using the LecturerId from the Lecturer table
                        claims = await _context.Claims
                            .Include(c => c.FeedbackMessages)
                            .Include(c => c.Lecturer)
                            .Where(c => c.LecturerId == lecturerRecord.LecturerId)
                            .OrderByDescending(c => c.CreatedAt)
                            .ToListAsync();

                        Console.WriteLine($"Found {claims.Count} claims for lecturer");
                    }
                    else
                    {
                        Console.WriteLine("No lecturer record found for this user");

                        claims = await _context.Claims
                            .Include(c => c.FeedbackMessages)
                            .Include(c => c.Lecturer)
                            .Where(c => c.Lecturer != null &&
                                       (c.Lecturer.Email == selectedLecturerUser.Email ||
                                        c.Lecturer.Name.Contains(selectedLecturerUser.Name)))
                            .OrderByDescending(c => c.CreatedAt)
                            .ToListAsync();

                        Console.WriteLine($"Found {claims.Count} claims using alternative search");
                    }
                }
            }

            return View(claims);
        }
    }
}