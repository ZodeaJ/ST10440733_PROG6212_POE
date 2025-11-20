using ProgFinalPoe.Models;

namespace ProgFinalPoe.Services
{
    public class SessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void SetUserSession(User user)
        {
            _httpContextAccessor.HttpContext.Session.SetInt32("UserId", user.UserId);
            _httpContextAccessor.HttpContext.Session.SetString("Username", user.Username);
            _httpContextAccessor.HttpContext.Session.SetString("Role", user.Role);
            _httpContextAccessor.HttpContext.Session.SetString("Name", user.Name);
        }

        public bool IsUserLoggedIn()
        {
            return _httpContextAccessor.HttpContext.Session.GetInt32("UserId") != null;
        }

        public string GetUserRole()
        {
            return _httpContextAccessor.HttpContext.Session.GetString("Role");
        }

        public int GetUserId()
        {
            return _httpContextAccessor.HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        public void ClearSession()
        {
            _httpContextAccessor.HttpContext.Session.Clear();
        }
    }
}