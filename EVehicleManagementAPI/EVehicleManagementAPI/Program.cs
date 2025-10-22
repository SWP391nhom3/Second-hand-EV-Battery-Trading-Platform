using EVehicleManagementAPI.DBconnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EVehicleDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ✅ Thêm cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // địa chỉ FE (React/Vite)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // chỉ thêm nếu bạn dùng cookie/token
        });
});

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

// Thêm dòng này TRƯỚC khi map controller
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.UseAuthorization();

// quick health endpoint so you can confirm app started
app.MapGet("/", () => Results.Text("App running"));

app.MapControllers();
app.Run();
