using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Domain.Entities;
using RecyclingApp.Domain.ValueObjects;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RecyclingApp.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // 1. Seed Roles
        var roles = new[] { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Admin User
        var adminEmail = "admin@recycleapp.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                PhoneNumber = "+201000000000",
                Address = new Address("123 Admin St", "Cairo", "1A"),
                PointsBalance = 100
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // 3. Seed Regular User
        var userEmail = "user@recycleapp.com";
        var regularUser = await userManager.FindByEmailAsync(userEmail);
        if (regularUser == null)
        {
            regularUser = new ApplicationUser
            {
                UserName = userEmail,
                Email = userEmail,
                FullName = "John Doe",
                PhoneNumber = "+201111111111",
                Address = new Address("456 User Rd", "Giza", "12"),
                PointsBalance = 250
            };

            var result = await userManager.CreateAsync(regularUser, "User@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(regularUser, "User");
            }
        }

        // 4. Seed Categories
        if (!await context.Categories.AnyAsync())
        {
            var plastics = new Category("Plastics", "Recyclable plastic bottles, containers, and packaging materials.");
            var paper = new Category("Paper & Cardboard", "Newspapers, books, cardboard boxes, and office paper.");
            var metal = new Category("Metals", "Aluminum cans, steel food tins, and scrap metal objects.");
            var glass = new Category("Glass", "Glass bottles, jars, and other glass containers.");

            context.Categories.AddRange(plastics, paper, metal, glass);
            await context.SaveChangesAsync();

            // 5. Seed Products
            var petBottle = new Product("PET Plastic Bottle", "Standard plastic water/soda bottles.", new Money(1.50m), 500, 50, "PL-PET-01", plastics.Id);
            var hdpeJug = new Product("HDPE Milk Jug", "High-density polyethylene milk jugs.", new Money(2.20m), 200, 20, "PL-HDPE-02", plastics.Id);
            
            var cardboard = new Product("Corrugated Cardboard", "Flattened shipping boxes.", new Money(0.80m), 10, 30, "PP-CRD-01", paper.Id); // Low stock (10 <= 30)
            var newsPaper = new Product("Newspapers & Magazines", "Clean bundle of papers.", new Money(0.50m), 0, 15, "PP-NWS-02", paper.Id); // Out of stock

            var sodaCan = new Product("Aluminum Soda Can", "Clean aluminum drink cans.", new Money(3.00m), 1000, 100, "MT-ALM-01", metal.Id);
            var steelCan = new Product("Steel Food Can", "Rinsed steel soup/food cans.", new Money(1.80m), 600, 50, "MT-STL-02", metal.Id);

            var glassBottle = new Product("Glass Beverage Bottle", "Green or clear glass bottles.", new Money(2.50m), 300, 40, "GL-BOT-01", glass.Id);

            context.Products.AddRange(petBottle, hdpeJug, cardboard, newsPaper, sodaCan, steelCan, glassBottle);
            await context.SaveChangesAsync();

            // 6. Seed Recycling Transactions
            if (regularUser != null)
            {
                var transactions = new[]
                {
                    new RecyclingTransaction(regularUser.Id, petBottle.Id, 20, 30.00m),
                    new RecyclingTransaction(regularUser.Id, sodaCan.Id, 50, 150.00m),
                    new RecyclingTransaction(regularUser.Id, glassBottle.Id, 10, 25.00m),
                    new RecyclingTransaction(regularUser.Id, hdpeJug.Id, 15, 33.00m),
                    new RecyclingTransaction(regularUser.Id, steelCan.Id, 8, 14.40m)
                };

                context.RecyclingTransactions.AddRange(transactions);
                await context.SaveChangesAsync();
            }
        }
    }
}
