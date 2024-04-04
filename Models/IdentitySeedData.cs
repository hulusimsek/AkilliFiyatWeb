using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AkilliFiyatWeb.Models
{
    public static class IdentitySeedData
    {
        private const string adminUser = "mrholmeess";
        private const string adminPassword = "mRHolmeess58_123";

        public static async void IdentityTestUser(IApplicationBuilder app)
        {
            var context = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<IdentityContext>();


            var userManager = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            var user = await userManager.FindByNameAsync(adminUser);

            if(user == null)
            {
                user = new AppUser {
                    FullName = "Hulusi Şimşek",
                    UserName = adminUser,
                    Email = "sivasliiii5885@gmail.com",
                    PhoneNumber = "1234"                    
                };

                await userManager.CreateAsync(user, adminPassword);
            }
        }
    } 
}