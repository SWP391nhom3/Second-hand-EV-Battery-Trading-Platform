using EVehicleManagementAPI.DBconnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EVehicleManagementAPI.Models;
using System.Text;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EVehicleDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// show detailed errors in Development to diagnose startup exceptions
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// quick health endpoint so you can confirm app started
app.MapGet("/", () => Results.Text("App running"));

app.MapControllers();

// Seed Staff role and a default Staff account on startup (idempotent)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EVehicleDbContext>();

    try
    {
        db.Database.Migrate();
    }
    catch
    {
        // ignore migrate errors in environments without DB permission
    }

    var staffRole = db.Roles.FirstOrDefault(r => r.Name == "Staff");
    if (staffRole == null)
    {
        staffRole = new Role { Name = "Staff", Status = "ACTIVE" };
        db.Roles.Add(staffRole);
        db.SaveChanges();
    }

    var staffEmail = "staff@demo.com";
    var existingStaff = db.Accounts.Include(a => a.Role).FirstOrDefault(a => a.Email == staffEmail);
    if (existingStaff == null)
    {
        string Hash(string s)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
            return Convert.ToBase64String(bytes);
        }

        var account = new Account
        {
            Email = staffEmail,
            Phone = "",
            PasswordHash = Hash("Staff@123"),
            RoleId = staffRole.RoleId,
            CreatedAt = DateTime.Now
        };
        db.Accounts.Add(account);
        db.SaveChanges();
    }
}

app.Run();
