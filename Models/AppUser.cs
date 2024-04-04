using Microsoft.AspNetCore.Identity;

namespace AkilliFiyatWeb.Models
{
    public class AppUser: IdentityUser
    {
        public string? FullName { get; set; }
    }
}