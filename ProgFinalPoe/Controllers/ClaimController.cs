using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgFinalPoe.Data;

namespace ProgFinalPoe.Controllers
{
    public class ClaimController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ClaimController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        //method to display the feedback of the claims 
        public async Task<IActionResult> ClaimFeedback(int claimId)
        {
            var claim = await _context.Claims
                        .Include(c => c.FeedbackMessages)
                        .Include(c => c.Lecturer)
                        .FirstOrDefaultAsync(c => c.ClaimId == claimId);

            if (claim == null) return NotFound();
            return View(claim);
        }

        //method for coordinator and manager to be able to view the document uploaded
        public async Task<IActionResult> ViewDocument(int claimId)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null || string.IsNullOrEmpty(claim.SupportingDocument))
                return NotFound();

            var path = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", claim.SupportingDocument);
            var ext = Path.GetExtension(path).ToLowerInvariant();
            var contentType = ext switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream",
            };

            var bytes = await System.IO.File.ReadAllBytesAsync(path);
            return File(bytes, contentType, Path.GetFileName(path));
        }
    }
}