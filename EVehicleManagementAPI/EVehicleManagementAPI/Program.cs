using EVehicleManagementAPI.DBconnect;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext
builder.Services.AddDbContext<EVehicleDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Thêm cấu hình CORS cho frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            // Lấy allowed origins từ configuration hoặc environment variable
            var allowedOrigins = builder.Configuration["CORS:AllowedOrigins"]?.Split(';') 
                ?? new[] { "http://localhost:5173", "https://localhost:5173", "http://localhost:3000", "https://localhost:3000" };
            
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ Kích hoạt CORS trước khi dùng Authorization
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
