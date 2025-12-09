using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// הוספת DbContext לשירותים
// -------------------------
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ToDoDB"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ToDoDB"))
    ));

// -------------------------
// הגדרת JWT Authentication
// -------------------------
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();

// -------------------------
// הגדרת JSON options - שדות באותיות קטנות
// -------------------------
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// -------------------------
// הוספת Swagger
// -------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------------------------
// הגדרת CORS
// -------------------------
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "https://todolist-client-widkc.onrender.com",
            "http://localhost:3000",
            "http://localhost:5005"
        )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// -------------------------
// הפעלת Swagger
// -------------------------
app.UseSwagger();
app.UseSwaggerUI();

// -------------------------
// הפעלת CORS
// -------------------------
app.UseCors();

// -------------------------
// Authentication & Authorization
// -------------------------
app.UseAuthentication();
app.UseAuthorization();

// -------------------------
// Route בסיסי לבדיקה
// -------------------------
app.MapGet("/", () => "API is running!");

// -------------------------
// CRUD Routes (Protected)
// -------------------------

// שליפת כל המשימות
app.MapGet("/tasks", async (ToDoDbContext db) => await db.MyTables.ToListAsync())
    .RequireAuthorization();

// שליפת משימה לפי ID
app.MapGet("/tasks/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.MyTables.FindAsync(id);
    return item is not null ? Results.Ok(item) : Results.NotFound();
})
    .RequireAuthorization();

// הוספת משימה חדשה
app.MapPost("/tasks", async (MyTable newItem, ToDoDbContext db) =>
{
    db.MyTables.Add(newItem);
    await db.SaveChangesAsync();
    return Results.Created($"/tasks/{newItem.Id}", newItem);
})
    .RequireAuthorization();

// עדכון משימה
app.MapPut("/tasks/{id}", async (int id, MyTable updatedItem, ToDoDbContext db) =>
{
    var item = await db.MyTables.FindAsync(id);
    if (item is null) return Results.NotFound();

    item.Name = updatedItem.Name;
    item.IsComplete = updatedItem.IsComplete;

    await db.SaveChangesAsync();
    return Results.NoContent();
})
    .RequireAuthorization();

// מחיקת משימה
app.MapDelete("/tasks/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.MyTables.FindAsync(id);
    if (item is null) return Results.NotFound();

    db.MyTables.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
    .RequireAuthorization();

// -------------------------
// JWT Auth Routes
// -------------------------

app.MapPost("/auth/register", async (ToDoDbContext db, UserDto userDto, ILogger<Program> logger) =>
{
    logger.LogInformation($"Register attempt: {userDto.UserName}");

    if (string.IsNullOrWhiteSpace(userDto.UserName) || string.IsNullOrWhiteSpace(userDto.Password))
    {
        logger.LogWarning("Username or password empty");
        return Results.BadRequest("Username and password are required");
    }

    var exists = await db.Users.AnyAsync(u => u.UserName == userDto.UserName);
    if (exists)
    {
        logger.LogWarning($"User already exists: {userDto.UserName}");
        return Results.BadRequest("Username already exists");
    }

    var user = new User
    {
        UserName = userDto.UserName,
        PasswordHash = userDto.Password // לצורך הפשטות בלבד
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    logger.LogInformation($"User created: {userDto.UserName}");
    return Results.Ok("User created");
});

app.MapPost("/auth/login", async (ToDoDbContext db, UserDto loginDto) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == loginDto.UserName);

    if (user == null || user.PasswordHash != loginDto.Password)
    {
        return Results.Unauthorized();
    }

    var jwtSettingsLocal = app.Configuration.GetSection("Jwt");
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettingsLocal["Key"]!));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
        new Claim("userId", user.Id.ToString())
    };

    var token = new JwtSecurityToken(
        issuer: jwtSettingsLocal["Issuer"],
        audience: jwtSettingsLocal["Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: credentials
    );

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

    return Results.Ok(new { token = tokenString });
});

// -------------------------
// הפעלה
// -------------------------
app.Run();