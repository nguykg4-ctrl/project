using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ScreenWorking.Server.API.Data;
using ScreenWorking.Server.API.Services;
using ScreenWorking.Server.API.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database configuration: PostgreSQL in production, SQLite in local/dev
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=screenworking.db";
builder.Services.AddDbContext<CollaborationDbContext>(options =>
{
    if (connectionString.Contains("Host="))
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

// Authentication
string jwtSecret = builder.Configuration["Jwt:Secret"] ?? "ScreenWorkingSuperSecretKey2026!WithMinimum256BitsLength";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// Domain Services
builder.Services.AddSingleton<RoomManager>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<OperationLogService>();
builder.Services.AddSingleton<CollaborationWebSocketHandler>();

var app = builder.Build();

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseWebSockets();

// WebSocket Endpoint
app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var handler = context.RequestServices.GetRequiredService<CollaborationWebSocketHandler>();
        await handler.HandleConnectionAsync(context);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

app.MapControllers();

// Ensure DB Created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CollaborationDbContext>();
    db.Database.EnsureCreated();
}

app.Run();

public partial class Program { }
