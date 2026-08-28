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

        // 3. Seed Regular Users (Customers)
        var user1Email = "user@recycleapp.com";
        var regularUser = await userManager.FindByEmailAsync(user1Email);
        if (regularUser == null)
        {
            regularUser = new ApplicationUser
            {
                UserName = user1Email,
                Email = user1Email,
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

        var user2Email = "sarah.connor@example.com";
        var customer2 = await userManager.FindByEmailAsync(user2Email);
        if (customer2 == null)
        {
            customer2 = new ApplicationUser
            {
                UserName = "sarah_connor",
                Email = user2Email,
                FullName = "Sarah Connor",
                PhoneNumber = "+201222222222",
                Address = new Address("789 Resistance Blvd", "Alexandria", "5"),
                PointsBalance = 480
            };

            var result = await userManager.CreateAsync(customer2, "User@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(customer2, "User");
            }
        }

        var user3Email = "michael.scott@dundermifflin.com";
        var customer3 = await userManager.FindByEmailAsync(user3Email);
        if (customer3 == null)
        {
            customer3 = new ApplicationUser
            {
                UserName = "michael_scott",
                Email = user3Email,
                FullName = "Michael Scott",
                PhoneNumber = "+201555555555",
                Address = new Address("1725 Slough Ave", "Scranton", "2B"),
                PointsBalance = 150
            };

            var result = await userManager.CreateAsync(customer3, "User@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(customer3, "User");
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
            var cardboard = new Product("Corrugated Cardboard", "Flattened shipping boxes.", new Money(0.80m), 10, 30, "PP-CRD-01", paper.Id); // Low stock
            var newsPaper = new Product("Newspapers & Magazines", "Clean bundle of papers.", new Money(0.50m), 0, 15, "PP-NWS-02", paper.Id); // Out of stock
            var sodaCan = new Product("Aluminum Soda Can", "Clean aluminum drink cans.", new Money(3.00m), 1000, 100, "MT-ALM-01", metal.Id);
            var steelCan = new Product("Steel Food Can", "Rinsed steel soup/food cans.", new Money(1.80m), 600, 50, "MT-STL-02", metal.Id);
            var glassBottle = new Product("Glass Beverage Bottle", "Green or clear glass bottles.", new Money(2.50m), 300, 40, "GL-BOT-01", glass.Id);

            context.Products.AddRange(petBottle, hdpeJug, cardboard, newsPaper, sodaCan, steelCan, glassBottle);
            await context.SaveChangesAsync();

            // 6. Seed Recycling Transactions (Orders)
            if (regularUser != null && customer2 != null && customer3 != null)
            {
                var transactions = new[]
                {
                    new RecyclingTransaction(regularUser.Id, petBottle.Id, 20, 30.00m, "Completed"),
                    new RecyclingTransaction(regularUser.Id, sodaCan.Id, 50, 150.00m, "Completed"),
                    new RecyclingTransaction(regularUser.Id, glassBottle.Id, 10, 25.00m, "Pending"),
                    new RecyclingTransaction(regularUser.Id, hdpeJug.Id, 15, 33.00m, "Completed"),
                    new RecyclingTransaction(regularUser.Id, steelCan.Id, 8, 14.40m, "Cancelled"),

                    new RecyclingTransaction(customer2.Id, sodaCan.Id, 100, 300.00m, "Completed"),
                    new RecyclingTransaction(customer2.Id, petBottle.Id, 60, 90.00m, "Completed"),
                    new RecyclingTransaction(customer2.Id, glassBottle.Id, 40, 100.00m, "Completed"),

                    new RecyclingTransaction(customer3.Id, cardboard.Id, 50, 40.00m, "Completed"),
                    new RecyclingTransaction(customer3.Id, petBottle.Id, 30, 45.00m, "Pending"),
                    new RecyclingTransaction(customer3.Id, steelCan.Id, 25, 45.00m, "Completed")
                };

                context.RecyclingTransactions.AddRange(transactions);
                await context.SaveChangesAsync();
            }
        }
    }
}
