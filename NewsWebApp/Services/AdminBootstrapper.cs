
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewsWebApp.Data;

namespace NewsWebApp.Services;


//console code to make an admin user in the database
public static class AdminBootstrapper
{
    public static async Task<bool> RunAsync(IServiceProvider services, string email)
    {
        email = email.Trim();
        if (!new EmailAddressAttribute().IsValid(email))
        {
            Console.Error.WriteLine("Enter a valid email address.");
            return false;
        }

        if (Console.IsInputRedirected)
        {
            Console.Error.WriteLine("Run this command in an interactive terminal so the password stays out of command history.");
            return false;
        }

        await using var scope = services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Console.WriteLine("Checking the configured database for an existing account...");
        try
        {
            if (await userManager.FindByEmailAsync(email) is not null)
            {
                Console.Error.WriteLine("That email already belongs to a user. No role was changed.");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Database check failed: {ex.Message}");
            Console.Error.WriteLine("Check the connection and apply pending migrations before retrying.");
            return false;
        }

        Console.Write("First name: ");
        var firstName = Console.ReadLine()?.Trim();

        Console.Write("Last name: ");
        var lastName = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(firstName) || firstName.Length > 100 ||
            string.IsNullOrWhiteSpace(lastName) || lastName.Length > 100)
        {
            Console.Error.WriteLine("First and last name are required (100 characters maximum each).");
            return false;
        }

        var password = ReadPassword("Password: ");
        var confirmation = ReadPassword("Confirm password: ");
        if (password != confirmation || password.Length == 0)
        {
            Console.Error.WriteLine("Passwords did not match or were empty.");
            return false;
        }

        await using var transaction = await db.Database.BeginTransactionAsync();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole("Admin"));
            if (!roleResult.Succeeded)
                return PrintErrors(roleResult);
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        }
        ;

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            return PrintErrors(createResult);

        var roleAssignment = await userManager.AddToRoleAsync(user, "Admin");
        if (!roleAssignment.Succeeded)
            return PrintErrors(roleAssignment);

        await transaction.CommitAsync();
        Console.WriteLine($"Admin account created for {email}.");
        return true;
    }

    private static bool PrintErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
            Console.Error.WriteLine(error.Description);
        return false;
    }

    private static string ReadPassword(string prompt)
    {
        Console.Write(prompt);
        var characters = new List<char>();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
                break;
            if (key.Key == ConsoleKey.Backspace)
            {
                if (characters.Count > 0)
                    characters.RemoveAt(characters.Count - 1);
            }
            else if (!char.IsControl(key.KeyChar))
            {
                characters.Add(key.KeyChar);
            }
        }
        Console.WriteLine();
        return new string(characters.ToArray());
    }
}