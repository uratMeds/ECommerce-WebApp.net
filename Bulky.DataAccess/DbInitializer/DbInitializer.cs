using Bulky.DataAccess.Data;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bulky.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(UserManager<IdentityUser> userManager,
                             RoleManager<IdentityRole> roleManager,
                             ApplicationDbContext db)
        {   
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public void Initialize()
        {
            // Migrations if they are not applied.
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {

            }

            // Create roles if they are not created.
            if ( !_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult() )
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();  
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();

                // If roles are not created then we will create adin user as well.
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@dotnet.com",
                    Email = "admin@dotnet.com",
                    Name = "Mohamed El Masnaoui",
                    PhoneNumber = "1234567890",
                    StreetAddress = "8 DotNet St.",
                    State = "Texas",
                    PostalCode = "2001",
                    City = "Tetouan"
                }, "Admin123").GetAwaiter().GetResult();

                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@dotnet.com");
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }

            return;
        }

    }
}
